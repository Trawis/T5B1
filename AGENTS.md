# AGENTS.md

Repository-level instructions for AI coding agents.

**Version**: 1.22  
**Status**: Active  
**Last Updated**: 2026-06-19

**Recent changes**:
- Renamed the repo-seed sync script to the stable path `scripts/sync-agent-guidelines.py` so Git history tracks changes cleanly.
- Clarified that repo-maintained scripts use stable filenames; versioned suffixes are for distributed copies, generated outputs, and archive artifacts.
- Enforced strict Git Flow branch families: `feature/*` for all normal work, `release/*` for releases, `hotfix/*` for production fixes.
- Added `feature/sync-agent-guidelines-<version>` as the recommended branch for pack updates.
- Added explicit PR creation/proposal requirement for every task branch.
- Split Python and shell script conventions into separate files.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.22 | 2026-06-19 | Renamed the repo-seed sync script to a stable filename and clarified script filename/versioning guidance. |
| 1.21 | 2026-06-19 | Enforced strict Git Flow branch families and added repo-seed sync workflow/script guidance. |
| 1.19 | 2026-06-19 | Unified branch prefix selection, PR creation/proposal behavior, and fallback PR reporting. |
| 1.18 | 2026-06-18 | Clarified that single-statement C# guards may omit braces when the statement is on the next line; replaced project-specific terminal examples with generic examples. |
| 1.17 | 2026-06-18 | Added C# spacing rule requiring a blank line after a completed control block before the next independent statement. |
| 1.16 | 2026-06-18 | Added UI/output text style preservation rule for Unicode/ASCII punctuation and decorative separators. |
| 1.15 | 2026-06-18 | Added C# control-flow convention forbidding inline guard statements such as `if (...) return ...;`. |
| 1.14 | 2026-06-15 | Added instruction strictness levels, no-AI-name Git artifact rules, indentation preservation guidance, and clearer solo/local workflow handling. |
| 1.13 | 2026-06-12 | Added Claude Code compatibility through `CLAUDE.md`, cleaned version history, and softened PR workflow wording for solo/local repositories. |
| 1.12 | 2026-06-12 | Baseline public starter pack with `AGENTS.md`, `README.md`, `CHANGELOG.md`, `FEATURES.md`, `.editorconfig`, PR template, coding convention docs, and FSD/TSD/GDD templates. |

---

## Instruction Strictness

Use these levels when applying this file:

- `MUST`: mandatory safety, correctness, repository hygiene, or user-instruction rule. Do not ignore it unless the user explicitly overrides it in the current task or it is impossible in the environment.
- `SHOULD`: preferred default. Follow it when practical, but preserve existing repository conventions when they clearly differ.
- `MAY`: optional guidance for mature projects, larger repositories, or future improvements.

Defaults:

- Safety, secrets, destructive commands, branch protection, no auto-merge, and honesty about validation are `MUST`.
- Code style, formatting preferences, and documentation updates are usually `SHOULD` unless the repository makes them mandatory. Git Flow branch family and PR-target rules are `MUST` when this pack is used as the repository workflow source of truth.
- New child `AGENTS.md` files, new convention files, extra templates, and stricter automation are `MAY` unless requested.

When a rule is too strict for a solo/local repository, keep the intent: isolate the change, document the intended branch/PR, validate what is practical, and never pretend unavailable workflow steps were completed.

---

## Purpose

This file is the source of truth for AI coding agents working in this repository.

Use it as operational guidance, not as a general programming tutorial. Follow these instructions unless a more specific instruction exists in a closer `AGENTS.md` file or the user explicitly overrides them in the current task.

---

## Claude Code Compatibility

`AGENTS.md` is the primary source of truth.

For Claude Code, use the included `CLAUDE.md` wrapper. It imports this file so the same rules can be shared without duplicating instructions.

Do not copy all rules into `CLAUDE.md`. Keep `CLAUDE.md` short and use it only for Claude-Code-specific notes or imports.

