# Architecture

The trainer is one net46, C# 6 assembly (`Trainer_v5.dll`) that runs inside
the game. It has no entry point of its own: the game calls it through mod
callbacks, Unity scene events and time events. It uses only public game types
from the vendored assemblies in `lib/SoftwareInc/`, with no
reflection or patches. Controls are described in [`features.md`](features.md).

`Helpers.Version` is the release version. `GameVersionCheck` holds the target
game build (Beta 1.8.42). A different running version is logged, never
blocked. After refreshing `lib/SoftwareInc/` (including `mcs.dll`), update
those constants; `GameVersionCheckTests` fails until they match.

## Components

| Part | Role |
|---|---|
| `Main` | Mod entry, Options screen, Trainer button, save data |
| `TrainerBehaviour` | Scene and time events, toggle transitions, `Cheats.*` flags. Raises triggers; owns no feature logic |
| `FeatureRegistry`, `FeatureDispatcher`, `FeatureGuard`, `ToggleStateTracker` | Schedule, runner, error boundary, toggle-change detection |
| `Features/*Features` | Game-state changes and one-shot actions, per domain |
| `SettingKeys` | Every saved setting, its save key and default |
| `SettingsWindow`, `*WindowTrainer`, `Window/` | Trainer window, native-window toolbars, editors |
| `Confirmation`, `Notification`, `InputHelper` | Dialogs, messages and input |
| `Experimental/` | Code that runs only while the Experimental option is on |

## Lifecycle

- Entering `MainScene` subscribes to the game's time events first, then
  installs the UI. A UI failure therefore cannot leave events unsubscribed.
  The menu tears both down.
- Entering and leaving a game reset per-game state (window references,
  specialization list, hosting flags, printer baselines). Saved settings are
  not reset.
- `GameSettings.GameReady` runs per-game setup: toolbars, specializations, the
  hosting-deal repair, and clearing experimental toggles when the option is
  off. It is not gated, because the game raises it just before
  `SelectorController.DoneLoading` is set.
- Scheduled work waits for `Helpers.IsSimulationReady()`: game singletons
  exist, loading is done and pre-simulation has finished. Earlier events are
  dropped, not replayed.
- `GameSettings.MaxFloor` is set before that gate, because the game reads it
  while rooms load.
- Buttons call `Helpers.RequireGameReady` and tell the player when nothing
  happened.

## Scheduling and errors

Each `FeatureRegistry` row has an id, a toggle, a trigger and an action.
Triggers are Frame, Minute, Hour, Day and Month while the toggle is on, and
Enabled/Disabled once when it changes.

- The game has no minute event; `GameMinuteWatcher` creates one.
- Frame rows are only for values the game overwrites faster than once a
  minute. They must not allocate: use index loops, no LINQ. `HashList`'s
  `foreach` boxes an enumerator.
- A periodic row also has an Enabled row, so it acts as soon as it is switched
  on, unless it is listed in `FeatureRegistry.NoCatchUp`.
- A Disabled (restore) row needs an Enabled row.
- `Cheats.*` flags have no save backing, so they are pushed on the first frame
  and on every transition.

`FeatureDispatcher` runs every row through `Helpers.TryExecute`. One feature is
one error boundary: a failure does not stop other features and is
rate-limited in the log. Feature classes do not catch or schedule themselves.
A failed Enabled/Disabled entry is retried until the toggle's next transition,
even across a game change. These actions must therefore be idempotent writes
or catch-up scans.

In the **Workshop build**, the game wraps every method body in its own
handler. An exception there shows the game's error window and unloads the mod
before `FeatureGuard` sees it. Only a failure written directly in an inline
lambda reaches the guard (`WorkshopExceptionBoundaryTests`). This is not
verified in the game.

## Toggle transitions

Every save load is a cold start. An enabled toggle runs its Enabled row. A
toggle that was on before leaving the game and is off in the next one still
runs its restore. This matters for state the game keeps across loads
(`AI.MaxBoxes`, `EdCost`, `Cheats.*`).

A toggle restores only when the normal value is known safely:

- a verified constant (`ProductPrinter.PrintPrice` is 0.1);
- a baseline captured before the first write (per-printer `PrintSpeed`, keyed
  by Unity instance id so no game object is kept);
- the game's own recalculation.

