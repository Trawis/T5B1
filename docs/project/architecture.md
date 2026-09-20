# Architecture and Technical Overview

**Project**: T5B1 - Software Inc. Trainer (v5, Beta 1)
**Status**: Active
**Last Updated**: 2026-09-10

This document describes the verified current technical system. Proposed changes
belong under `docs/project/designs/` until accepted or implemented.

## Purpose, Scope, and Quality Goals

T5B1 is a client-side trainer (cheat) mod for the Unity game *Software Inc.*
(Beta 1 branch). It compiles to a single managed assembly, `Trainer_v5.dll`,
that the game's DLL mod loader discovers at runtime. The mod adds an in-game
settings window and several employee-editing windows that let the player apply
one-shot actions (add money, max skills, take over companies) and toggle
continuously-enforced cheats (no needs, free employees, auto-finish work).

Quality goals that most influence the design:

- **Game compatibility**: the mod links against the game's own assemblies and
  must keep working across *Software Inc.* Beta versions, which is handled with
  compile-time conditional code paths.
- **Non-destructive persistence**: trainer settings serialize into the save
  file and must round-trip without corrupting saves.
- **Fail-soft behavior**: a cheat that throws must not crash the game; risky
  operations are guarded and logged.
- **Discoverability**: features are exposed through in-game UI built from the
  game's own `WindowManager` widgets so they match the game's look and feel.

Out of scope: the mod does not modify the game on disk, ship its own engine, or
provide any networked/server component.

## System Context

- **User**: a *Software Inc.* player who installs the mod DLL into the game's
  mod folder.
- **Host system**: the *Software Inc.* game process (Unity engine). The mod runs
  entirely inside this process and depends on the game's public runtime types
  (`GameSettings`, `HUD`, `WindowManager`, `Actor`, `Employee`, `SoftwareProduct`,
  `MarketSimulation`, and related classes).
- **External systems**: none at runtime. The only external reference is a Discord
  invite URL surfaced to the user as text.

The mod is a plugin: it has no entry point of its own and is driven entirely by
callbacks from the game (mod lifecycle, Unity scene events, and time events).

## Components and Responsibilities

Source lives under `Trainer_v5/Trainer.Source/`. All types share the
`Trainer_v5` namespace (with nested `SDK`/`Window` namespaces).

- `Main` (`ModMeta`): mod entry metadata, toolbar button creation, and settings
  serialization through `WriteDictionary`.
- `TrainerBehaviour` (`ModBehaviour`): runtime driver for scene/time events,
  per-frame toggle enforcement, and one-shot trainer actions.
- `Helpers`: central static state for versioning, settings dictionaries, role
  and specialization lists, efficiency options, property helpers, and game
  version detection.
- `Constants`: named numeric constants for UI layout and gameplay tuning.
- `SettingsWindow`: six-column trainer window built from `Helpers.Settings`.
  Buttons and toggles are wired to `TrainerBehaviour`.
- `EmployeeSkillChangeWindow`: standalone selected-employee role and
  specialization editor.
- `DetailWindowTrainer`: injects Trait, Demand, Creativity, Inspiration, and
  LeadSpec buttons into the game's employee detail window.
- `Window.EmployeeTraitChangeWindow`, `Window.EmployeeDemandChangeWindow`, and
  `Trainer.Source.Window.EmployeeLeadSpecChangeWindow`: lazy-singleton editor
  windows for traits, lead-design demands, and lead specializations.
- `UIHelper`, `UIFactory`, and `Utilities`: factories for game UI widgets and
  window/column layout.
- `InputHelper`: validated numeric input dialogs with parse and range checks.
- `Notification`: thin wrapper over the game's popup and dialog spawners.
- `Logger`: extension methods that write prefixed messages to the game console.
- `Extensions`: dictionary get, set, toggle, and combobox index helpers.
- `SDK.EmployeeHelper`: employee trait and lead-design demand helpers.
- `SDK.WindowHelper`, `LayoutHelper`, and `VerticalLayout`: vertical layout
  helpers for newer editor windows.
- `SDK.ComponentStyleHelper` (`DefaultStyles`) and `Window.WindowStyles`: shared
  component height and text styles.

## Important Runtime Flows

**Startup / scene changes.** `TrainerBehaviour.Start` seeds a `System.Random`
and subscribes to `SceneManager.sceneLoaded`. `OnLevelFinishedLoading` reacts to
scene names: entering `MainScene` calls `Main.CreateUIButtons`, installs the
detail-window buttons, and subscribes to time events; returning to `MainMenu`
tears those down. The `Customization` scene widens the available start-year and
start-loan options.

**Toolbar buttons.** `Main.CreateUIButtons` adds a *Trainer* button (opens the
settings window) to the main fan panel and a *Skill Change* button to the actor
window. `SettingsWindow.Init` lazily builds the six-column window from the
`Helpers.Settings` toggle dictionary; each toggle is bound back to the dictionary
and each button to a `TrainerBehaviour` static method.

