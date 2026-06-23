## Software Inc Trainer

*NOTE: I can't guarantee that some features integrated into this trainer will work with old saves. If you want to test it, please back up your data first.

Report bugs and suggestions through GitHub Issues.

[Features](FEATURES.md)
[Changelog](CHANGELOG.md)

## Development

This repository follows Git Flow.

| Branch | Purpose |
|--------|---------|
| `main` | Stable releases only |
| `develop` | Integration branch — all feature work targets here |

### Contributing

Branch from `develop` and open a PR back to `develop`:

```
feature/<short-description>
```

Use `feature/*` for all normal work — including documentation, tests, maintenance, and non-emergency bug fixes.

For releases, branch from `develop` as `release/<major>.<minor>.<patch>` and target `main`.

Hotfixes branch from `main` as `hotfix/<short-description>` and are merged back to both `main` and `develop`.

See [`AGENTS.md`](AGENTS.md) for full branching rules and coding conventions.

## CI / CD

| Trigger | Workflow | Result |
|---------|----------|--------|
| Push or PR to `develop` | CI | Build check |
| Push to `main` | Release | Build + zip artifact |
| Tag `v*.*.*` on `main` | Release | Build + zip artifact + draft GitHub Release |
