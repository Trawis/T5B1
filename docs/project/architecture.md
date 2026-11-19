# Architecture

The trainer is one net46, C# 6 assembly (`Trainer_v5.dll`). It runs inside the
game. The game calls it through mod callbacks, scene events and time events.
It uses only public game types from `Trainer_v5/Trainer.Libraries/`. It uses
no reflection and no patches. Controls are listed in
[`features.md`](features.md).

`Helpers.Version` is the release version. `GameVersionCheck` holds the target
game build (Beta 1.8.42). Another running version is logged, not blocked.
After refreshing `Trainer.Libraries/`, update those constants;
`GameVersionCheckTests` fails until they match.

## Components

| Part | Role |
|---|---|
| `Main` | Mod entry, Options screen, Trainer button, save data |
| `TrainerBehaviour` | Scene and time events, toggle transitions, `Cheats.*` flags |
| `FeatureRegistry`, `FeatureDispatcher`, `FeatureGuard`, `ToggleStateTracker` | Schedule, runner, error boundary, toggle changes |
| `Features/*Features` | Game changes and one-shot actions |
| `SettingKeys` | Every saved setting and its default |
| `SettingsWindow`, `*WindowTrainer`, `Window/` | Trainer window, toolbars, editors |
| `Confirmation`, `Notification`, `InputHelper` | Dialogs and input |
| `Experimental/` | Code that runs only with the Experimental option |

## Lifecycle

- Entering `MainScene` subscribes to time events first, then installs the UI.
  The menu removes both.
- Entering a game and leaving it reset per-game state. Saved settings are not
  reset.
- `GameSettings.GameReady` installs toolbars and runs per-game setup. It is
  not gated, because the game raises it just before `DoneLoading`.
- Scheduled work runs only when `Helpers.IsSimulationReady()` is true. Earlier
  events are dropped.
- Buttons call `Helpers.RequireGameReady` and explain when nothing happened.

## Scheduling

Each `FeatureRegistry` row has an id, a toggle, a trigger and an action.

- Triggers: Frame, Minute, Hour, Day, Month, and Enabled/Disabled when a
  toggle changes.
- `GameMinuteWatcher` creates the minute event the game lacks.
- Frame rows are only for values the game overwrites quickly. They must not
  allocate: use index loops, no LINQ.
- Each periodic row has an Enabled row, unless listed in
  `FeatureRegistry.NoCatchUp`.
- A Disabled (restore) row needs an Enabled row.
- `Cheats.*` flags are not saved. They are pushed on the first frame and on
  every transition.

`FeatureDispatcher` runs each row through `Helpers.TryExecute`. One failing
feature does not stop others. Failures are rate-limited in the log. A failed
Enabled/Disabled entry is retried until the next transition. These actions
must be idempotent.

**Workshop build:** the game wraps every method body in its own handler. An
exception there unloads the mod before `FeatureGuard` sees it. Only code in an
inline lambda reaches the guard. This is not verified in the game.

## Toggle transitions

Every save load is a cold start. An enabled toggle runs its Enabled row. A
toggle that was on before and is now off runs its restore.

