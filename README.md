# Software Inc. Trainer

A trainer (cheat) mod for *Software Inc.* Beta 1. It adds a Trainer Settings
window with actions and toggles, and buttons above the game's Employees,
Products and Companies windows.

> **Back up your saves.** Some actions are permanent. Trainer settings are
> stored in the save game.

## Installation

You need *Software Inc.* Beta 1 on Windows. The trainer targets Beta 1.8.42
(`GameVersionCheck.TargetText`). Other builds are not supported.

- **Steam Workshop (recommended):** subscribe to the Workshop item. It
  contains the trainer's source, which the game compiles when the mod loads.
- **DLL:** download `Trainer_v<version>.zip` from
  [GitHub Releases](https://github.com/Trawis/T5B1/releases). Copy
  `Trainer_v5.dll` and the `Trainer.Localization` folder into the game's
  `DLLMods` folder.

## Usage

Load a game, then press the **Trainer** button or **F1**. **F2** closes the
window. It has **Actions**, **Toggles** and **Settings** tabs. The buttons
above the Employees, Products and Companies windows act on the rows selected
there.

The game's Options screen has two trainer switches, both off by default:
*Enable Experimental Features* (shows untested controls) and *Debug Logging*.

Every control is described in
[`docs/project/features.md`](docs/project/features.md).

## Limitations

- The trainer changes nothing in multiplayer games.
- Many toggles do not undo their changes when switched off. See
  [Switching toggles off](docs/project/features.md#switching-toggles-off).
- The build uses the vendored game assemblies in
  `lib/SoftwareInc/`. A game update can break it until they are
  refreshed.
- The Workshop build is compiled by the game's own C# 6 compiler and must
  follow the
  [Workshop source build](docs/project/architecture.md#workshop-source-build)
  rules. An exception in a trainer method can unload the Workshop mod.
- Tests cannot create live game objects. Code that touches them is checked only
  in the game.

## Build and test

You need a .NET SDK that builds `net46` projects (CI uses .NET 8).

```bash
git clone https://github.com/Trawis/T5B1.git
cd T5B1
dotnet build SoftwareIncTrainer.sln -c Release
dotnet format SoftwareIncTrainer.sln --verify-no-changes
dotnet test SoftwareIncTrainer.sln -c Release
```

The tests also run the game's source injector and compiler on the trainer
source.

## Releases and branches

- The release version is `Helpers.Version` in
  `src/Trainer.Source/Helpers.cs`.
- A release to `main` needs a higher version and one matching
  `## [<version>] - YYYY-MM-DD` section in [`CHANGELOG.md`](CHANGELOG.md).
- Nightly prereleases are built from `develop` and never change the version.
- Branch and pull request rules are in
  [`.agents/project.md`](.agents/project.md) and [`AGENTS.md`](AGENTS.md).

## Documentation and support

- [Features](docs/project/features.md): controls and effects
- [Architecture](docs/project/architecture.md): runtime, saves, game APIs and
  the Workshop build
- [Changelog](CHANGELOG.md)
- Bugs and ideas: GitHub Issues. Chat:
  [Discord](https://discord.com/invite/J584aG).

Maintained by Trawis with community contributors (see
[`credits.txt`](assets/Localization/credits.txt)).

No license file is present in the repository.
