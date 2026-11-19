# Software Inc. Trainer

A trainer (cheat) mod for *Software Inc.* Beta 1. It adds a Trainer Settings
window with one-shot actions and toggles, plus buttons above the game's
Employees, Products and Companies windows, all built from the game's own UI.

> **Back up your saves.** Several actions are permanent, and settings are
> stored in the save game.

## Installation

*Software Inc.* Beta 1 on Windows is required. The trainer targets the game
version in `GameVersionCheck.TargetText` (currently Beta 1.8.42); other
builds are not supported.

- **Steam Workshop (recommended).** Subscribe to the trainer's Workshop item.
  The item is the trainer's source code, which the game compiles when the mod
  loads.
- **DLL.** Download `Trainer_v<version>.zip` from
  [GitHub Releases](https://github.com/Trawis/T5B1/releases) and copy
  `Trainer_v5.dll` and the `Trainer.Localization` folder into the game's
  `DLLMods` folder.

## Usage

Load a game and press the **Trainer** button or **F1** (**F2** closes the
window). The window has **Actions**, **Toggles** and **Settings** tabs. The
buttons above the Employees, Products and Companies windows act on the rows
selected there.

The game's Options screen lists the trainer with two switches, both off by
default and stored outside saves: *Enable Experimental Features* (shows
untested controls) and *Debug Logging*.

See [`docs/project/features.md`](docs/project/features.md) for every control.

## Limitations

- Toggles that change game state do not all undo it when switched off; see
  [Switching toggles off](docs/project/features.md#switching-toggles-off).
- The build depends on the vendored game assemblies in
  `Trainer_v5/Trainer.Libraries/`; a game update can break it until they are
  refreshed.
- The Workshop build is compiled by the game's own C# 6 compiler, so the
  source must follow the rules in
  [Workshop Source Build](docs/project/architecture.md#workshop-source-build).
  An exception in a trainer method can unload the Workshop mod.
- Automated tests cannot create live game objects; behaviour that touches
  them is verified only in the game.

## Build and test

Requires a .NET SDK that can build `net46` projects (CI uses .NET 8).

```bash
git clone https://github.com/Trawis/T5B1.git
cd T5B1
dotnet build "Trainer v5 - Beta 1.sln" -c Release
dotnet format "Trainer v5 - Beta 1.sln" --verify-no-changes
dotnet test "Trainer v5 - Beta 1.sln" -c Release
```

The tests also run the game's own source injector and compiler over the
trainer source.

## Releases

The release version is `Helpers.Version` in
`Trainer_v5/Trainer.Source/Helpers.cs`. Before merging a release to `main`,
raise it and add exactly one `## [<version>] - YYYY-MM-DD` section to
[`CHANGELOG.md`](CHANGELOG.md); the Release workflow then builds, tests and
publishes `Trainer_v<version>.zip` with those notes, and fails if the section
is missing, empty or duplicated. A pull request to `main` fails when the
`v<version>` tag already exists. The Nightly workflow publishes a
prerelease build of `develop` (tagged `nightly-<UTC time>-r<run>-a<attempt>`)
when it has new commits; nightlies never change `Helpers.Version`.

Branches follow Git Flow: `feature/*` from and into `develop`, `release/*`
from `develop` into `main`, `hotfix/*` from and into `main`; release and
hotfix changes are merged back to `develop`. Coding rules are in
[`AGENTS.md`](AGENTS.md).

## Documentation

- [`docs/project/features.md`](docs/project/features.md): controls and effects
- [`docs/project/architecture.md`](docs/project/architecture.md): runtime,
  persistence, game APIs and Workshop build
- [`CHANGELOG.md`](CHANGELOG.md): version history

## Contributing and support

Report bugs and suggestions through GitHub Issues; community chat is on
[Discord](https://discord.com/invite/J584aG).

Maintained by Trawis with community contributors (see
[`credits.txt`](Trainer_v5/Trainer.Localization/credits.txt)).

No license file is currently present in the repository.
