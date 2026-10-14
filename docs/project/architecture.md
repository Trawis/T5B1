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

Startup subscribes to Unity scene-load events. Entering the main scene
installs the trainer UI and subscribes to the game's time events; returning
to the menu tears both down.

Features are scheduled at per-frame, minute, hour, day, or month cadence,
matched to how often the underlying game state can actually change.
`GameMinuteWatcher` exists only because the game has no native minute-passed
event. One-shot actions run from UI-triggered input dialogs rather than a
scheduled cadence.

## Runtime Feature Isolation

Scheduling/orchestration (`TrainerBehaviour`) owns runtime error boundaries:

    Helpers.TryExecute(featureId, action);

One trainer feature is one boundary, even when it spans multiple game
collections. Domain feature implementations do not implement their own
catch/logging layer. A failed feature must not block unrelated scheduled
features; repeated failures are rate-limited rather than flooding the log.
`TryExecute`'s `bool` result is used only where orchestration needs success
state for retryable one-time work.

The domain classes under `Features/` own only game-state mutations. They do
not call `TryExecute` or decide when they run; `TrainerBehaviour` owns the
cadence, the boundary, and the toggle transitions, and calls into them. UI
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
`SettingKey<T>` carrying its save key and default; `Helpers.Settings` and
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

## UI Architecture

Trainer UI is built entirely from Software Inc's `WindowManager` widgets, so
it matches native game UI. `SettingsWindow` owns the main trainer
toggle/button grid; specialized editors (employee traits, demands, lead
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
- Runtime behavior depends on the actual game and needs in-game validation.

## Architectural Debt

- None currently tracked.

## Design Records

Large proposed architectural changes belong in `docs/project/designs/`; this
document describes implemented architecture.
