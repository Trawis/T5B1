## Software Inc Trainer

*NOTE: I can't guarantee that some features integrated into this trainer will work with old saves. If you want to test it, please back up your data first.

Report bugs and suggestions through GitHub Issues.

[Features](FEATURES.md)
[Changelog](CHANGELOG.md)

## Development

This repository follows Git Flow.

### Long-lived branches

| Branch | Purpose |
|--------|---------|
| `main` | Production — stable releases only |
| `develop` | Integration — all feature work targets here |

### Branch families

| Branch | Base | PR target | Use for |
|--------|------|-----------|---------|
| `feature/<short-description>` | `develop` | `develop` | All normal work: features, fixes, docs, tests, refactors, tooling |
| `release/<major>.<minor>.<patch>` | `develop` | `main` | Release preparation and version bumps |
| `hotfix/<short-description>` | `main` | `main` | Urgent fixes for released code |

`release/*` and `hotfix/*` changes must also be brought back to `develop` after merge.

See [`AGENTS.md`](AGENTS.md) for full branching rules and coding conventions.

## CI / CD

| Trigger | Workflow | Result |
|---------|----------|--------|
| Push or PR to `develop` | CI | Build check |
| Push to `main` | Release | Build + versioned zip artifact + versioned GitHub Release |

The release workflow reads the semantic version from `Helpers.Version`. A release creates the matching `v<major>.<minor>.<patch>` tag and uses the version in the downloadable archive name. Increase `Helpers.Version` before merging another release to `main`.
