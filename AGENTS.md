# AGENTS.md

Repository-level instructions for coding agents.

**Document role**: Managed coding-agent instructions
**Sync destination**: `AGENTS.md`

## Start Here

1. Follow the current user request.
2. Read `.agents/project.md` when it exists.
3. Read the nearest child `AGENTS.md` when working in its scope.
4. Load specialized guidance only when its subject is relevant to the operation.
5. Read project requirements or documentation only when the task refers to them,
   they govern the affected behavior, or they are needed to resolve a concrete
   uncertainty. Do not scan `docs/project/` by default.

Instruction precedence is: user request, closest child `AGENTS.md`,
`.agents/project.md`, this file, then supporting guidance. Report meaningful
conflicts instead of guessing.

## Ownership and Documentation Authority

- `docs/project/` is authoritative, project-owned documentation.
- `docs/templates/` contains managed, read-only reference material. Never edit
  target templates or treat placeholders as live requirements.
- `.agents/project.md`, child `AGENTS.md` files, project documentation,
  workflows, and other project-owned paths remain project-owned.
- Repo-seed managed files, including this file, `CLAUDE.md`, selected
  `.agents/` guidance, templates, and `scripts/sync-docs.py`, must not be
  customized directly in target repositories.
- `.repo-seed-state.json` is managed sync state; do not edit it manually.
- Untouched scaffolds and unconfirmed Draft or inferred content are not
  authoritative requirements.

If a change makes existing project-owned documentation inaccurate, update the
affected documentation.

## Working Rules

- Inspect the affected implementation and nearby tests before editing. Follow
  nearby established patterns. Consult additional project documentation,
  configuration, or conventions only when relevant to the change or needed to
  resolve an uncertainty.
- Make the smallest safe change that satisfies the task. Do not reformat,
  rename, or clean up unrelated code.
- Prefer source files over generated output, and keep comments focused on
  constraints or non-obvious intent.
- Add or update relevant tests for behavior changes when practical.
- For review or diagnosis, report evidence without changing files unless a fix
  is requested.
- Do not expose secrets or conceal failures.
- Do not run destructive commands without authorization.
- Do not alter production infrastructure, schemas, authentication, payments,
  licensing, telemetry, public contracts, or production dependencies unless
  the task requires it.
- Ask before broad refactors, framework replacement, repository restructuring,
  or new cross-codebase patterns unless explicitly requested.

## Specialized Guidance

Load only the guidance relevant to the task:

- language or script work: `.agents/conventions/`
- Git, branches, commits, or pull requests: `.agents/guidelines/git.md`
- CI/CD or releases: `.agents/guidelines/ci-cd.md`
- substantial documentation work, including creation, restructuring,
  bootstrapping, technical designs or specifications, and significant
  maintenance: `.agents/guidelines/documentation.md`

Repository-specific rules and nearby code take precedence over managed
conventions. Do not create or materially alter workflows, permissions,
publishing, deployment, or infrastructure unless explicitly requested.

When provided, explicit acceptance criteria and tickets define task scope;
FSDs govern observable application behavior; GDDs govern gameplay intent; and
accepted technical designs govern only their scoped changes. Report conflicts
between requirement sources before implementing.

## Validation and Completion

Run the narrowest relevant validation, such as focused tests, configured lint
or formatting checks, a build of the affected project, or a practical smoke
check. Do not run irrelevant language or project checks.

Before completion, review the final diff against the request for scope,
correctness, applicable local conventions, ownership boundaries, and relevant
validation. Check branch and pull-request requirements only when performing Git
or pull-request operations. Report what changed and was validated, along with
skipped or blocked checks and remaining risks. Never claim an action or check
succeeded unless it did.
