# Architecture and Technical Overview

**Project**: T5B1 - Software Inc. Trainer (v5, Beta 1)
**Status**: Active
**Last Updated**: 2026-09-28

This document describes the current implemented architecture. Proposed
changes belong under `docs/project/designs/`; individual features belong in
`FEATURES.md`.

## Purpose

T5B1 is a client-side Software Inc. Beta 1 trainer (cheat mod), compiled as a
single net46 Unity assembly (`Trainer_v5.dll`) loaded by the game's DLL mod
loader. It runs entirely inside the game process, ships no engine or server
component of its own, and targets only the current vendored Beta 1 build with
no backward-compatibility layer for older builds.

## System Context

The mod is a plugin with no entry point of its own; it is driven entirely by
game callbacks (mod lifecycle, Unity scene events, time events). It depends
on the game's public runtime types (`GameSettings`, `HUD`, `WindowManager`,
`Actor`, `Employee`, and related classes) and has no other external
dependencies.

## Components

- `Main`: mod entry point, toolbar buttons, settings persistence.
- `TrainerBehaviour`: scene/time event orchestration, feature scheduling,
  toggle transitions, and the runtime error-boundary contract (see Runtime
  Feature Isolation).
- `Features/*Features`: domain classes that own the game-state mutations and
  one-shot UI actions: `EnvironmentFeatures` (rooms, furniture, fires,
  intruders, native cheat flags), `EmployeeFeatures`, `CompanyFeatures`
  (money, bills, loans, costs, rewards, company actions), `ProductFeatures`,
  `MarketFeatures` (hosting deals, distribution, marketing), and
  `WorkItemFeatures` (design, research, patents, porting, team assignment).
- `SettingKeys`: the registry declaring every persisted setting, its key, and
  its default. `Helpers`: the settings dictionaries built from it, property
  accessors, and `TryExecute`.
- `GameMinuteWatcher`: synthesizes a minute-passed event the game doesn't
  natively provide.
- `SettingsWindow` and the UI layer: `UIFactory` is the only place that
  spawns labels, buttons, toggles and input boxes; `UIHelper` adds the named
  `GameObject` variants, column layout, window sizing and the scrollable
  column; `InputHelper` and `Notification` cover input dialogs and errors.
- Domain/editor helpers (`DetailWindowTrainer`, `Window.*`, `SDK.*`):
  specialized employee/actor editing windows.

## Runtime Architecture

Startup subscribes to Unity scene-load events (`SceneRoutes` maps a scene name to install, new-game screen or teardown; the Unity calls stay in `TrainerBehaviour`). Entering the main scene
installs the trainer UI and subscribes to the game's time events; returning
to the menu tears both down.

Features are scheduled at per-frame, minute, hour, day, or month cadence,
matched to how often the underlying game state can actually change.
`GameMinuteWatcher` exists only because the game has no native minute-passed
event. One-shot actions run from UI-triggered input dialogs rather than a
scheduled cadence.

Scheduled features are rows in `FeatureRegistry`: an id, the setting that
gates it, a trigger and the action. `FeatureDispatcher` runs the rows for a
trigger in registration order, each through `Helpers.TryExecute`. Triggers are
Frame, Minute, Hour, Day and Month (while the setting is on) and Enabled and
Disabled (once, on the frame a toggle changes; each toggle is observed once
per frame). A feature with several cadences has several rows sharing its id.
To schedule a new feature, add its rows; `TrainerBehaviour` only raises the
triggers. The `Cheats.*` flags pushed once per load stay in `TrainerBehaviour`.
A periodic feature (Minute, Hour, Day, Month) also has an Enabled row so it
takes effect the moment it is switched on, unless it is listed in
`FeatureRegistry.NoCatchUp`; `FeatureScheduleTests` enforces that, so a new
toggle has to make the choice. A Disabled (restore) row needs an Enabled row.

## Runtime Feature Isolation

Scheduling/orchestration (`FeatureDispatcher`) owns runtime error boundaries:

    Helpers.TryExecute(featureId, action);

One trainer feature is one boundary, even when it spans multiple game
collections. Domain feature implementations do not implement their own
catch/logging layer. A failed feature must not block unrelated scheduled
features; repeated failures are rate-limited rather than flooding the log.
`TryExecute`'s `bool` result is used only where orchestration needs success
state for retryable one-time work.

