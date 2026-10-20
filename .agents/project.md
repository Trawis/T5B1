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

## Workshop Source Compile

The Steam Workshop item is compiled from `Trainer.Source` by the game's own compiler, which lists the files with
`Directory.GetFiles(AllDirectories)` and does not sort them (see `docs/project/architecture.md`, Workshop Source Build).

- A type that has a field of an enum type needs that enum to be declared before it in compile order: put the enum in a file
  of the same folder that sorts first (for example `DomainEnums.cs`), or above the type in the same file. Otherwise the
  game fails to load the mod with "Field '...' is an enum type with a bad underlying type" (#462).
- The parse-only tests cannot catch this; test it by loading the mod in the game.

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

The release/hotfix merge-back into `develop` also needs a PR (a short-lived
`feature/merge-back-<version>` branch), never a direct push, even if one would
succeed by bypassing branch protection.

Before branch or pull-request work, fetch and prune remotes, inspect the working tree and current branch, and check existing branches and pull requests to avoid duplicate work.
