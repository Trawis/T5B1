# AGENTS.md

Repository-level instructions for coding agents.

**Document role**: Managed coding-agent instructions
**Sync destination**: `AGENTS.md`

## Start Here

1. Follow the current user request.
2. Read `.agents/project.md` when it exists.
3. Before editing, read applicable child `AGENTS.md` files along the affected
   paths, including any the agent did not load automatically.
4. Read only the specialized guidance and project requirements needed for the
   task. Do not scan `docs/project/` by default.

Precedence: user request, applicable child instructions (closest first),
`.agents/project.md`, this file, then supporting guidance. Report conflicts.

## Ownership and Documentation Authority

- `docs/project/` is authoritative, project-owned documentation.
- `docs/templates/` is managed, read-only reference material. Never edit target
  templates or treat placeholders as live requirements.
- `.agents/project.md`, child `AGENTS.md` files, project documentation,
  workflows, and other project-owned paths stay project-owned.
- Add a child `AGENTS.md` only when a subtree needs narrower instructions. It
  overrides this file within its scope, not the reverse.
- Do not customize repo-seed managed files in target repositories. These
  include this file, `CLAUDE.md`, selected `.agents/` guidance, templates, and
  `scripts/sync-docs.py`.
- `.repo-seed-state.json` is managed sync state. Do not edit it manually.
- Untouched scaffolds and unconfirmed Draft or inferred content are not
  authoritative requirements.

If a change makes project-owned documentation inaccurate, update it.

## Working Rules

- Inspect the affected implementation and nearby tests before editing. Follow
  nearby established patterns. Consult other documentation, configuration, or
  conventions only when relevant to the change or needed to resolve an
  uncertainty.
- Make the smallest safe change that satisfies the task. Do not reformat,
  rename, or clean up unrelated code.
- Prefer source files over generated output.
- Write self-explanatory code. Do not add comments or docstrings unless they
  convey essential information not apparent from code or are required by the
  project. Never invent intent. Preserve required documentation, annotations,
  directives, and license notices.
- Prefer existing checks or a focused smoke test. Add permanent automated tests
  only for meaningful behavior or regression risks not already covered, or when
  the project requires them. Do not test trivial code, prose, formatting, or
  incidental implementation details just to increase coverage.
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

Use relevant native repository skills when supported. Skills supplement these
instructions. Load only guidance needed for the task:

- language or script work: `.agents/conventions/`
- Git, branches, commits, pull requests, or PR descriptions:
  `.agents/guidelines/git.md`
- CI/CD or releases: `.agents/guidelines/ci-cd.md`
- substantial documentation or technical designs: `.agents/guidelines/documentation.md`
- issue creation, labels, issue structure, or triage:
  `.agents/guidelines/issues.md`

Do not scan skill directories. Follow nearby code where consistent with the
applicable rules. Do not create or materially alter workflows, permissions,
publishing, deployment, or infrastructure unless explicitly requested.

When provided, explicit acceptance criteria and tickets define task scope.
FSDs govern observable application behavior, GDDs govern gameplay intent, and
accepted technical designs govern only their scoped changes. Report conflicts
between requirement sources before implementing.

## Validation and Completion

Run the narrowest relevant validation, such as focused tests, configured lint
or formatting checks, a build of the affected project, or a practical smoke
check. Do not run irrelevant language or project checks.

Before completion, review the final diff against the request for scope,
correctness, applicable local conventions, ownership boundaries, and relevant
validation. Remove unnecessary added comments, docstrings, and documentation.
Check branch and pull-request requirements only when performing Git
or pull-request operations. Report what changed and was validated, plus
skipped or blocked checks and remaining risks. Never claim an action or check
succeeded unless it did.