**Per-frame enforcement.** `TrainerBehaviour.Update` runs every frame while the
game is loaded. It hotkeys F1/F2 to open/close the window, lazily loads employee
specializations once a company exists, then iterates furniture, rooms, and actors
applying whichever toggles are active (e.g. `NoStress`, `NoNeeds`,
`FreeEmployees`, `CleanRooms`, `FullEnvironment`), plus company-wide effects
(auto-finish design/research/patent, free print, no server cost, reduced ISP
cost, expansion cost). Toggles read their state through
`Helpers.GetProperty(TrainerSettings, "<Key>")`.

**Time events.** `OnMonthPassed` advances employee birth dates when `LockAge` is
on (keeping ages fixed). `OnHourPassed`/`OnDayPassed` are currently no-ops.

**One-shot actions.** Buttons that need input open a game input dialog whose
callback performs the change (e.g. `IncreaseMoney`, `SetProductPrice`,
`FixBugs`, `TakeoverCompany`). Actions post a confirmation popup or dialog via
`Notification`/`HUD.AddPopupMessage`.

**Employee editing.** `DetailWindowTrainer` adds buttons to the game's employee
detail window. Inspiration editing writes directly to `Employee`'s public
`Inspiration` field on the existing object. Creativity is a public but
read-only (`initonly`) field with no public setter or other official method to
change it, and this codebase does not use reflection, so creativity editing
still clones the `Employee` with the new value and transfers state onto the
clone, same as before. Trait and demand windows toggle flag enums directly;
lead-spec and skill windows write skill/specialization values on the selected
actors.

**Failure handling.** `Helpers.TryExecute` wraps risky actions in try/catch and
routes exceptions to `Logger.LogException` and `Debug.LogException`. Several
partially-working or version-specific features are guarded (see Cross-Cutting).

## Data, Interfaces, and Integrations

- **Persistence.** `Main.Serialize`/`Deserialize` write every key in
  `Helpers.Settings` and `Helpers.StoresSettings` into the game's
  `WriteDictionary`, so toggle and efficiency choices persist with the save.
  Deserialization falls back to the current in-memory value when a key is absent,
  which keeps older saves loadable after new toggles are added.
- **Game API surface.** The mod is tightly coupled to the game's internal types.
  It links against the game assemblies shipped in `Trainer.Libraries/`
  (`Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`) and Unity modules.
  See "Game Library Refresh" below for the full reference list and the
  procedure for updating them.
- **UI.** All widgets are spawned through the game's `WindowManager`
  (`SpawnWindow`, `SpawnButton`, `SpawnCheckbox`, `SpawnComboBox`,
  `SpawnInputDialog`, etc.), so the trainer UI is native game UI.
- **Localization.** UI text uses `"Key".LocDef("English default")`; translations
  live under `Trainer_v5/Trainer.Localization/<language>/Trainer.tyd`
  (Croatian, English, German, Korean, Spanish, Chinese, Chinese (Simplified)).

## Configuration and Deployment

- **Runtime/target.** `Trainer_v5.csproj` targets `net46` with C# `LangVersion 6.0`
  to match the game's Unity runtime. Game/Unity references are marked
  `Private=False` (not copied) because the game supplies them at runtime.
- **Build configurations.** The solution defines exactly two configurations,
  `Debug` and `Release`, with no version-specific preprocessor symbols. Both
  target the current vendored Software Inc Beta 1 assemblies under
  `Trainer.Libraries/` directly, so code that depends on the game's API
  (e.g. `MaxMarketShare`/`AutoMaxMarketShare`, which use
  `SoftwareProduct.MarketShare`) compiles unconditionally rather than behind
  a compatibility guard. `Helpers.GetGameVersion()` returns a constant
  `"Beta 1"` string: there is no reliable way to read an exact Software Inc
  build number from committed information, so it reports the current target
  generically instead of inventing a version number.
  Old game-version-specific configurations and preprocessor symbols
  (`SWINCBETA`, `SWINCRELEASE`, `SWINCBETA1_6`/`1_7`/`1_8`/`1_9`/`1_10`) are
  not maintained; T5B1 does not preserve backward compatibility with older
  Software Inc Beta 1 builds. Game compatibility is updated by refreshing
  the vendored assemblies under `Trainer.Libraries/` and fixing whatever
  source incompatibilities that refresh introduces, not by adding
  compatibility configurations or conditional code paths.
- **Version source of truth.** `Helpers.Version` (currently `5.2.7`) is the
  single semantic version; the release and nightly workflows parse it from
  `Helpers.cs`.
- **Packaging / deployment.** The Release build produces `Trainer_v5.dll`, which
  is packaged with the `Trainer.Localization` folder into
  `Trainer_v5_<version>.zip`. The player installs the DLL and localization into
  the game's mod directory.

## Game Library Refresh

`Trainer_v5.csproj` references exactly six vendored Software Inc / Unity
assemblies under `Trainer_v5/Trainer.Libraries/` (`Private=False`; the game
supplies them at runtime, they are not shipped with the mod):