---

## Instruction Scope and Precedence

Use the most specific trusted instructions available for the files being changed.

Precedence order:

1. Current user instructions for the task.
2. The closest `AGENTS.md` in the target repository or subdirectory.
3. Parent `AGENTS.md` files, moving upward toward the repository root.
4. General coding knowledge and existing local code style.

Rules:

- Prefer small, targeted changes over broad rewrites.
- Do not reformat unrelated files.
- When editing files in multiple subprojects, check the applicable instructions for each changed path.
- If no repo-specific instructions exist for a subpath, use this file and preserve the existing local style.

---

## Minimal Context Rule

- Apply only the rules relevant to the files touched by the task.
- Do not rewrite existing code only to satisfy style preferences unless the task asks for cleanup.
- Do not make broad architecture, formatting, dependency, or naming changes as a side effect.
- Prefer the smallest safe change that solves the requested problem.
- When in doubt, preserve the existing local style of the file being edited.
- Preserve existing indentation style in touched files: use spaces if the file/repo uses spaces, and tabs if it uses tabs.
- Preserve existing user-facing text style in touched files, including ASCII vs Unicode punctuation, decorative separators, quote style, symbols, labels, and terminal/UI output formatting.

---

## Agent Operating Rules

- Understand the task and inspect the relevant files before editing.
- Check for applicable convention files under `docs/` before editing code, scripts, or docs.
- Prefer small, targeted changes over broad rewrites.
- Do not reformat unrelated files.
- Preserve existing indentation style in modified files. Do not convert spaces to tabs or tabs to spaces unless the task is explicitly formatting/style cleanup.
- Preserve existing punctuation and text formatting style in modified user-facing strings.
- Do not rename public APIs, files, projects, branches, packages, or namespaces unless the task requires it.
- Do not add new production dependencies unless the task clearly requires them.
- Preserve existing architecture, project boundaries, and naming patterns.
- Add or update unit tests when feasible, especially for behavior changes, bug fixes, validation logic, and edge cases.
- If tests are not added, explain why they were not practical or useful for the change.
- Always try to build the project when a build command exists and the change affects buildable code.
- Always try to run relevant tests when test commands or test projects exist.
- Recheck the implemented behavior against the task and changed files before finishing.
- If checks cannot be run, state exactly why.
- If the implementation is uncertain, requirements are ambiguous, or validation cannot prove the behavior, ask or clearly report the uncertainty instead of pretending it is complete.
- Report what changed, what was validated, and any remaining risks.
- Do not include AI assistant, tool, or model names in branch names, commit messages, PR titles, PR descriptions, changelog entries, release notes, generated helper text, or user-facing documentation unless the task is explicitly about AI tooling or these agent-guideline files.
- Never hide failing tests, build errors, skipped checks, or uncertainty.

Ask before:

- broad refactors
- changing architecture
- replacing libraries/frameworks
- changing project structure
- renaming public APIs
- changing persistence models
- changing CI/CD or deployment behavior
- applying a new pattern across the codebase

---

## Repository Setup and Validation Commands

Before finishing work, run the narrowest relevant checks available in the repository.

Validation priority:

1. Build the affected project when a build command exists.
2. Run relevant automated tests when they exist.
3. Run lint/format checks when configured.
4. Recheck changed behavior against the task.

