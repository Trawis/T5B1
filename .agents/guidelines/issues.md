# Issue Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/issues.md`

This file is the single canonical source for issue creation, labels, issue
structure, and triage. Issue forms and other guidance agree with it instead of
defining a competing list. For Git, branches, commits, or pull requests, see
`.agents/guidelines/git.md`.

## Issue Entry Points

Contributors get exactly two GitHub Issue Forms
(`.github/ISSUE_TEMPLATE/*.yml`):

- Bug Report (`bug_report.yml`, applies `type: bug`)
- Feature Request (`feature_request.yml`, applies `type: feature`)

Required form fields enforce the minimum issue contract:

- Bug Report requires Summary, Steps to Reproduce, Expected Behavior, Actual
  Behavior, and Environment. Additional Context is optional.
- Feature Request requires Summary, Goal, and Done when. Suggested approach is
  optional.

Forms do not add Priority, Area, Dependencies, Risks, or implementation-note
fields. Forms only apply in the web UI. Issues created through the API, the
GitHub CLI, or by maintainers can bypass them, so the structures below still
apply.

Tech debt, chores, decisions, ideas, documentation, and questions have no
form. Maintainers create them as a blank issue and apply the `type:*` label
manually. `blank_issues_enabled: false` in `.github/ISSUE_TEMPLATE/config.yml`
only hides the blank-issue option from contributors; maintainers with write
access can still open one. Do not change it to `true`.

```text
Contributor:
  Bug Report
  Feature Request

Maintainer:
  Bug Report
  Feature Request
  Blank issue -> assign type manually
```

## Canonical Type Labels

Every issue gets exactly one type label:

- `type: bug` - existing behavior is incorrect.
- `type: feature` - implementation-ready new behavior or capability.
- `type: tech-debt` - refactoring, internal cleanup, architecture
  improvement, or accumulated maintenance debt.
- `type: chore` - tooling, dependencies, CI, repository maintenance, or
  documentation maintenance.
- `type: decision` - a concrete choice must be resolved before
  implementation.
- `type: idea` - worth recording but not yet defined enough to implement.

Forms need not map one-to-one to types. Bug and Feature are contributor entry
points. Maintainers apply the other types through a blank issue, for example:

```text
internal refactor        -> blank issue -> type: tech-debt
dependency cleanup       -> blank issue -> type: chore
architecture choice      -> blank issue -> type: decision
possible future feature  -> blank issue -> type: idea
```

Do not replace `type:*` labels with GitHub's native issue types. The
convention must stay portable across personal accounts and organizations.

## Priority Labels

Priority is optional. Do not require it, and do not add a priority field or
question to forms or issue bodies. Priority belongs only in a `priority:*`
label.

- `priority: critical`
- `priority: high`
- `priority: medium`
- `priority: low`

## Area Labels

Do not establish a mandatory shared `area:*` taxonomy. A project may add a few
component or area labels when they give real filtering value. Do not add one
just for completeness.

## Idea, Decision, and Feature Lifecycle

```text
idea -> decision (only when a real unresolved choice blocks implementation) -> feature / tech-debt / chore
```

An idea may stay incomplete indefinitely. Promote it to `type: decision` only
when a choice blocks implementation. Promote it to `type: feature`,
`type: tech-debt`, or `type: chore` once it is implementation-ready. Do not
create Idea or Decision forms; maintainers use a blank issue with the
structures below.

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
practical. Do not force Context, Scope, Dependencies, Risks, Priority,
Requirements, or Implementation Notes onto every issue. Add one only when that
issue benefits.

Decision issues:

```md
## Decision

What must be decided?

## Context

Relevant constraints or alternatives.

## Done when

- The decision is recorded.
- Follow-up implementation issues are created when needed.
```

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
| `critical`    | `priority: critical` |
| `high`        | `priority: high` |
| `medium`      | `priority: medium` |
| `low`         | `priority: low` |
| `lowest`      | `priority: low` |
