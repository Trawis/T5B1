---
name: repository-audit
description: Audits a repository that uses or is expected to use repo-seed for drift from its installed state and reports an actionable, read-only findings list. Use when asked to audit, check, validate, compare, or find drift in a repo-seed repository, including managed files, profile and convention selection, stale artifacts, and competing agent guidance.
---

# Repository Audit

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
This skill covers repo-seed drift only. Use `code-review` for general code review
and do not repeat the coding guidance in `AGENTS.md` or `.agents/guidelines/`.

The audit is read-only. Do not edit files or run a writing sync unless the user
separately asks for fixes.

## Workflow

1. Establish the evidence. Read `.repo-seed-state.json` for the pack version,
   profile, convention selection, managed files, and tombstones. If it is
   missing, the repository has no recorded repo-seed install; say so and audit
   only what the user asked for against an explicitly requested profile.
2. Locate the pack to compare against: an extracted release archive or a
   repo-seed checkout containing `manifest.json` and `files/`. Use the one the
   user names. Do not download one without permission. Without a source pack,
   deterministic comparison is impossible, so report that and continue with
   the evidence you can inspect.
3. Run the repository's own sync script instead of reimplementing its logic.
   From the target, use `python scripts/sync-docs.py --source <pack> --audit`
   for drift diagnostics, then `--dry-run` with the same arguments to see what
   a sync would change. Both write nothing. Pass `--profile` or `--conventions`
   only to test a selection, and say that you did. Never run without `--audit`
   or `--dry-run`.
4. Classify every reported line and every other observation into the categories
   below. The script's output is the deterministic source for managed files,
   stale paths, and selection; add the checks it cannot make.
5. Look for competing or duplicated agent guidance the script does not judge:
   other agent instruction files that contradict or repeat managed guidance
   (for example a second root instruction file, a `CLAUDE.md` that is no longer
   a thin wrapper, or `.agents/project.md` restating managed rules), skills
   under `.agents/skills/` or `.claude/skills/` whose names or purposes
   overlap a managed skill, and differences between the two skill surfaces.
6. Open project-owned files only when needed to explain a reported mismatch.
7. Verify each candidate finding before reporting it. Do not report style-only
   points or speculative cleanup.

## Categories

- **Managed drift**: managed files that are missing, or whose content differs
  from the pack, including a target pack version behind the source.
- **Stale artifacts**: tombstoned or no-longer-managed paths awaiting review,
  and obsolete known repo-seed files still present.
- **Selection inconsistencies**: a profile or convention selection that is
  unrecorded, unknown, unavailable, or inconsistent with what is installed,
  such as an installed convention that is no longer selected.
- **Competing guidance**: duplicated or conflicting agent instructions.
- **Intentional project-owned differences**: customized scaffolds, README,
  CHANGELOG, `.agents/project.md`, child `AGENTS.md` files, workflows, GitHub
  templates, and `docs/project/`. These are not drift. List them separately and
  do not recommend overwriting them.
- **Unverifiable**: anything that needs human judgment, hosted state such as
  labels or branch protection, or evidence you could not obtain.

## Findings

Report concrete findings first, ordered by impact. For each actionable finding
give the affected path or state, the expected repo-seed behavior, the evidence
(the script line or file inspected), and the smallest useful correction, such as
the sync command that would resolve it. A managed file edited locally is
corrected by a sync, which overwrites the edit, so say that before recommending
it. Then list the intentional project-owned differences and the unverifiable
items. If nothing is wrong, say so.

## Completion

State the source pack and version compared, the exact commands run and whether
each succeeded, and any category you could not check. Never claim a comparison
was made when it was not. Do not create branches, commit, push, or change hosted
state.
