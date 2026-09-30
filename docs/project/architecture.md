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
- `TrainerBehaviour`: scene/time event orchestration, feature scheduling, and
  the runtime error-boundary contract (see Runtime Feature Isolation).
- `Helpers`: settings dictionaries, property accessors, and `TryExecute`.
- `GameMinuteWatcher`: synthesizes a minute-passed event the game doesn't
  natively provide.
- `SettingsWindow` and the UI layer (`UIHelper`/`UIFactory`/`Utilities`/
  `InputHelper`/`Notification`): game-native UI construction and input.
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

When feature logic is extracted out of `TrainerBehaviour` (#126), scheduling
stays in the orchestration layer while domain handlers own only game-state
mutations.

## Toggle Transitions

Scheduling/orchestration also owns enable/disable transitions. Reversible
toggles restore game state on disable only when the previous or current
normal value can be recovered safely (a verified constant, or the game's own
public recomputation); destructive or unverifiable toggles are not given
fake restoration semantics.

## Persistence and Settings

Trainer settings persist through the game's `WriteDictionary`/deserialize
mechanism; serialized key compatibility must be preserved across releases.
Settings are currently raw string-keyed dictionaries, tracked as technical
debt (#127).

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
- Runtime behavior depends on the actual game and needs in-game validation.

## Architectural Debt

- `TrainerBehaviour` still owns most feature logic directly (#126).
- Settings remain raw string-keyed rather than typed (#127).
- No automated test project exists (#129).
- UI helper APIs overlap across `UIHelper`/`UIFactory`/`Utilities`/
  `SDK.WindowHelper` (#130).

## Design Records

Large proposed architectural changes belong in `docs/project/designs/`; this
document describes implemented architecture.
