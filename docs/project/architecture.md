# Architecture

T5B1 is a client-side trainer for *Software Inc.* Beta 1: one net46 / C# 6
assembly (`Trainer_v5.dll`) that runs inside the game process. It has no entry
point of its own; the game drives it through mod callbacks, Unity scene events
and time events. It uses only the game's public types from the vendored
assemblies in `Trainer_v5/Trainer.Libraries/` (no reflection, patches or
compatibility layer). User-visible behaviour is in [`features.md`](features.md).

Versions: `Helpers.Version` is the release version. `GameVersionCheck`
holds the targeted game build (Beta 1.8.42); a different running version is
shown in the window title and logged, never blocked. When refreshing
`Trainer.Libraries/` (including `mcs.dll`), update those constants;
`GameVersionCheckTests` fails until they match.

## Components

- `Main`: mod entry point, Options screen, toolbar button, save
  serialization.
- `TrainerBehaviour`: scene and time events, toggle transitions and the
  `Cheats.*` flags; it raises triggers and owns no feature logic.
- `FeatureRegistry`, `FeatureDispatcher`, `FeatureGuard`,
  `ToggleStateTracker`: the schedule, its runner, the error boundary and toggle
  change detection.
- `Features/*Features`: the game-state changes and one-shot actions, per
  domain (Company, Employee, Product, Market, WorkItem, Environment).
- `SettingKeys`: every persisted setting with its save key and default.
- UI: `SettingsWindow` with `SettingActionRows`/`SettingToggleRows`, the
  toolbars `EmployeesWindowTrainer`, `ProductsWindowTrainer` and
  `CompaniesWindowTrainer`, the editors in `Window/`, and `UIFactory`,
  `UIHelper`, `InputHelper`, `Confirmation` and `Notification`.
- `Experimental/`: code that runs only while the Experimental option is on.

## Runtime lifecycle

- `SceneRoute` maps Unity scene loads: entering `MainScene` subscribes to the
  game's time events (before any UI is installed), then installs the trainer
  UI; the menu tears both down.
- Entering the main scene and leaving a game reset per-game state
  (`TrainerBehaviour.ResetPerGameState`: window references, specialization
  list, hosting flags, printer baselines). Persisted settings are not touched.
- `GameSettings.GameReady` runs per-game initialization (toolbars,
  specializations, hosting-deal repair, clearing experimental toggles when the
  option is off). It is not gated on readiness: the game raises it just before
  `SelectorController.DoneLoading` is set.
- All scheduled work waits for `Helpers.IsSimulationReady()` (singletons
  present, `DoneLoading` set, pre-simulation neither running nor finishing).
  Earlier events are dropped, not replayed. User actions check
  `Helpers.RequireGameReady` and say why nothing happened.

## Scheduling and error isolation

