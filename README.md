# Software Inc. Trainer

A trainer (cheat) mod for the game *Software Inc.* (Beta 1 branch). It adds an
in-game window and employee editors that let you apply one-shot actions (add
money, max skills, take over companies) and continuous toggles (no needs, free
employees, auto-finish work).

> **Note:** Some features may not work with old saves. Back up your data before
> testing.

## Why

*Software Inc.* is a business-simulation game. This mod gives players
sandbox-style control over the simulation for experimentation, testing, and
casual play, exposed through the game's own UI so it feels native.

## Status

`active` - maintained for the current *Software Inc.* Beta 1 branch.

## Requirements

- *Software Inc.* (Beta 1) installed.
- Windows (the game platform).
- To build from source: a .NET SDK able to build the `net46` target project
  (CI uses the .NET 8 SDK). The build references the game's assemblies in
  `Trainer_v5/Trainer.Libraries/` and Unity modules, which are supplied by the
  game at runtime.

## Setup

```bash
git clone https://github.com/Trawis/T5B1.git
cd T5B1
```

To install a built mod, copy `Trainer_v5.dll` and the `Trainer.Localization`
folder into the game's mod directory.

## Build

```bash
dotnet build "Trainer v5 - Beta 1.sln"
```

The solution defines `Debug`, `Release`, `SWINCBETA`, `SWINCBETA1_7`, and
`SWINCRELEASE` configurations for game-version conditional compilation.

## Test

```bash
# No automated test project exists.
# Validate changes by loading the mod in the game.
```

## Run

Install the built `Trainer_v5.dll` and `Trainer.Localization` into the game's
mod directory, launch *Software Inc.*, load a game, and press the **Trainer**
button (or F1).

## Configuration

- **Version source of truth:** `Helpers.Version` in
  `Trainer_v5/Trainer.Source/Helpers.cs`.
- **Build configurations:** select a configuration to target a specific game
  version via preprocessor symbols (see [Build](#build)).
- **Persistence:** trainer toggle and efficiency settings are saved into the
  game save file. No secrets or external configuration are required.

## Usage

Open the trainer with the **Trainer** button or **F1** (**F2** closes it). The
**Skill Change** button on the actor window and extra buttons on the employee
detail window edit selected employees. See [`FEATURES.md`](FEATURES.md) for the
full feature list.

## Documentation

- [`docs/project/architecture.md`](docs/project/architecture.md) - architecture and technical overview
- [`FEATURES.md`](FEATURES.md) - feature list
- [`CHANGELOG.md`](CHANGELOG.md) - version history

## Known Limitations

- May not work with old saves; back up first.
- Tightly coupled to specific game versions; game updates can break the build
  and require preprocessor/version updates.
- No automated tests; behavior is validated by loading the mod in the game.

## Help and Contributing

- Report bugs and suggestions through GitHub Issues.
- Community chat and support: [Discord](https://discord.com/invite/J584aG).

### Branching (Git Flow)

This repository follows Git Flow.

**Long-lived branches**

| Branch | Purpose |
|--------|---------|
| `main` | Production - stable releases only |
| `develop` | Integration - all feature work targets here |

**Branch families**

| Branch | Base | PR target | Use for |
|--------|------|-----------|---------|
| `feature/<short-description>` | `develop` | `develop` | Normal implementation and maintenance |
| `release/<major>.<minor>.<patch>` | `develop` | `main` | Release preparation and version bumps |
| `hotfix/<short-description>` | `main` | `main` | Urgent fixes for released code |

`release/*` and `hotfix/*` changes must also be brought back to `develop` after
merge. See [`AGENTS.md`](AGENTS.md) for full branching rules and coding
conventions.

### CI / CD

| Trigger | Workflow | Result |
|---------|----------|--------|
| Push or PR to `develop` | CI | Build check |
| Daily schedule or manual dispatch | Nightly | Build + versioned RC artifact + prerelease |
| Push to `main` | Release | Build + versioned zip artifact + versioned GitHub Release |

The release workflow reads the semantic version from `Helpers.Version`. A release
creates the matching `v<major>.<minor>.<patch>` tag and uses the version in the
downloadable archive name. Increase `Helpers.Version` before merging another
release to `main`. The nightly workflow appends the workflow run number as an RC
suffix, producing versions such as `5.2.6-rc123`; nightly prereleases do not
replace the latest stable release.

## Maintainers

Trawis, with community contributors (see
[`Trainer_v5/Trainer.Localization/credits.txt`](Trainer_v5/Trainer.Localization/credits.txt)).

## License

No license file is currently present in the repository.