The domain classes under `Features/` own only game-state mutations. They do
not call `TryExecute` or decide when they run; `FeatureRegistry` and
`FeatureDispatcher` own the cadence, the boundary, and the toggle transitions,
and call into them. UI
buttons call the one-shot actions on the domain classes directly.
The classes and their members are `internal`; each keeps its own one-line
`Settings` accessor rather than sharing one, to avoid coupling them.

## Toggle Transitions

Scheduling/orchestration also owns enable/disable transitions. Reversible
toggles restore game state on disable only when the previous or current
normal value can be recovered safely (a verified constant, a baseline captured
before the first override, or the game's own public recomputation);
destructive or unverifiable toggles are not given fake restoration semantics.

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

## Game API Compatibility

- Targets `net46` / C# 6, matching the game's Unity runtime.
- Compiles against the current vendored Beta 1 assemblies in
  `Trainer.Libraries/` only; no backward-compatibility layer, no reflection
  or runtime API bypasses.
- Game API compatibility is maintained by refreshing `Trainer.Libraries/` and
  fixing whatever source incompatibilities the refresh introduces.
- `Helpers.Version` is the release version source of truth.

## Workshop Source Build

The Steam Workshop item is the trainer's source, not the DLL. When the game
loads it, `ModController.InjectTryBlocks` prepends three `SWINC*` `#define`
lines, strips comments and wraps every method and constructor body in
`try`/`catch`, and the game's bundled C# 6 compiler (`mcs`) compiles the
result. The SDK build cannot predict whether that succeeds, so the source has
to stay inside what the injector and that compiler handle:

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
run the game's own injector and compiler over `Trainer.Source` in CI. The
injector's private methods are called by reflection from the tests only, and
`GameSourceInjector.VerifyMirror` fails when the game's `InjectTryBlocks` stops
calling what the tests repeat. `Trainer.Libraries/mcs.dll` is the compiler
from the game's `Managed` folder (Unity's build of Mono's C# compiler). Only
the tests use it: it is not referenced by the mod project or packaged. The game
marks it as requiring a 32-bit process, which a 64-bit test host rejects, so
the tests load a temporary copy with that one flag cleared. When the game
updates, refresh it together with `Assembly-CSharp.dll`.

What the tests establish: the injected source parses with the game's compiler,
every injected `try` wraps a method or constructor body, and each hazard above
is checked (the generic-field rule by `GameCompilerCompatibilityTests`), with
failures reported against the repository file, line and column.
What they cannot establish:

- Type resolution and the rest of the semantic compile. The vendored
  libraries are a subset of the game's assemblies and `mcs` cannot load the
  game's types without the others, so the SDK build covers this.
- Valid IL beyond the patterns listed above, and anything that only fails at
  runtime in Unity (the generic-field failure is one). Those need the game or
  the in-game checklist.
- How exceptions behave at runtime: the injected handler reports through the
  game's error handler before `FeatureGuard` sees the failure (#241).

## UI Architecture

Trainer UI is built entirely from Software Inc's `WindowManager` widgets, so
it matches native game UI. `SettingsWindow` owns the main trainer
toggle/button grid (the standard toggles are rows in `SettingToggleRows`: column, localization key, default text, setting; the toggles with an immediate side effect and the Experimental switch stay explicit); specialized editors (employee traits, demands, lead
specialization, skill changes) are separate windows built on shared layout
helpers. UI text is localized; translations live under
`Trainer_v5/Trainer.Localization/<language>/`.

## Validation

- CI/CD builds and validates the `Release` configuration only; `Debug` is
  local-development only.
- `dotnet format`/`dotnet build` are the local static checks.
- `Trainer_v5.Tests` (xUnit, `net462`) runs in CI after the build. It covers
  setting keys and defaults, save serialization round-trips, input validation,
  toggle transitions, minute-event cadence, and the `FeatureGuard` behind
  `Helpers.TryExecute` (return value and log throttling). It runs the real
  trainer code against the vendored assemblies outside Unity, so anything that
  calls into live game objects (the `Features/` classes, window construction)
  is not covered and needs in-game validation.
- The Workshop source pipeline is checked with the game's own injector and
  compiler (see Workshop Source Build).
- Runtime behavior depends on the actual game and needs in-game validation.

## Architectural Debt

- None currently tracked.

## Design Records

Large proposed architectural changes belong in `docs/project/designs/`; this
document describes implemented architecture.