- `Assembly-CSharp.dll`
- `Assembly-CSharp-firstpass.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.TextRenderingModule.dll`
- `UnityEngine.UI.dll`

If this list ever changes, update it to match `Trainer_v5.csproj`'s
`<Reference>`/`<Content>` entries - they are the source of truth.

**Supported target.** T5B1 supports the current vendored Software Inc Beta 1
build only; there is no backward compatibility with older builds. The exact
Software Inc build number cannot be established reliably from the committed
assemblies: their embedded version resources (`FileVersion`/`ProductVersion`)
are either `0.0.0.0` or blank, and none contain a recognizable Unity engine
version string. The strongest claim that can actually be proven is: **the
current Beta 1 assembly set committed under `Trainer_v5/Trainer.Libraries/`
in this repository**. Git already versions these binaries, so no separate
hash manifest or validation script is maintained for them.

**Refresh procedure**, when Software Inc updates:

1. Update Software Inc to the current supported Beta 1 build.
2. Obtain the required assemblies from that installation (the game's own
   managed-assembly folder).
3. Replace the corresponding DLLs under `Trainer_v5/Trainer.Libraries/`,
   matching the six filenames above exactly.
4. Review the binary file replacements in Git (e.g. `git status`) before
   committing, to confirm only the intended files changed.
5. Build `Debug`.
6. Build `Release`.
7. Resolve any source/API incompatibilities the new assemblies introduce.
8. Perform focused in-game smoke testing of the affected features.
9. Commit the DLL refresh together with whatever compatibility fixes step 7
   required.
10. Prepare release notes/version changes later, on the normal
    `release/<version>` branch - not as part of the refresh commit.

## Cross-Cutting Concerns

- **Compatibility.** T5B1 targets the current vendored Software Inc Beta 1
  build only; there are no compile-time compatibility guards for older game
  versions. Backward-compatible save loading is handled by the
  serialize/deserialize fallback described above.
- **Reliability.** `Helpers.TryExecute` and defensive null/collection checks
  (e.g. `IsGameReady`, `IsGameLoaded`) keep cheats from throwing into the game
  loop. Some methods short-circuit when the game or a required manager is not yet
  available.
- **Observability.** `Logger` writes prefixed (`Trainer: ...`) messages to the
  in-game dev console; there is no external logging.
- **Performance.** The `Update` loop iterates all furniture, rooms, and actors
  every frame when toggles are active. This is acceptable for the game's scale
  but is the mod's main hot path; guards ensure work only runs while a game is
  loaded.
- **Security/privacy.** No network calls, telemetry, or persisted secrets. The
  only outward reference is a static Discord invite URL shown to the user.

## Development and Validation

- **Build.** `dotnet build "Trainer v5 - Beta 1.sln"` with `--configuration
  Debug` or `--configuration Release` (the only two supported
  configurations) on `windows-latest` with .NET 8 SDK; the toolchain still
  targets `net46`. CI/CD only builds and supports `Release`; `Debug` is for
  local development and must be built locally.
- **Local checks.** `dotnet format --verify-no-changes` for formatting;
  `dotnet build` for compilation. There is no automated test project, so
  `dotnet test` provides no coverage; behavior is validated by loading the mod
  in the game.
- **CI/CD.**
  - *CI* (`.github/workflows/ci.yml`): builds the `Release` configuration on
    push/PR to `develop`; a compile failure fails the job.
  - *Nightly* (`.github/workflows/nightly.yml`): daily/manual; if `develop` had
    commits in the last 24h, builds a Release artifact and publishes a
    `v<version>-rc<run-number>` prerelease.
  - *Release* (`.github/workflows/release.yml`): on push to `main`, reads
    `Helpers.Version`, builds Release, packages the zip, and creates the
    `v<version>` GitHub Release (fails if the tag already points at another
    commit; increase `Helpers.Version` before releasing).
- **Environment limitation.** Building/running requires the game and Unity
  assemblies referenced from `Trainer.Libraries/`; full runtime validation
  requires the actual game.

## Decisions and Design Records

No formal technical designs are recorded under `docs/project/designs/` yet.
Record significant future changes there and summarize the resulting state here.

## Risks, Technical Debt, and Open Questions

- The per-frame `Update` method is large and enforces many toggles inline;
  it concentrates most runtime logic in one place.
- Several features are version-gated or noted in code as partially working or
  broken (e.g. `RemoveSoft` is disabled, `ReduceBoxPrice` is commented out,
  `//TODO` notes on maintenance and print pricing). `HREmployees` exists but is
  not currently wired to any UI button.
- Heavy coupling to internal game types means game updates can break the
  build; compatibility is maintained by refreshing the vendored assemblies
  under `Trainer.Libraries/` (see "Game Library Refresh" below).
- No automated tests exist; regressions are caught only by in-game testing.

## Detailed Documentation

None extracted yet. See `README.md` for setup/branching, `FEATURES.md` for the
user-facing capability list, and `CHANGELOG.md` for release history.