A toggle restores only when the normal value is known: a fixed constant, a
baseline captured before the first write, or the game's own recalculation.
Other toggles leave their changes. See
[Switching toggles off](features.md#switching-toggles-off).

## Settings and saves

Settings are saved in the save game by `Main.Serialize` and `Deserialize`.

- Each setting is declared once in `SettingKeys`. Code uses `SettingKeys.X`,
  never a string.
- **The key string is the save key. Never rename or reuse it.**
- `ToggleKey` and `StoreKey` are not generic: the game's compiler emits
  invalid IL for fields of generic types.
- A full load gives missing keys their default. Building and company loads keep
  current values.
- The game calls `Deserialize` only if the save has trainer data.
  `SettingsSession` detects other games and applies defaults.
- Retired keys use `SettingKeys.Retired(...)`. They stay reserved, are not
  saved, and old values are ignored.

**Mod options.** *Enable Experimental Features* and *Debug Logging* are stored
by the game in `DLLMods/<mod>Setting.txt`, not in saves. Only `True` enables
them. Experimental toggles are listed in `ExperimentalToggles`. They switch
off when the option is off.

**Logging.** `Logger.LogDebug` writes only to the dev console. Every call is
inside `if (LoggingOptions.DebugEnabled)`. Do not use `Debug.Log*` for traces:
the game limits `Player.log` and counts logged errors.

## Adding a toggle

1. Declare it in `SettingKeys` and add it to `SettingsTests`.
2. Add a `SettingToggleRows` row and the golden list in
   `SettingToggleRowsTests`.
3. Add its text to `Trainer.Localization/English/Trainer.tyd`.
4. Add `FeatureRegistry` rows: cadence, Enabled, and Disabled if it restores.
5. Document what switching off leaves in `features.md`.
6. To retire it, use `Retired(...)`. Never rename the key.

The setting, serialization, row, localization and schedule tests fail when a
step is missing.

## Workshop source build

The Workshop item is source code. The game adds `#define` lines, strips
comments, wraps every method body in `try`/`catch` and compiles with its own
C# 6 compiler (`mcs`). The source must follow these rules:

- C# 6 only.
- No call in a field or property initializer followed by a property with
  accessors. Assign in a static constructor instead.
- No `'{'` or `'}'` char literals. No apostrophe in a string before a comment
  with a brace on the same line.
- No parameter or local named `ex` in a block-bodied method.
- No fields in generic classes or structs.
- No method that ends in `throw` without any `return`.
- Files are compiled unsorted. An enum used as a field type must come first:
  put it in an earlier-sorting file such as `DomainEnums.cs`, or above the type
  in the same file. Otherwise the mod fails to load.

`GameSourcePipelineTests`, `GameCompilerCompatibilityTests` and
`GameCompilerHazardTests` run the game's injector and `mcs` on the source.
They prove parsing only. The SDK build proves type resolution. Runtime IL
problems and the error handler need the game.

## UI

- UI is built from the game's `WindowManager` widgets.
- `SettingsWindow` is built once per game. Tabs only show and hide panels.
- Refreshing controls uses a `_refreshing` guard, because the game's toggles
  always raise change events.
- Each toolbar adds its buttons once per native window and removes a partial
  set on failure.
- Toolbars are never behind the Experimental option.

**Targeting.** Toolbar actions use only the rows selected in the native list.
The target is captured on click and checked again before a delayed write.

- Employees: `EmployeeTarget`, exactly one for single-employee editors.
- Products: one live software product you own.
- Companies: one live AI company, not yours, not a subsidiary, not bankrupt.

**Confirmations.** Only high-impact actions ask, through `Confirmation.Ask`.
On Yes, the game and the exact targets are checked again. If anything changed,
nothing is applied. `ConfirmationTests` pins the set:

- AI Bankrupt All, Bankrupt, negative Adjust AI Funds
- Clear all loans, Unlock and Claim Rewards, Max Reputation
- Founder, Make All Employees Happy Now, Make Happy
- Optimize Team Assignments
- Experimental: Amazing Compatibility, Skip 1 Day, Skip To Hour, Hire
  Employees

**Messages.** Only `Notification` calls the game's popup and dialog APIs.
Popups start with `Trainer: ` once and use `PopupIDs.None`. Other ids are not
shown.

**Game API limits.**

- A negative `Company.MakeTransaction` that an AI company cannot cover
  bankrupts it.
- `SoftwareProduct.ChangePrice` refuses prices below 1, so price is written
  directly.
- Demands are granted with `AcceptDemand`. Removal changes `DemandResults`,
  `DemandsMet` and `DemandsRequested` together.

## Localization

- Text uses `"Key".LocDef("English")`.
- Keys go in `English/Trainer.tyd` under the matching section.
- Other languages may omit keys; English is shown.
- Shared texts live in `SharedMessages`.
- Placeholders use `string.Format` at the `LocDef` call.
- `LocDef` cannot run in tests, so pure helpers return values instead of
  text.
- `LocalizationTests` enforces these rules.

## Validation

- CI builds and tests `Release` only.
- `Trainer_v5.Tests` (xUnit, `net462`) runs trainer code against the vendored
  assemblies outside Unity: settings and saves, input validation, scheduling,
  transitions, localization and the Workshop source pipeline. Code that
  touches live game objects needs in-game testing.

Open technical debt is tracked as GitHub issues labelled `type: tech-debt`.
