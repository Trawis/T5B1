# Architecture and Technical Overview

**Project**: T5B1 - Software Inc. Trainer (v5, Beta 1)
**Status**: Active
**Last Updated**: 2026-10-20

This document describes the current implemented architecture. Proposed
changes belong under `docs/project/designs/`; user-visible features are listed
in [`features.md`](features.md).

## Purpose and Scope

T5B1 is a client-side Software Inc. Beta 1 trainer (cheat mod) in a single
net46 / C# 6 assembly (`Trainer_v5.dll`). It runs inside the game process and
has no entry point of its own: the game drives it through mod lifecycle
callbacks, Unity scene events and time events. It depends only on the game's
public runtime types (`GameSettings`, `HUD`, `WindowManager`, `Actor`,
`Employee`, ...).

It ships two ways: the Steam Workshop item, which is the trainer's source
compiled by the game (see Workshop Source Build), and the DLL, loaded from the
game's `DLLMods` folder. It compiles against the current vendored Beta 1
assemblies in `Trainer.Libraries/` only, with no compatibility layer for older
builds and no reflection or runtime API bypasses; a game update is absorbed by
refreshing `Trainer.Libraries/` (including `mcs.dll`) and fixing what breaks.
`Helpers.Version` is the release version source of truth.

## Components

- `Main`: mod entry point, toolbar buttons, Options screen, settings
  persistence.
- `TrainerBehaviour`: scene/time event orchestration, toggle transitions and
  the `Cheats.*` flags; it raises triggers and owns no feature logic.
- `FeatureRegistry`, `FeatureDispatcher`, `FeatureGuard`, `ToggleStateTracker`:
  the schedule, its runner, the error boundary and toggle-change detection.
- `Features/*Features`: domain classes that own game-state mutations and
  one-shot UI actions: `EnvironmentFeatures`, `EmployeeFeatures`,
  `CompanyFeatures`, `ProductFeatures`, `MarketFeatures`, `WorkItemFeatures`.
- `SettingKeys`: the registry of every persisted setting, its save key and
  default. `Helpers`: the settings dictionaries built from it and `TryExecute`.
- `GameMinuteWatcher`: synthesizes the minute-passed event the game lacks.
- UI: `SettingsWindow` (with `SettingToggleRows`), `UIFactory` and `UIHelper`
  (widgets and layout), `InputHelper`, `Notification`, and the employee editors
  (`DetailWindowTrainer`, `Window/*`, `SDK/*`).

## Runtime Architecture

Startup subscribes to Unity scene-load events (`SceneRoutes` maps a scene name
to install, new-game screen or teardown). Entering the main scene installs the
trainer UI and subscribes to the game's time events; returning to the menu
tears both down. Entering the main scene and leaving the game also reset the
per-game caches (`TrainerBehaviour.ResetPerGameState`: window references, the
specialization list, hosting timing flags, the pending-list warning and the
printer baselines); persisted settings are never part of that reset.

Scheduled features are rows in `FeatureRegistry`: an id, the setting that
gates it, a trigger and the action. `FeatureDispatcher` runs the rows for a
trigger in registration order, each through `Helpers.TryExecute`. Triggers are
Frame, Minute, Hour, Day and Month (while the setting is on) and Enabled and
Disabled (once, on the frame a toggle changes). A feature with several
cadences has several rows sharing its id. Cadence is matched to how often the
game state can change; Frame rows are reserved for values the game overwrites
faster than the minute cadence and stay allocation-free (index loops;
`HashList`'s `foreach` boxes an enumerator, and `Thoughts.Values` is the live
list, so it is walked backwards instead of copied). `GameMinuteWatcher` exists
only because the game has no native minute event. One-shot actions run from
UI-triggered input dialogs, not from a cadence.

A periodic feature (Minute, Hour, Day, Month) also has an Enabled row so it
takes effect the moment it is switched on, unless it is listed in
`FeatureRegistry.NoCatchUp`; a Disabled (restore) row needs an Enabled row.
The `Cheats.*` flags (Force Lights, Show Room Ceilings, Unlimited
Subsidiaries) stay in `TrainerBehaviour`: they have no save backing, so they
are pushed on the first frame of the process and again on every toggle
transition, which includes the first observation after each load.

## Runtime Feature Isolation

`FeatureDispatcher` owns the runtime error boundary:

    Helpers.TryExecute(featureId, action);