### .NET / C# Defaults

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test
```

If formatting changes are expected, run:

```bash
dotnet format
```

Then re-run build/tests if code was changed.

---

## Project Structure

This is a C# .NET project — a game trainer mod for Software Inc. (Beta 1).

- Solution file: `Trainer v5 - Beta 1.sln`
- Source project: `Trainer_v5/Trainer_v5.csproj`
- Source code: `Trainer_v5/Trainer.Source/`
- Libraries: `Trainer_v5/Trainer.Libraries/`
- Localization: `Trainer_v5/Trainer.Localization/`

---

## Repository Documentation

Keep project documentation aligned with behavior, setup, commands, public features, and versioned output.

- Update `README.md` when changing setup, build, run, or usage steps.
- Update `CHANGELOG.md` when adding, changing, fixing, or removing user-facing features. Use newest version first. Do not dump raw git commits.
- Update `FEATURES.md` when features are added, removed, or change state.
- Do not invent features, claims, performance numbers, or compatibility guarantees.

---

## Artifact and Bundle Naming

Use stable, searchable filenames for generated artifacts, bundles, and release packages.

- Do not use vague suffixes such as `final`, `final2`, `new`, `latest`, `copy`, `fixed`, or `v2`.
- Use `MAJOR.MINOR.PATCH` format.
- Keep the artifact name prefix stable across versions.
- Scripts committed to this repository use stable filenames. Versioned suffixes are for distributed standalone copies, generated output files, and archive artifacts.

Pattern: `<artifact-name>_<major>.<minor>.<patch>.<extension>`

---

## Code Conventions

Detailed code conventions live in separate files under `docs/`:

- C#/.NET: `docs/coding-conventions-csharp.md`
- Shell scripts: `docs/coding-conventions-shell.md`
- Python scripts: `docs/coding-conventions-python.md`
- General script guidance: `docs/coding-conventions-scripts.md`

Rules:

- Follow the nearest applicable convention file for the files being edited.
- Preserve existing local style when it conflicts with a generic convention and the task is not style cleanup.
- Preserve existing indentation style: tabs in this repo for C# files.
- Do not rewrite unrelated code just to satisfy conventions.

---

## Git Workflow

This repository uses strict Git Flow. Normal work must flow through `develop`; `main` is reserved for released/production-ready code.

### Long-Lived Branches

- `main` — production branch (releases merged here)
- `develop` — integration branch (all feature work targets here)

### Strict Git Flow Branch Families

| Branch family | Base branch | PR target | Use for |
|---------------|-------------|-----------|---------|
| `feature/*` | `develop` | `develop` | All normal planned work, including implementation, docs, tests, refactors, tooling, and non-emergency fixes |
| `release/*` | `develop` | `main` | Release preparation, version bumps, release notes, final stabilization |
| `hotfix/*` | `main` | `main` | Urgent fixes for production/released code |

Do not create `docs/*`, `test/*`, `refactor/*`, `chore/*`, or `bugfix/*` branches. Use `feature/*` for all normal work, even when the change is documentation-only, test-only, maintenance, or a non-emergency bug fix.

### Branch Naming

```text
feature/<short-kebab-description>
release/<major>.<minor>.<patch>
hotfix/<short-kebab-description>
```

Examples:

```text
feature/add-money-button
feature/fix-no-stress-toggle
feature/update-agent-guidelines
feature/sync-agent-guidelines-1-22-0
release/5.3.0
hotfix/fix-crash-on-load
```

### Pull Request Rules

Every task branch MUST create or propose a pull request. Do not skip PR creation.

- Do not auto-merge pull requests.
- Do not approve your own pull request.
- Do not bypass branch protection.

Default PR targets:

- `feature/*` targets `develop`.
- `release/*` targets `main`; changes must also be brought back to `develop` after merge.
- `hotfix/*` targets `main`; fix must also be brought back to `develop` after merge.

### No AI Names in Git Artifacts

Do not include AI assistant, tool, provider, or model names in branch names, commit messages, PR titles, PR descriptions, changelog entries, or release notes.

Write Git artifacts as if authored by the repository maintainer.

Allowed exception: files whose purpose is AI-agent configuration or documentation, such as `AGENTS.md` and `CLAUDE.md`.

### Commit Rules

- Keep commits focused and logically grouped.
- Use imperative commit messages.
- Do not include secrets, credentials, tokens, local paths, machine-specific files, or AI assistant/model/tool names.

Examples:

```text
Add money button feature
Fix no-stress toggle
Sync agent guidelines
```

---

## Syncing This Pack

The sync script at `scripts/sync-agent-guidelines.py` can update guideline files from a central source repository.

```bash
python /path/to/repo-seed/scripts/sync-agent-guidelines.py --source /path/to/repo-seed --target . --dry-run
python /path/to/repo-seed/scripts/sync-agent-guidelines.py --source /path/to/repo-seed --target .
```

Recommended branch for syncing: `feature/sync-agent-guidelines-<version>`

Syncing must not auto-commit, auto-push, create a PR, or auto-merge. Review the diff, resolve any conflicts, run relevant checks, then commit and open a PR to `develop`.

---

## Command Safety

Do not run destructive commands unless explicitly instructed.

Examples of destructive commands:

```bash
git reset --hard
git clean -fd
git push --force
git branch -D <branch>
rm -rf <path>
```

If a destructive action seems necessary, explain the reason and ask first.

---

## Boundaries

Do not modify without explicit instruction:

- secrets, credentials, tokens, certificates, or `.env*` files
- production deployment files
- CI/CD pipelines
- generated code
- lock files
- package references or dependency versions
- public API contracts
- authentication/authorization behavior

Do not commit secrets or machine-specific files.

---

## Dependency Policy

- Do not add production dependencies unless explicitly required by the task.
- Prefer built-in language/framework APIs before adding packages.
- If a dependency is necessary, choose a maintained package and explain why.
- Do not change package versions as part of unrelated work.

---

## Agent Completion Checklist

Before finishing a task, confirm:

- Applicable convention docs under `docs/` were checked for changed code/scripts/docs.
- Existing indentation style was preserved in modified files (tabs for C#).
- Existing user-facing text/output style was preserved, including ASCII vs Unicode punctuation and decorative separators.
- C# control-flow style was preserved or applied: no inline `if (...) return ...;`, braces required for multi-statement blocks, and blank line after a completed control block before the next independent statement.
- Branch family follows strict Git Flow: `feature/*`, `release/*`, or `hotfix/*`.
- A new task branch was created, or branch creation was impossible and the reason is reported.
- Changes are focused on the requested task.
- New behavior has unit tests when feasible, or a clear explanation why tests were not added.
- The affected project was built when possible, or skipped with a clear reason.
- Relevant automated tests were run when available, or skipped with a clear reason.
- The implementation was rechecked against requirements and changed files.
- `README.md`, `CHANGELOG.md`, or `FEATURES.md` were updated when the change affected public behavior, setup, commands, or features.
- A pull request was created, or exact PR instructions were provided.
- PR title, branch name, source branch, and target branch follow strict Git Flow conventions.
- Branch names, commit messages, PR text, changelog entries, and helper text do not contain AI assistant/model/tool names.
- The PR was not auto-merged.

---

## PR Description Template

Use `.github/pull_request_template.md` when it exists. Otherwise, use this structure:

```markdown
## Summary
- 

## Changes
- 

## Documentation
- [ ] README.md updated if needed
- [ ] CHANGELOG.md updated if needed
- [ ] FEATURES.md updated if needed

## Validation
- [ ] `dotnet format --verify-no-changes`
- [ ] `dotnet build`
- [ ] `dotnet test`
- [ ] Smoke/manual check where practical
- [ ] Implementation rechecked against requirements

## Branching / Merge Safety
- [ ] Branch follows `feature/*`, `release/*`, or `hotfix/*`
- [ ] PR targets the correct Git Flow branch
- [ ] No auto-merge requested/performed

## Notes / Risks
- 
```

---

## Updating This File

Update `AGENTS.md` when:

- project structure changes
- build/test/lint commands change
- recurring agent mistakes are discovered
- new architectural boundaries are introduced

Keep updates short, operational, and specific. Detailed style rules belong in separate files under `docs/`.