Scheduled features are `FeatureRegistry` rows: an id, the toggle that gates
it, a trigger and an action. Triggers are Frame, Minute, Hour, Day and Month
(while on) and Enabled/Disabled (once, when the toggle changes). The game has
no minute event, so `GameMinuteWatcher` synthesizes one. Frame rows are only
for values the game overwrites faster than once a minute and must be
allocation-free (index loops, no LINQ; `HashList`'s `foreach` boxes).

A periodic row also has an Enabled row so it acts as soon as it is switched
on, unless its id is in `FeatureRegistry.NoCatchUp`. A Disabled (restore) row
needs an Enabled row. `Cheats.*` flags (Force Lights, Show Room Ceilings,
Unlimited Subsidiaries) have no save backing and are pushed on the first frame
and on every transition.

`FeatureDispatcher` runs each row through `Helpers.TryExecute(featureId,
action)`: one feature is one error boundary, failures are rate-limited in the
log, and a failing feature does not stop the others. Domain classes do not
catch, log or schedule themselves. A failed Enabled/Disabled entry is retried
(every frame for 60 frames, then every 60th) until the toggle's next
transition, and survives the per-game reset; therefore these actions must be
idempotent writes or catch-up scans.

In the **Workshop build** this isolation is weaker: the game's injected
handler wraps every named method body and, on an exception, shows its error
window and unloads the mod before `FeatureGuard` sees it. Only a failure
written directly in an inline lambda reaches the guard
(`WorkshopExceptionBoundaryTests` simulate this). Not verified in the game.

## Toggle transitions

Toggle states are observed per game, so every save load is a cold start: an
enabled toggle runs its Enabled row, and a toggle that was on before leaving
the game and is off in the next one still runs its restore. This matters for
state the game keeps across loads (`AI.MaxBoxes`, `EdCost`, `Cheats.*`).

A toggle restores only when the normal value can be recovered safely: a
verified constant (`ProductPrinter.PrintPrice` 0.1), a baseline captured before
the first write (per-printer `PrintSpeed`, keyed by Unity instance id) or the
game's own recalculation. Other toggles have no Disabled row and leave their
changes; [`features.md`](features.md#switching-toggles-off) lists the result.
In short, they write furniture fields that are not saved and have no game
writer (lost on reload), saved state without a recorded original (salaries,
birth dates, vacations, `ForgetfulETA`, finished work, refunds), or values the
game rewrites itself at an undetermined time.

## Persistence

Settings are saved in the save game through `Main.Serialize`/`Deserialize`
(`WriteDictionary`). Every setting is declared once in `SettingKeys` as a
`ToggleKey` or `StoreKey` (not generic: the game's compiler emits invalid IL
for fields of user-defined generic types); `Helpers.Settings` and
`Helpers.StoresSettings` are built from it, and code refers to
`SettingKeys.X`, never a string. **The key string is the save key and must
never change or be reused.**

- A full load gives every key the save lacks its declared default, so values
  never carry over from the previous save; building and company loads keep the
  values in use.
- The game calls `Deserialize` only when the save has a trainer entry, so
  `SettingsSession` detects a new game by its `GameSettings` instance id and
  applies its `ModData` entry or the defaults.
- Retired settings (`ReduceBoxPrice`, `AutoMaxMarketShare`, `MoreCreativity`,
  `ReduceExpansionCost`, `DisableSkillDecay`, the old per-save `Experimental`)
  are declared with `SettingKeys.Retired(...)`: the name stays reserved, but
  they are not written or loaded, so a stored `true` is ignored.

### Mod options

*Enable Experimental Features* and *Debug Logging* are stored by the game in
`DLLMods/<mod>Setting.txt` (`DLLMod.Settings`, rewritten by
`DLLMod.SaveSetting`), not in saves. `Main.Initialize` reads them when the mod
loads; `Main.ConstructOptionsScreen` builds the switches. Only a stored `True`
enables either option.

Experimental code lives in `Trainer.Source/Experimental/` and checks
`ExperimentalOptions.Enabled`. Experimental toggles are normal persisted
toggles whose rows are in `SettingToggleRows.CreateExperimental()` and are
listed in `ExperimentalToggles`; turning the option off, or loading a game
while it is off, switches them off. Experimental actions are rows in
`SettingActionRows.CreateExperimental()`. The settings window rebuilds its
content in place when the option changes.

### Logging

Normal logging (`Logger.Log`, feature failures through `TryExecute`) is always
on. `Logger.LogDebug` writes `Trainer [debug]:` lines to the dev console only,
and every call is wrapped in `if (LoggingOptions.DebugEnabled)`. Trace output
must not use `UnityEngine.Debug.Log*`: the game caps `Player.log` and counts
logged errors toward a limit that can pause the game. Logging code must stay
exception-free because the Workshop handler wraps it too.

## Adding a persistent toggle

1. Declare `public static readonly ToggleKey X = Toggle("X");` in
   `SettingKeys` and add it to `SettingsTests.PersistedToggleKeys`.
2. Add a `SettingToggleRows` row (domain, localization key, English text, key)
   and the key to the golden list in `SettingToggleRowsTests`. A toggle with an
   immediate side effect goes in `CreateImmediate` and
   `SettingsWindow.OnToggleChanged`.
3. Add the localization key to `Trainer.Localization/English/Trainer.tyd`
   (see Localization).
4. Add `FeatureRegistry` rows: the cadence row, an Enabled row (or a
   `NoCatchUp` entry) and, if it restores, a Disabled row. The action lives in a
   `Features/*Features` class.
5. Document what switching off leaves in `features.md`.
6. To retire a toggle, change its declaration to `Retired(...)`, remove its UI
   and rows, add it to `RetiredSettingsTests` and never rename the key.

`SettingKeysTests`, `SettingsTests`, `SerializationTests`,
`SettingToggleRowsTests`, `LocalizationTests`, `FeatureScheduleTests`,
`FeatureDispatcherTests` and `RetiredSettingsTests` fail when a step is
missed.

## Workshop source build

The Steam Workshop item is the trainer's **source**. When the game loads it,
`ModController.InjectTryBlocks` adds three `SWINC*` `#define` lines, strips
comments and wraps every method and constructor body in `try`/`catch`; then
the game's bundled C# 6 compiler (`mcs`) compiles it. The SDK build cannot
predict whether that works, so the source must follow these rules:

- **C# 6 only**; `mcs` rejects C# 7 syntax (CS1644).
- No call in a property or field initializer followed by a property with
  accessors: the injector treats a member-level `(` as a method and rewrites
  the wrong brace. Assign in a static constructor instead.
- No `'{'` or `'}'` char literals, and no apostrophe in a string before a
  comment containing a brace on the same line.
- No parameter or local named `ex` in a method with a block body (collides
  with the injected catch variable, CS0136).
- No fields in user-defined generic classes or structs (invalid IL at
  runtime).
- No method or constructor that ends in a `throw` without any `return`: the
  injected `try` leaves the compiler emitting a branch past the end of the
  method.
- The game lists the source files with `Directory.GetFiles` (all
  directories) and does not sort them. An enum used as a field type must be
  compiled before the type that uses it: declare it in a file of the same
  folder that sorts before the user (for example `DomainEnums.cs`) or above
  that type in the same file. Otherwise
  the game fails to load the mod ("Field '...' is an enum type with a bad
  underlying type"). The tests cannot catch this.

`GameSourcePipelineTests`, `GameCompilerCompatibilityTests` and
`GameCompilerHazardTests` run the game's own injector and `mcs` over
`Trainer.Source` and report errors against the repository file and line. The
tests call the injector's private methods by reflection (tests only), and
`GameSourceInjector.VerifyMirror` fails when the game's `InjectTryBlocks`
changes. `Trainer.Libraries/mcs.dll` comes from the game's `Managed` folder
and is used only by the tests (loaded from a copy with the 32-bit flag
cleared).

The tests do not prove type resolution (the SDK build does), runtime IL
validity in Unity, or the game's error handler at runtime; those need the
game.

## UI

All UI is built from the game's `WindowManager` widgets. `SettingsWindow` is
built once per game; its three tabs are panels switched with `SetActive`, so
switching creates nothing and keeps entered values. Showing it again sets the
controls from the stored settings under a `_refreshing` guard, because the
game's toggles have no set-without-notify.

The toolbars above the Employees (`HUD.employeeWindow`), Products
(`HUD.PlayerProductWindow`) and Companies (`HUD.companyWindow`) windows are
one class each: `Install` adds the buttons once per live window and removes a
partial set on failure; `Reset` forgets the window when the game is left.
Toolbars are never gated behind the Experimental option.

**Targeting.** Toolbar actions act only on rows selected in the native list,
captured at the click and checked again before any deferred write:

- Employees: `EmployeeTarget` (exactly one selected for the single-employee
  editors; Skills and Make Happy take every selected employee).
- Products: one live `SoftwareProduct` owned by the player in the list's
  software mode.
- Companies: one `SimulatedCompany` that is a live AI company
  (`CompanyFeatures.IsLiveAICompany`), not a subsidiary and not bankrupt.

**Game API notes.** `Company.MakeTransaction` with a negative amount that the
AI company cannot cover bankrupts and buys it out at once, so Adjust AI Funds
confirms negative amounts. Product price is written directly because
`SoftwareProduct.ChangePrice` refuses prices below 1. Demands are granted
through `AcceptDemand` and removed by changing `DemandResults`, `DemandsMet` and
`DemandsRequested` together, because the detail window shows demands only while
`DemandsMet` is set.

**Messages.** `Notification` is the only code that calls the game's popup and
dialog APIs. Popups start with `Trainer: ` exactly once and use
`PopupIDs.None` (other ids are not shown) with a non-Issue sound; dialogs and
input prompts have no prefix.

**Confirmations.** Routine and value-entry actions do not ask. The
high-impact set asks through `Confirmation.Ask` (the game's Yes/No message box
without a "Don't ask again" id): AI Bankrupt All, Clear all loans, Unlock and
Claim Rewards, Bankrupt (typed or toolbar), negative Adjust AI Funds, Max
Reputation, Founder, Make All Employees Happy Now, Make Happy, Optimize Team
Assignments, and the experimental Amazing Compatibility, Skip 1 Day, Skip To
Hour and Hire Employees. On Yes the game and the exact targets are checked
again (`Confirmation.IsSameTargets`, `IsScopeUnchanged`); if anything changed,
nothing is applied. `ConfirmationTests` pins this set; update it and this list
together.

## Localization

User-visible text uses `"Key".LocDef("English default")`, with the key in
`Trainer.Localization/English/Trainer.tyd` under the section matching its use
(`#Input fields`, `#Actions`, `#Toggles`, `#Labels`, `#Dialogs`, `#Errors`,
`#Notifications`). Other languages may omit keys; the English default is shown.
A text used from more than one place is a method in `SharedMessages`.
Placeholders are filled with `string.Format` at the `LocDef` call. `LocDef`
cannot run in the test host (the game's `Localization` type fails to
initialize), so pure helpers return values and the text is built where it is
shown. Log lines and internal names stay English. `LocalizationTests` enforces
these rules.

## Validation

- CI builds and tests `Release` only; `dotnet format` and `dotnet build` are
  the local static checks. A few project-owned warnings are errors in
  `Trainer_v5.csproj`; they do not affect the game's Workshop compile.
- `Trainer_v5.Tests` (xUnit, `net462`) runs trainer code against the vendored
  assemblies outside Unity: settings and saves, input validation, scheduling,
  transitions, localization and the Workshop source pipeline. Code that
  touches live game objects needs in-game testing.

Open technical debt is tracked as GitHub issues labelled `type: tech-debt`.