One trainer feature is one boundary, even across several game collections.
Domain classes do not catch or log, call `TryExecute` or decide when they run.
A failed feature must not block unrelated scheduled features, and repeated
failures are rate-limited rather than flooding the log. `TryExecute`'s `bool`
result is used only where orchestration needs success state for retryable
one-time work. UI buttons call the one-shot actions on the domain classes
directly. The classes are `internal static`, each with its own private
one-line `Settings` accessor.

In the DLL build this isolates a failing feature. In the Workshop source build
it only keeps the rest of the frame running: the game's injected handler runs
first. Every named method body, which includes nearly every registry action,
ends in `ModController.HandleException`, which shows the error window and
unloads and deactivates the mod before `FeatureGuard` sees the resulting
`ModException`. An inline lambda body is not wrapped, so a failure written
directly in one reaches only the guard (`WorkshopExceptionBoundaryTests`
simulate this with the game's injector and compiler). The handler's behavior
comes from the game assembly and has not been run in the game (#241).

## Logging

Normal logging is always on and has no setting: a feature failure is reported
once per rate-limit window through `Helpers.TryExecute` (a `Logger` console line
naming the feature plus `UnityEngine.Debug.LogException`), and a skipped action
writes one `Logger.Log` warning to the dev console. Nothing in Normal logging
looks at `LoggingOptions`.

*Debug Logging* (mod Options, default off) adds trace lines on top:

- `Logger.LogDebug` writes `Trainer [debug]: ...` to the dev console only
  (`DevConsole.Console.Log`). It never uses `Debug.Log*`: the game stops Unity
  logging at 1 MB and 2 MB of `Player.log`, and its error handler counts every
  logged error or exception against a budget of 50 and can pause the game to
  show its report dialog. Do not add further `Debug.LogError` or
  `Debug.LogException` paths for trace output.
- Every `.LogDebug(` call is wrapped in `if (LoggingOptions.DebugEnabled)`, so
  nothing is built while the option is off. Trace lines are event based, never
  per frame: a toggle transition and the entries it queued, the first failure
  and the end of a transition's retries (not each retry), game ready,
  dispatcher creation, the per-game reset and the first application of the
  one-time static settings.
- `LoggingOptions` (`Trainer.Source/LoggingOptions.cs`) stores `DebugLogging`
  as `True`/`False` in the same per-mod settings file as the Experimental
  option (see below) and is independent of it.

In the Workshop source build every method body, including the logging helpers,
is wrapped by the injected exception handler, so logging code must stay trivial
and exception-free. `LoggingOptionsTests` enforces the guard and the
console-only rule. Not verified in the game.

## Toggle Transitions

Reversible toggles restore game state on disable only when the previous or
current normal value can be recovered safely (a verified constant, a baseline
captured before the first override, or the game's own public recomputation);
destructive or unverifiable toggles are not given fake restoration semantics.
The observed toggle states are cleared when the game is left or the mod is
deactivated, so each save load is a cold start: an enabled toggle runs its
enable hook. A toggle that was on before the reset keeps that fact for its
first observation, so one that is now off still runs its restore; this matters
for state the game keeps across loads (`AI.MaxBoxes`, `EdCost`, `Cheats.*`).

An Enabled/Disabled entry whose guarded call fails is not consumed: the
dispatcher keeps the failed entries of the toggle's latest transition and
retries only those on the following frames (every frame for the first 60, then
every 60th). The next transition of the same toggle replaces what is pending;
pending retries survive the tracker reset, so a restore that failed is still
run in the next game. This is safe because every Enabled/Disabled action is an
idempotent write of an absolute value or baseline, or a catch-up scan of
unfinished work; an action with a lasting one-off side effect must not be
registered on these triggers. Retries do not make the Workshop build recover
from an exception: a named method that throws still unloads the mod.

### Non-restoring toggles

A toggle with no Disabled row in `FeatureRegistry` stops acting when switched
off and does not undo earlier writes; `features.md` lists the player-visible
result per toggle. Restoring toggles are `NoEducationCost`, `FreePrint`,
`FullEnvironment`, `FullRoomBrightness`, `IncreaseWalkSpeed`,
`IncreaseCourierCapacity`, `IncreasePrintSpeed` and `ReduceISPCost`; the
`Cheats.*` toggles are bound to their setting when it changes. `FreePrint`
restores the constructor value of `ProductPrinter.PrintPrice` (`0.1`, the only
game write). `IncreasePrintSpeed` restores a per-printer baseline of
`ProductPrinter.PrintSpeed`, which has no game writer and no constructor
value: the baseline is read before the first write, keyed by the printer's
Unity instance id (no game object is kept), pruned against
`GameSettings.ProductPrinters` on every apply and restore, and cleared when a
game is left. A printer created or loaded while the toggle is on is captured
on the next hourly apply; a printer without a baseline is left untouched on
restore.

The other toggles fall into these groups (checked against the vendored
`Assembly-CSharp.dll`):

- Furniture fields that are not saved and have no game writer:
  `Furniture.AuraValues` (`IncreaseBookshelfSkill`), `Furniture.Comfort`
  (`NoMaintenance`), `Furniture.Water` and `Wattage` (`NoWaterElectricity`),
  `Furniture.Noisiness` (`NoiseReduction`), `Furniture.CanSteal`
  (`DisableFurnitureStealing`), `Upgradable.FireStarter` (`DisableFires`).
  `Furniture.SerializeMe` does not write them, so they stay changed for the
  loaded objects and come from the object definition after a load. The original
  values are per furniture type or item and are not captured.
- Saved state with no recorded original: `Employee` salary fields
  (`FreeEmployees`), `Employee.BirthDate` (`LockAge`), `Actor.VacationMonth`
  (`NoVacation`), `Upgradable.Quality` (repairs by `NoMaintenance`), support
  ticket timestamps, work items advanced or started by the `Auto*` toggles,
  deals added by `MoreHostingDeals`, closed or removed distribution platforms
  (`DigitalDistributionMonopol`), despawned burglars and inspectors,
  `PassedFireInspection`, and money refunded by the cost-refund toggles.
- Values the game writes itself: `Furniture.ActorNoise`, `EnvironmentNoise` and
  `FinalNoise`, `Actor.Noisiness`, `Actor.Effectiveness`, `Employee.Stress` and
  needs, `Employee.JobSatisfaction`, `Room.Temperature`, `GermCount` and
  `Smell`, `GameSettings.ServerCost`, `StaffSalaryDue`, the utility bills,
  `Heat` and `ForcePause`. The assembly has a writer for each; how soon it
  replaces the last trainer value is not determined.

No public game method that recomputes the first two groups was identified, and
the calling conditions of the game's salary and vacation writers for player
employees were not traced. The settings window does not mark one-way toggles.

## Persistence and Settings

Trainer settings persist through the game's `WriteDictionary`/deserialize
mechanism; serialized key compatibility must be preserved across releases.
Every persisted setting is declared once in `SettingKeys` as a typed
`ToggleKey` or `StoreKey` carrying its save key and default (not generic: the game's source-mod compiler emits invalid IL for fields of user-defined generic types); `Helpers.Settings` and
`Helpers.StoresSettings` are built from it, so serialization still reads and
writes the same string keys. Code references settings through `SettingKeys`
(a misspelled setting fails to compile) rather than raw strings. The key string
passed when declaring a setting is the save-file key and must never change; a
new setting is one new declaration there plus its UI and behavior.
A full load takes every key the save lacks from its declared default, so settings never carry over from the previously loaded save; building and company loads keep the values in use.
`ReduceBoxPrice`, `AutoMaxMarketShare`, `MoreCreativity`, `ReduceExpansionCost`, `DisableSkillDecay` and the old per-save `Experimental` are
retired (`RetiredSettingsTests`): they are declared with `SettingKeys.Retired(...)`, which reserves the name and keeps
the declaration but leaves the key out of `Helpers.Settings`, `Serialize` and `Deserialize` (#439). New saves do not
contain them, and a `true` stored in an older save is ignored, so it cannot restore removed behavior; the other keys
in such a save load as usual. An older trainer build that loads a new save finds these keys missing and falls back to
its default or in-memory value (all six are off by default and had no working effect in the last release), so
downgrading loses at most a stale `true` of a setting that does nothing. `ReduceExpansionCost` only wrote `GameSettings.ExpansionCost`, which nothing in the
game reads; the real plot price is `PlotArea.Price` (`PricePerSqm * Area + AddonCost`), and `PricePerSqm` is saved
with the plot and also feeds selling refunds and company worth, so a baseline-based reduction could not be restored
safely after a reload (#354). `Experimental` is retired the same way (#245): nothing reads a saved value, so an old
save with `Experimental` true cannot enable experiments. `DisableSkillDecay` (#342) was only ever a default value: no
code read it and Beta 1 has no skill decay to disable.

### Experimental opt-in (mod Options)

The *Enable Experimental Features* switch is the only authority for
experimental behavior (default off). `Main.ConstructOptionsScreen` builds it
with `WindowManager` widgets. It is stored in the game's own per-mod settings,
not in saves and not in `Helpers.Settings`:

- The game reads `DLLMod.Settings` from `DLLMods/<mod file>Setting.txt` in the
  `DLLMod` constructor and rewrites the file on `DLLMod.SaveSetting` (the call
  `ModBehaviour.SaveSetting` makes). `Main.Initialize(DLLMod)` runs after the
  read, so `ExperimentalOptions.Load` has the value when the mod loads, before
  any game exists. The value is global to the installation and never written to
  a save; `ModMeta.Serialize`/`Deserialize` are per save and are not used, and
  the game's `Options` screen has no registration point for mods.
- `OptionsWindow.AddModOption` calls `ConstructOptionsScreen(parent, inGame)`
  once per mod when the Options window starts (and when a mod is loaded or
  reloaded later); the game keeps the content, so the toggle reads the value
  when built and is its only writer.
- *Debug Logging* (`LoggingOptions`, see Logging) is a second, independent
  option in the same `DLLMod.Settings` file under its own key `DebugLogging`.
- `ExperimentalOptions` (`Trainer.Source/Experimental/`) is the single
  accessor; only a stored `True` enables it, missing or damaged values stay off.
- Experimental-only code goes in `Trainer.Source/Experimental/` (namespace
  `Trainer_v5.Experimental`), is checked against `ExperimentalOptions.Enabled`
  by its caller, and is never added to `FeatureRegistry`, `SettingToggleRows`
  or the Trainer Settings window. There is no experimental feature yet, and the
  Trainer Settings window has no Experimental toggle or Test button.

Not verified in the game: the Options layout, the toggle round trip and that
the game writes the settings file for a Workshop source mod the same way as for
a DLL mod (`Assembly-CSharp.dll` shows the same `DLLMod` path for both).

## Maintaining Persistent Features

A persistent toggle touches the steps below. The tests named per step are the
ones that fail when it is skipped.

1. Declare the key. Add `public static readonly ToggleKey X = Toggle("X");` to
   `SettingKeys` (`Store("X")` for a `StoreKey`). Toggles default to off and
   stores to unset; the field name equals the key string, which is unique and
   never renamed or reused. Add the key to `SettingsTests.PersistedToggleKeys`
   (`PersistedStoreKeys` for a store). Enforced by `SettingKeysTests` and
   `SettingsTests` (`ToggleKeys_MatchThePersistedKeySet`,
   `FreshSettings_AreAllDisabledAndStoresAreUnset`); a duplicate key throws
   when `SettingKeys` is first used.
2. Serialization needs no code: `Main.Serialize` and `Main.Deserialize` iterate
   the dictionaries built from `SettingKeys`. A full load gives a key missing
   from the save its declared default; a building or company load keeps the
   value in use (`SerializationTests`). Code refers to the key as
   `SettingKeys.X`, never as a string (`SettingKeyUsageTests`).
3. Add the Settings window row. A standard toggle is one
   `SettingToggleRows.Create` row: column 2 to 4, a unique localization key, the
   English default text and the key. A toggle with an immediate side effect
   stays explicit in `SettingsWindow` and is listed in
   `SettingToggleRowsTests`; a persisted toggle without a row must be listed
   there too. Enforced by `SettingToggleRowsTests`
   (`EveryPersistedToggleHasOneRowOrIsExplicitInTheWindow`,
   `RowsStayInTheFirstFourColumnsAndHaveAUniqueLocalizationKey`).
4. Localize the text. The label is looked up with the localization key and the
   in-code default text (`"Key".LocDef("Default text")`). Add the key and the
   same English text to `Trainer.Localization/English/Trainer.tyd`; other
   language files are not required to contain it. A message that includes a
   runtime value keeps a numbered placeholder in the text and is filled in with
   `string.Format("Key".LocDef("Name: {0}"), value)`; a literal `\n` in the
   text is a line break. Enforced by
   `SettingToggleRowsTests.EveryRowDefaultTextMatchesTheEnglishFile` and
   `LocalizationTests` (`EveryKeyTheCodeLooksUpExistsInEnglish`,
   `CodeFallbackTextMatchesTheEnglishFile`, `EveryLanguageKeyAlsoExistsInEnglish`,
   `NoLanguageDefinesAKeyTwice`, `EveryLocalizationFileEndsWithANewline`,
   `ActiveMessagesAndLabelsGoThroughLocDef`,
   `MessagesWithPlaceholdersAreFormattedByTheCode`).
5. Choose the cadence and add `FeatureRegistry` rows: a Frame, Minute, Hour, Day
   or Month row for the toggle, whose action lives on a `Features/*Features`
   class and does not catch errors itself. The row id is the one error boundary
   of the feature and belongs to one setting
   (`FeatureDispatcherTests.TheRegistryUsesEachFeatureIdWithOneSettingOnly`;
   `FeatureBoundaryTests` keep the `Features/` classes free of `TryExecute`
   and of each other). A periodic row also needs an Enabled row, or the id in
   `FeatureRegistry.NoCatchUp` (`FeatureScheduleTests`).
6. Decide what switching off does. A restoring toggle gets a Disabled row that
   writes the recovered value; it needs an Enabled row
   (`FeatureScheduleTests.EveryRestoreRowHasAnApplyRowForTheSameFeature`). A
   toggle with no Disabled row is non-restoring: record it, with the
   player-visible result, in `features.md` and in Non-restoring toggles above.
   Enabled and Disabled actions must be idempotent: a failed one is retried
   (`FeatureDispatcherTests`). Transitions come from `ToggleStateTracker`: an
   enable runs on the first observation of a save that has the toggle on, and a
   restore runs when a toggle that was on before leaving the game is off in the
   next one
   (`ToggleStateTrackerTests`, `FeatureDispatcherTests.ARestoreRunsWhenTheNextSaveHasTheToggleOffAfterTheTrackerWasReset`,
   `TransitionResetWiringTests`).
7. Keep a retired toggle declared, but through `Retired(...)` in `SettingKeys`
   so its name stays reserved and it is no longer written or loaded. Removing
   the UI and the rows is allowed; renaming the key is not. Add it to
   `RetiredSettingsTests` (original name, default off, no `FeatureRegistry`
   row, no Settings window row, absent from new saves) and remove it from
   `PersistedToggleKeys`.
8. Keep the code inside the Workshop source build rules (see Workshop Source
   Build). `GameSourcePipelineTests` (`EverySourceFileParsesWithTheGamesCompilerAfterInjection`,
   `EveryInjectedTryBlockWrapsAMethodOrConstructorBody`,
   `NoMethodBodyEndsInAnUnconditionalThrow`), `GameCompilerCompatibilityTests`
   (`SourceDeclaresNoGenericClassesOrStructs`) and `GameCompilerHazardTests`
   fail when the source breaks a rule, and `WorkshopExceptionBoundaryTests`
   describe how a failing registry action behaves there.

## Workshop Source Build

The Steam Workshop item is the trainer's source, not the DLL, and is the
primary deployment. When the game loads it, `ModController.InjectTryBlocks`
prepends three `SWINC*` `#define` lines, strips comments and wraps every
method and constructor body in `try`/`catch`, and the game's bundled C# 6
compiler (`mcs`) compiles the result. The SDK build cannot predict whether that
succeeds, so the source has to stay inside what the injector and that compiler
handle:

- No call in a property or field initializer that is followed by a property
  with accessors: the injector takes any `(` at member level for a method and
  rewrites the wrong brace (v5.3.2, #222). Assign in a static constructor.
- No `'{'` or `'}'` char literals, and no apostrophe inside a string before a
  comment that contains a brace on the same line.
- No parameter or local named `ex` in a method with a block body: it collides
  with the injected catch variable (CS0136).
- No fields in user-defined generic classes or structs (invalid IL, #225/#229).
- No method or constructor body that has no `return` and ends in a `throw`: the
  injected `try` leaves nothing after it and the compiler emits a branch past
  the end of the method.
- C# 6 only. The game's compiler rejects C# 7 syntax (CS1644).

`GameSourcePipelineTests` and `GameCompilerHazardTests` in `Trainer_v5.Tests`
run the game's own injector and compiler over `Trainer.Source` in CI, reporting
failures against the repository file, line and column. The injector's private
methods are called by reflection from the tests only, and
`GameSourceInjector.VerifyMirror` fails when the game's `InjectTryBlocks` stops
calling what the tests repeat. `Trainer.Libraries/mcs.dll` is the compiler from
the game's `Managed` folder; only the tests use it (it is not referenced by the
mod project or packaged). The game marks it as requiring a 32-bit process,
which a 64-bit test host rejects, so the tests load a temporary copy with that
flag cleared.

What the tests cannot establish:

- Type resolution and the rest of the semantic compile: the vendored libraries
  are a subset of the game's assemblies and `mcs` cannot load the game's types
  without the others, so the SDK build covers this.
- Valid IL beyond the patterns above, and anything that only fails at runtime
  in Unity (the generic-field failure is one). Those need the game.
- How the game's error handler behaves at runtime (see Runtime Feature
  Isolation; #241).

## UI Architecture

Trainer UI is built from Software Inc's `WindowManager` widgets so it matches
native game UI. `SettingsWindow` owns the main toggle/button grid: standard
toggles are rows in `SettingToggleRows` (column, localization key, default
text, setting), toggles with an immediate side effect stay explicit, and the
Experimental opt-in lives in the mod Options. Employee editors (traits,
demands, lead specialization, inspiration, founder, skill changes) are separate
windows on shared layout helpers; their buttons sit above the native detail
window (`DetailButtonLayout`).

- Trait, Demand, LeadSpec, Inspiration and Founder resolve their employee
  through `EmployeeTarget`: the open detail window's employee, else exactly one
  selected employee, never the first of several. A deferred dialog re-checks
  that the same live employee is still there before writing. Skill Change keeps
  its intentional multi-employee selection.
- Founder (`FounderToggle`, #190) flips only `Employee.Founder` after a Yes/No
  confirmation; the game treats a founder as unpaid (`Actor.WorksForFree`) and disables
  Fire when the detail window is shown, so the employee must be selected again.
- Demand grants through the game's `AcceptDemand` (its effect runs and its cost
  is charged as in the game; a refund would stack with the game's refund on
  dismissal) and removes by changing `DemandResults`, `DemandsMet` and
  `DemandsRequested` together (`DemandBits`), because the native detail window
  shows demands only while `DemandsMet` is set.
- Messages (#451) go through `Notification`, the only code that calls the
  game's popup and dialog APIs. `Notification.Popup` shows a HUD popup (Cogs
  icon) whose text starts with `Trainer: ` exactly once: text that already
  starts with it, as the English popup values do, is left alone, so a translation
  may include it or not. `ShowInfo`, `ShowWarning` and `ShowError` open
  non-modal dialogs without a prefix; a refusal (unknown or ambiguous name,
  invalid input) is an error, a completed action or a nothing-to-do notice is
  information. Input dialogs (`SpawnInputDialog`, `InputHelper`) are separate:
  their title and prompt are never prefixed. Logging stays in `Logger`.
  `NotificationTests` enforces this.
- Confirmation policy (#242): value-entry and routine one-shot actions
  (takeover, subsidiary, max market recognition, max skill, reset age, sell
  stock, unlock furniture/space, instant buttons, extend deadline, offshore
  transfer, team optimization, editors) never ask; the typed dialog is itself
  the approval for value entry. Only the audited high-impact set asks, always
  through `Confirmation.Ask`: AI Bankrupt All, Clear all loans, Unlock and Claim
  Rewards, targeted Bankrupt, Max Reputation and Founder. `Ask` is the game's own
  `WindowManager.ShowMessageBox` Yes/No (no question id, so no "Don't ask again";
  no pausing; Enter is Yes and Esc is No in the game's dialog; No, Esc and closing
  never run the action). Prompts name the action and scope, carry no `Trainer: `
  prefix, and bulk actions show the count taken when the dialog opened; with a
  scope of zero they only show a message. On Yes the game is checked again
  (`Helpers.RequireGameReady`, same `GameSettings` instance) and the scope is
  re-resolved (`Confirmation.IsScopeUnchanged`); if it changed nothing is applied
  and the player is told. Adding a confirmation means updating this list and
  `ConfirmationTests`, which pins the exact set.
- Most UI text is localized (a localization key with an English default) in
  `Trainer_v5/Trainer.Localization/<language>/`; some popups, dialogs and
  labels are English-only.

## Validation

- CI builds and validates `Release` only; `Debug` is for local development.
  `dotnet format` and `dotnet build` are the local static checks.
- `Trainer_v5.Tests` (xUnit, `net462`) runs real trainer code against the
  vendored assemblies outside Unity: setting keys, serialization, input
  validation, toggle transitions, scheduling, localization and the Workshop
  source pipeline. Anything that calls into live game objects (the `Features/`
  classes, window construction) is not covered and needs in-game validation.

Open technical debt is tracked as GitHub issues labeled `type: tech-debt`.
