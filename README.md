# Software Inc. Trainer

A trainer (cheat) mod for the game *Software Inc.* (Beta 1 branch). It adds an
in-game window and employee editors for one-shot actions (add money, max
skills, take over companies) and continuous toggles (no needs, free employees,
auto-finish work), built from the game's own UI widgets.

Status: `active`, maintained for the current *Software Inc.* Beta 1 branch.

> **Note:** Some features may not work with old saves. Back up your data before
> testing.

## Installation

*Software Inc.* (Beta 1) on Windows is required.

- **Steam Workshop (primary).** Subscribe to the trainer's Workshop item. The
  item is the trainer's source: the game's injector wraps its methods and the
  game's bundled C# compiler builds it when the mod loads. The source therefore
  has to stay inside what that compiler accepts (see
  [Workshop Source Build](docs/project/architecture.md#workshop-source-build)).
- **DLL (separate path).** Download the release zip from GitHub Releases and
  copy `Trainer_v5.dll` and the `Trainer.Localization` folder into the game's
  `DLLMods` folder.

## Usage

Load a game and press the **Trainer** button (or **F1**; **F2** closes the
window). The extra buttons above the **Employees** window (including **Skills**) edit
the one selected employee, or all selected employees in the skill editor. See
[`docs/project/features.md`](docs/project/features.md) for every feature.

### Experimental options

The game's Options screen lists the trainer with an *Enable Experimental
Features* switch, off by default. The choice is stored by the game in the
mod's own `DLLMods/<mod>Setting.txt`, applies to every save game and is not
part of a save. Turning it on shows additional, unfinished toggles in Trainer
Settings (currently *No Forgetful Skill Loss*); turning it off switches them
off. The Trainer Settings window has no Experimental control of its own.

### Retired settings

Reduce Box Price, Auto Max Market Share, More Creativity, Reduce Expansion Cost
and the former Experimental toggle are retired: they have no control and no
behavior, and a value stored for them in an older save is ignored.

## Limitations

- May not work with old saves; back up first.
- Toggles that write game state do not all undo it when switched off; see
  [`features.md`](docs/project/features.md#switching-toggles-off).
- Tightly coupled to the vendored game assemblies in
  `Trainer_v5/Trainer.Libraries/`; a game update can break the build and
  requires refreshing them. Older Beta 1 builds are not supported.
- Automated tests cannot create live game objects; behavior that touches them
  is checked only by loading the mod in the game.

## Build and test

Requires a .NET SDK that can build the `net46` project (CI uses .NET 8). The
build references the game assemblies in `Trainer_v5/Trainer.Libraries/`.

```bash
git clone https://github.com/Trawis/T5B1.git
cd T5B1
dotnet build "Trainer v5 - Beta 1.sln"
dotnet test "Trainer v5 - Beta 1.sln"
```

The solution has the configurations `Debug` and `Release`, both targeting the
vendored assemblies; CI builds `Release` only. `Trainer_v5.Tests` (xUnit,
`net462`) covers the logic that runs outside Unity, and runs the game's own
source injector and C# compiler over the trainer source. The release version
is `Helpers.Version` in `Trainer_v5/Trainer.Source/Helpers.cs`. Trainer
settings are saved in the game save; no secrets or external configuration are
needed.

## Documentation

- [`docs/project/features.md`](docs/project/features.md) - feature reference
- [`docs/project/architecture.md`](docs/project/architecture.md) - architecture, persistence and Workshop build constraints
- [`CHANGELOG.md`](CHANGELOG.md) - version history

## Contributing

Report bugs and suggestions through GitHub Issues; community chat is on
[Discord](https://discord.com/invite/J584aG). Branching and coding rules are in
[`AGENTS.md`](AGENTS.md).

### Branching (Git Flow)

| Branch | Base | PR target | Use for |
|--------|------|-----------|---------|
| `main` | - | - | Stable releases only |
| `develop` | - | - | Integration |
| `feature/<short-description>` | `develop` | `develop` | Implementation and maintenance |
| `release/<major>.<minor>.<patch>` | `develop` | `main` | Release preparation and version bumps |
| `hotfix/<short-description>` | `main` | `main` | Urgent fixes for released code |

`release/*` and `hotfix/*` changes are also merged back to `develop`.

### CI / CD

| Trigger | Workflow | Result |
|---------|----------|--------|
| Push to `develop`, PR to `develop` or `main` | CI | Release build + format check + tests |
| Daily schedule or manual dispatch | Nightly | When `develop` has commits from the last 24 hours: build + format check + tests, a nightly zip, and a prerelease development build tagged `nightly-<UTC time>-r<run>-a<attempt>` that does not replace the latest stable release |
| PR to `main` | Release Version Guard | Fails when the `v<Helpers.Version>` tag already exists |
| Push to `main` | Release | Build + format check + tests, versioned zip + GitHub Release tagged `v<Helpers.Version>` |

Increase `Helpers.Version` before merging another release to `main`.

#### Nightly development builds

A nightly is an independent development build of `develop`, not a release candidate and not a stable release. It does not guess or imply the next release version; the UTC build time and the workflow run identify it.

| Item | Example | Meaning |
|------|---------|---------|
| Tag | `nightly-20261020T115100Z-r122-a1` | UTC build time (`yyyyMMddTHHmmssZ`), workflow run number, run attempt. Sorts by date and is unique for every run and rerun. Never a stable `vX.Y.Z` tag. |
| Release name | `Trainer Nightly — 2026-10-20 11:51 UTC` | Marked as a prerelease, never as the latest release. The notes list the full and short commit SHA, branch, build time and run. |
| Zip and artifact | `Trainer_v5_nightly_20261020T115100Z_r122_a1.zip` | Same identity as the tag. |
| Assembly and file build version | `5.4.0-nightly.20261020T115100Z.r122.a1` | SemVer build identifier of the DLL (`AssemblyInformationalVersion`; the numeric assembly and file version stay `5.4.0.0`). It is not a release version. |
| In-game text | `v5.4.0` | `Helpers.Version`, the product version chosen for the next release. Nightlies never change it. |

A rerun of the workflow gets a new run attempt and a new time, so it creates a new release and never changes an earlier one. The workflow fails instead of touching an existing published nightly, a tag that points to another commit, or a tag without a release.

Historical `-rc` prereleases are not managed by the workflow; retention is decided separately.

## Maintainers

Trawis, with community contributors (see
[`Trainer_v5/Trainer.Localization/credits.txt`](Trainer_v5/Trainer.Localization/credits.txt)).

## License

No license file is currently present in the repository.