Other toggles have no Disabled row and leave their changes; see
[Switching toggles off](features.md#switching-toggles-off). They write
furniture fields that are not saved (lost on reload), saved state with no
recorded original, or values the game rewrites at an unknown time.

## Settings and saves

Settings are stored in the save game by `Main.Serialize` and `Deserialize`.

- Each setting is declared once in `SettingKeys`. Code uses `SettingKeys.X`,
  never a string, so a typo fails to compile.
- **The key string is the save key. Never rename or reuse it.** Changing it
  resets that setting for every player.
- `ToggleKey` and `StoreKey` are not generic, because the game's compiler emits
  invalid IL for fields of generic types.
- A full load gives every missing key its default, so values never carry over
  from the previous save. Building and company loads keep the current values.
- The game calls `Deserialize` only when the save has a trainer entry.
  `SettingsSession` detects other games by their `GameSettings` instance and
  applies their `ModData` entry or the defaults.
- Retired settings use `SettingKeys.Retired(...)`. The name stays reserved, it
  is no longer saved or loaded, and a `true` in an old save is ignored.

**Mod options.** *Enable Experimental Features* and *Debug Logging* are
stored by the game in `DLLMods/<mod>Setting.txt` (`DLLMod.Settings`), not in
saves. `Main.Initialize` reads them when the mod loads. Only a stored `True`
turns an option on. Experimental toggles are listed in `ExperimentalToggles`
and switch off when the option is off, including on game load.

**Logging.** `Logger.LogDebug` writes only to the dev console, and every call
is inside `if (LoggingOptions.DebugEnabled)`. Traces must not use
`UnityEngine.Debug.Log*`: the game caps `Player.log` and counts logged errors
toward a limit that can pause the game.

## Adding a toggle

1. Declare it in `SettingKeys` and add it to
   `SettingsTests.PersistedToggleKeys`.
2. Add a `SettingToggleRows` row and its key to the golden list in
   `SettingToggleRowsTests`.
3. Add its text to `assets/Localization/English/Trainer.tyd`.
4. Add `FeatureRegistry` rows: the cadence row, an Enabled row and, if it
   restores, a Disabled row. Put the action in a `Features/*Features` class.
5. Record what switching off leaves in `features.md`.
6. To retire it, declare it with `Retired(...)` and remove its UI and rows.
   Never rename the key.

The settings, serialization, row, localization and schedule tests fail when a
step is missed.

## Workshop source build

The distributed layout does not follow the repository folders. The Workshop
item holds `Trainer.Source/` (from `src/Trainer.Source/`) and
`Trainer.Localization/` (from `assets/Localization/`). The DLL zip holds
`Trainer_v5.dll` and `Trainer.Localization/`.

The Workshop item is the trainer's source. When the game loads it,
`ModController.InjectTryBlocks` adds `#define` lines, strips comments and
wraps every method body in `try`/`catch`. The game's C# 6 compiler (`mcs`)
then builds it. The SDK build cannot predict this, so the source must follow
these rules:

- C# 6 only; `mcs` rejects newer syntax.
- No call in a field or property initializer followed by a property with
  accessors. The injector mistakes the `(` for a method and rewrites the wrong
  brace. Assign in a static constructor instead.
- No `'{'` or `'}'` char literals, and no apostrophe in a string before a
  comment that contains a brace on the same line.
- No parameter or local named `ex` in a block-bodied method; it collides with
  the injected catch variable.
- No fields in generic classes or structs (invalid IL at runtime).
- No method that ends in a `throw` without any `return`; the injected `try`
  makes the compiler emit a branch past the end of the method.
- The game compiles the files unsorted. An enum used as a field type must be
  compiled before the type using it: declare it in a file of the same folder
  that sorts earlier (for example `DomainEnums.cs`) or above that type.
  Otherwise the mod fails to load.

`GameSourcePipelineTests`, `GameCompilerCompatibilityTests` and
`GameCompilerHazardTests` run the game's injector and `mcs` on the source and
report errors by repository file and line. They check parsing only. The SDK
build checks type resolution. Runtime IL problems, compile order and the
game's error handler need the game.

## UI and safety

- All UI uses the game's `WindowManager` widgets.
- `SettingsWindow` is built once per game; tabs only show and hide panels, so
  switching keeps entered values.
- Refreshing controls sets a `_refreshing` guard, because the game's toggles
  always raise change events.
- Each toolbar adds its buttons once per native window and removes a partial
  set on failure. Toolbars are never behind the Experimental option.

**Targeting.** Toolbar actions use only the rows selected in the native list.
The target is captured on click and checked again before any delayed write.
Names are never used to find it.

- Employees: `EmployeeTarget`; single-employee editors need exactly one.
- Products: one live software product owned by the player.
- Companies: one live AI company, not the player's, not a subsidiary, not
  going bankrupt.

**Confirmations.** Only high-impact actions ask, through `Confirmation.Ask`.
On Yes, the game and the exact targets are checked again. If anything changed,
nothing is applied. `ConfirmationTests` pins this set; update it with this
list:

- AI Bankrupt All, Bankrupt and a negative Adjust AI Funds
- Clear all loans, Unlock and Claim Rewards, Max Reputation
- Founder, Make All Employees Happy Now and Make Happy
- Optimize Team Assignments
- Experimental: Amazing Compatibility, Skip 1 Day, Skip To Hour, Hire
  Employees and Fire Quietly

**Messages.** Only `Notification` calls the game's popup and dialog APIs.
Popups start with `Trainer: ` once and use `PopupIDs.None`, the only id the
game shows.

**Game API limits.**

- A negative `Company.MakeTransaction` that an AI company cannot cover
  bankrupts and buys it out at once.
- `SoftwareProduct.ChangePrice` refuses prices below 1, so the price is
  written directly.
- Demands are granted with `AcceptDemand`, so their effect and cost apply.
  Removal changes `DemandResults`, `DemandsMet` and `DemandsRequested`
  together, because the detail window shows demands only while `DemandsMet` is
  set.
- `Actor.Fire` skips the coworker `TeammateFired` mood, `NoteFiring` and the
  Negligence lawsuit check when `QuitAmicably` is set. Fire Quietly calls
  `NoteFiring` itself and runs only when that lawsuit is impossible
  (employed 12 months or less, or `ComplaintLevel` at most 1).
- `PatentNow` charges without checking funds, so funds, expiry and ownership
  are checked first.

## Localization

- Text uses `"Key".LocDef("English text")`. The key goes in
  `English/Trainer.tyd` under the section matching its use.
- Other languages may omit keys; English is shown instead.
- A text used in more than one place is a method in `SharedMessages`.
- Placeholders are filled with `string.Format` at the `LocDef` call.
- `LocDef` cannot run in the test host, so pure helpers return values and the
  text is built where it is shown.
- `LocalizationTests` enforces these rules.

## Validation

- CI builds and tests `Release` only.
- `Trainer_v5.Tests` (xUnit, `net462`) runs trainer code against the vendored
  assemblies outside Unity: settings and saves, input validation, scheduling,
  transitions, localization and the Workshop source pipeline. Code that
  touches live game objects needs in-game testing.

Open technical debt is tracked as GitHub issues labelled `type: tech-debt`.
