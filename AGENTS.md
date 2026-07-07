# AGENTS.md

Repository-level instructions for coding agents.

**Document role**: Managed coding-agent instructions
**Sync destination**: `AGENTS.md`
**Version**: 4.0.1

## Start Here

Before changing files:

1. Follow the current user request.
2. Read `.agents/project.md` when it exists.
3. Read the nearest child `AGENTS.md` for each path you may change.
4. Read applicable live requirements under `docs/project/`.
5. Load only the specialized guidance relevant to the task.

Instruction precedence is: user request, closest child `AGENTS.md`, `.agents/project.md`, this file, then supporting guidance. Report meaningful conflicts instead of guessing.

## Documentation Authority

- `README.md` and `CHANGELOG.md` are project-owned root entry points.
- `docs/project/` contains authoritative live project documentation.
- `docs/templates/` contains managed, read-only references.
- Never edit target templates or treat their placeholders as requirements.
- Untouched scaffolds and unconfirmed Draft or inferred content are not
  authoritative requirements.
- When behavior changes, update relevant project-owned documentation when practical.

Read `.agents/guidelines/documentation.md` for documentation work or changes affecting setup, behavior, architecture, features, guides, or specifications.

## Working Rules

- Inspect relevant implementation, tests, and instructions before editing.
- Make the smallest safe change that satisfies the task.
- Preserve existing architecture, APIs, naming, formatting, and user-facing style.
- Do not reformat, rename, or clean up unrelated code.
- Prefer source files over generated output.
- Keep comments focused on constraints or non-obvious intent.
- Add or update relevant tests for behavior changes when practical.
- For review or diagnosis, report evidence without making changes unless a fix is requested.

## Safety

Do not:

- expose, invent, rename, or commit secrets, credentials, tokens, certificates, or private keys;
- run destructive commands without explicit authorization;
- rewrite long-lived branch history, bypass protections, auto-merge, or approve your own pull request;
- hide failures, skipped checks, conflicts, or uncertainty;
- alter production deployment, infrastructure, schemas, authentication, payments, licensing, telemetry, or public contracts unless the task requires it;
- add or replace production dependencies without a clear requirement.

Ask before broad refactors, framework replacement, repository restructuring, or new cross-codebase patterns unless they are explicitly requested.

## Specialized Guidance

Use focused guidance only when applicable:

- documentation: `.agents/guidelines/documentation.md`
- Git and pull requests: `.agents/guidelines/git.md`
- CI/CD and releases: `.agents/guidelines/ci-cd.md`
- language and script conventions: `.agents/conventions/`

Repository-specific rules and nearby code always take precedence over managed conventions.

## Git and CI/CD

Follow the branching model documented by the repository or its hosted settings. When none exists, use the hosted default branch and a short descriptive task branch. Do not create a long-lived integration branch implicitly.

Before branch or pull-request work, check remote state and existing work when tooling is available. Keep branch names, commits, PR text, changelog entries, and release notes factual and free of assistant or tool names.

Do not create or materially alter workflows, permissions, publishing, deployment, or infrastructure unless explicitly requested. Preserve existing gates and state what was validated locally versus what requires hosted verification.

Read the relevant Git or CI/CD guideline before performing that specialized work.

## Requirements and Specifications

When provided:

- FSD controls observable behavior and acceptance criteria;
- GDD controls gameplay intent and player experience;
- architecture describes the verified current technical system;
- an accepted technical design under `docs/project/designs/` controls only its
  scoped change;
- tickets and explicit acceptance criteria define task scope.

Report conflicts between requirement sources before implementing.

## Validation

Run the narrowest relevant checks available:

1. build the affected project;
2. run focused tests;
3. run configured lint or format checks;
4. perform a practical smoke check;
5. review the diff against the request and live documentation.

Do not run irrelevant language or project checks. Report checks that could not run and why.

## Sync Ownership

Routine sync updates these files when their pack content differs:

- `AGENTS.md`, `CLAUDE.md`, and selected `.agents/` guidance;
- `docs/templates/`;
- `scripts/sync-docs.py`.

Routine sync retires only unchanged legacy-managed files whose hashes match the old manifest. Modified or unrecorded files, reclassified project-owned files, and legacy conflict output are preserved and reported.

Routine sync also maintains committed `.repo-seed-state.json` ownership
metadata. A smaller profile removes unchanged managed files that are no longer
selected. Modified stale files are preserved and remain tombstoned for review.
Do not edit this state file manually.

Do not customize these managed files in target repositories.

Project-owned files include `.agents/project.md`, child `AGENTS.md` files, root
`README.md` and `CHANGELOG.md`, `.editorconfig`, `.gitignore`, `docs/project/`,
`.github/workflows/`, and every unmapped path. The sync manifest cannot manage,
scaffold, retire, or delete workflow files. Scaffolding creates missing
project-owned files and may upgrade Markdown only while repo-seed provenance
proves it unchanged.

## Completion

Report:

- what changed;
- what was validated;
- skipped or blocked checks;
- remaining risks or follow-up.

Never claim an action or verification succeeded unless it actually did.
