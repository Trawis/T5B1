# Software Inc. Trainer

A trainer (cheat) mod for *Software Inc.* Beta 1. It adds a Trainer Settings
window and buttons above the game's Employees, Products and Companies windows.

> **Back up your saves.** Some actions are permanent. Trainer settings are
> saved in the save game.

## Installation

You need *Software Inc.* Beta 1 on Windows. The trainer targets Beta 1.8.42.

- **Steam Workshop (recommended):** subscribe to the Workshop item. The game
  compiles its source when the mod loads.
- **DLL:** download `Trainer_v<version>.zip` from
  [Releases](https://github.com/Trawis/T5B1/releases). Copy `Trainer_v5.dll`
  and the `Trainer.Localization` folder into the game's `DLLMods` folder.

## Usage

Load a game. Press the **Trainer** button or **F1**. **F2** closes the window.
Toolbar buttons act on the rows selected in the native window.

The game's Options screen has two trainer switches: *Enable Experimental
Features* and *Debug Logging*.

All controls: [`docs/project/features.md`](docs/project/features.md).

## Limitations

- Many toggles do not undo their changes when switched off. See
  [Switching toggles off](docs/project/features.md#switching-toggles-off).
- A game update can break the build until `Trainer_v5/Trainer.Libraries/` is
  refreshed.
- The Workshop build uses the game's C# 6 compiler. See
  [Workshop source build](docs/project/architecture.md#workshop-source-build).
- Tests cannot create live game objects. That code is checked only in the game.

## Build and test

You need a .NET SDK that builds `net46` (CI uses .NET 8).

```bash
dotnet build "Trainer v5 - Beta 1.sln" -c Release
dotnet format "Trainer v5 - Beta 1.sln" --verify-no-changes
dotnet test "Trainer v5 - Beta 1.sln" -c Release
```

## Releases and branches

The version is `Helpers.Version`. A release also needs one matching
`CHANGELOG.md` section. Branching and CI rules are in
[`.agents/project.md`](.agents/project.md) and
[`.agents/guidelines/`](.agents/guidelines/).

## Links

- [Features](docs/project/features.md)
- [Architecture](docs/project/architecture.md)
- [Changelog](CHANGELOG.md)
- [Discord](https://discord.com/invite/J584aG) and GitHub Issues for support
- [Credits](Trainer_v5/Trainer.Localization/credits.txt)

No license file is present in the repository.
