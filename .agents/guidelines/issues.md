# Issue Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/issues.md`

This file is the single canonical source for issue creation, labels, issue
structure, and triage. Issue forms and other guidance agree with it instead of
defining a competing list. For Git, branches, commits, or pull requests, see
`.agents/guidelines/git.md`.

## Issue Entry Points

Every issue starts from one of four GitHub Issue Forms
(`.github/ISSUE_TEMPLATE/*.yml`), one per type:

- Bug Report (`bug_report.yml`, applies `type: bug`)
- Feature Request (`feature_request.yml`, applies `type: feature`)
- Tech Debt (`tech_debt.yml`, applies `type: tech-debt`)
- Chore (`chore.yml`, applies `type: chore`)

Required form fields enforce the minimum issue contract:

- Bug Report requires Summary, Steps to Reproduce, Expected Behavior, Actual
  Behavior, and Environment. Additional Context is optional.
- Feature Request, Tech Debt, and Chore require Summary, Goal, and Done when.
  Suggested approach is optional.

Forms do not add Dependencies, Risks, or implementation-note fields. Forms only apply in the web UI. Issues created through the API, the
GitHub CLI, or by maintainers can bypass them, so the structures below still
apply.

Documentation maintenance uses the Chore form. `blank_issues_enabled: false` in
`.github/ISSUE_TEMPLATE/config.yml` hides the blank-issue option so every issue
starts from a form. Do not change it to `true`.

## Canonical Type Labels

Every issue gets exactly one type label:

- `type: bug` - existing behavior is incorrect.
- `type: feature` - implementation-ready new behavior or capability.
- `type: tech-debt` - changes to the project's own code or architecture that
  improve its structure without changing behavior: refactoring,
  simplification, removing dead code, or architecture cleanup.
- `type: chore` - upkeep of the tooling and files around the code:
  dependencies, CI, build and release setup, repository configuration, or
  documentation maintenance.

When `tech-debt` and `chore` both seem to fit, use `tech-debt` if the work
changes the structure of the product's source. Use `chore` if it changes the
tooling, configuration, or documentation around it. A dependency upgrade is a
`chore` even when it needs code changes.

Each form applies exactly one type label. Change the label if triage
disagrees.

Do not replace `type:*` labels with GitHub's native issue types. The
convention must stay portable across personal accounts and organizations.

Labels named here may not exist on the hosted repository. Applying a missing
label through the API or CLI can fail or create an unstyled label; check the
hosted list first (see GitHub-Hosted Labels below).

## Unresolved Choices

Settle a choice that blocks implementation before work starts. List the
alternatives under `Suggested approach`, decide in the issue discussion, and
record the outcome in the issue. Do not open a separate issue type for it.

## Issue Structure

Normal implementation issues:

```md
## Summary

Short explanation of the problem or change.

## Goal

What should be true after this work is complete?

## Suggested approach

Optional implementation direction when useful.

## Done when

- Concrete acceptance condition.
- Concrete acceptance condition.
```

`Suggested approach` is optional. Small issues may use only `Summary`, `Goal`,
and `Done when`. Make acceptance conditions observable or testable where
practical. Do not force Context, Scope, Dependencies, Risks, Requirements, or
Implementation Notes onto every issue. Add one only when that
issue benefits.

Bug reports keep a dedicated diagnostic structure with reproduction, expected,
actual, and environment fields; see `.github/ISSUE_TEMPLATE/bug_report.yml`.

## GitHub-Hosted Labels

The labels above are the shared repository convention. Hosted GitHub labels
are outside repo-seed's file synchronization. When hosted label configuration
matters, verify it
directly through GitHub.

Legacy labels may remain on historical issues. They are reported, not deleted
automatically. Migrate them deliberately, as a separate step.

| Legacy label  | Canonical replacement |
|---|---|
| `bug`         | `type: bug` |
| `enhancement` | `type: feature` |
| `feature`     | `type: feature` |
| `critical`, `high`, `medium`, `low`, `lowest` | none (priority is not tracked by labels) |
| `priority: critical`, `priority: high`, `priority: medium`, `priority: low` | none (retired) |
| `type: decision` | `type: feature`, `type: tech-debt`, or `type: chore` (re-triage) |
| `type: idea` | `type: feature`, `type: tech-debt`, or `type: chore` (re-triage) |
