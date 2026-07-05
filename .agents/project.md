# T5B1 Project Instructions

## Project

T5B1 is a C# .NET game trainer mod for Software Inc. (Beta 1).

- Solution: `Trainer v5 - Beta 1.sln`
- Source project: `Trainer_v5/Trainer_v5.csproj`
- Source code: `Trainer_v5/Trainer.Source/`
- Libraries: `Trainer_v5/Trainer.Libraries/`
- Localization: `Trainer_v5/Trainer.Localization/`

Preserve the existing architecture, public APIs, project boundaries, naming, tabs in C# files, and user-facing text style.

## Validation

Run the narrowest relevant checks before finishing:

```bash
dotnet format --verify-no-changes
dotnet build "Trainer v5 - Beta 1.sln"
dotnet test "Trainer v5 - Beta 1.sln"
```

Verify that test projects exist before treating a successful `dotnet test` command as test coverage. Report environment-blocked checks separately from code failures.

## Project Documentation

- Update `README.md` for setup, build, run, or usage changes.
- Update `CHANGELOG.md` for user-facing additions, changes, fixes, or removals.
- Update `FEATURES.md` when feature availability or status changes.
- Do not invent features, compatibility claims, or performance results.

## Git Flow

This repository uses strict Git Flow:

| Branch family | Base | Pull request target | Purpose |
|---|---|---|---|
| `feature/*` | `develop` | `develop` | Normal implementation, documentation, tests, maintenance, and non-emergency fixes |
| `release/*` | `develop` | `main` | Release preparation and stabilization |
| `hotfix/*` | `main` | `main` | Urgent production fixes |

Do not create other task-branch families. Release and hotfix changes must be brought back to `develop` after merging. Every task branch must create or propose a pull request; never auto-merge or approve your own pull request.

Before branch or pull-request work, fetch and prune remotes, inspect the working tree and current branch, and check existing branches and pull requests to avoid duplicate work.
