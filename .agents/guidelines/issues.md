# Issue Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/issues.md`

This file is the single canonical source for issue creation, labels, issue
structure, and triage. Issue forms and other guidance agree with it
instead of defining a competing list. It does not cover Git, branches,
commits, or pull requests; see `.agents/guidelines/git.md` for those.

## Issue Entry Points

Contributors get exactly two GitHub Issue Forms
(`.github/ISSUE_TEMPLATE/*.yml`):

- Bug Report (`bug_report.yml`, applies `type: bug`)
- Feature Request (`feature_request.yml`, applies `type: feature`)

Required form fields enforce the minimum issue contract in the GitHub
issue-creation UI:

- Bug Report requires Summary, Steps to Reproduce, Expected Behavior, Actual
  Behavior, and Environment; Additional Context is optional.
- Feature Request requires Summary, Goal, and Done when; Suggested approach
  is optional.

Forms do not add Priority, Area, Dependencies, Risks, or implementation-note
fields. Forms only apply in the web UI: issues created through the API, the
GitHub CLI, or by maintainers can bypass them, so required fields are not a
guarantee and the structures below still apply.

There is no dedicated form per type label, and none for tech debt, chores,
decisions, ideas, documentation, or questions/support; those are maintainer
classifications created as a blank issue with the applicable `type:*` label
applied manually. `blank_issues_enabled: false` in
`.github/ISSUE_TEMPLATE/config.yml` only disables the blank-issue option for
contributors; maintainers with write access can still open one. Do not
change it to `true`.

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

Forms do not need a one-to-one relationship with types. Bug and Feature
are contributor entry points; the remaining types are mainly maintainer
classifications applied through a blank issue, for example:

```text
internal refactor        -> blank issue -> type: tech-debt
dependency cleanup       -> blank issue -> type: chore
architecture choice      -> blank issue -> type: decision
possible future feature  -> blank issue -> type: idea
```

Do not replace `type:*` labels with GitHub's native issue types; the shared
convention must stay portable across personal accounts and organizations
without requiring an organization-only feature.

## Priority Labels

Priority is optional. Do not require every issue to carry one, and do not
add a priority field or question to issue forms or bodies; priority
belongs only in the corresponding `priority:*` label.

- `priority: critical`
- `priority: high`
- `priority: medium`
- `priority: low`

## Area Labels

Do not establish a mandatory shared `area:*` taxonomy. A project may add a
small number of project-specific component or area labels when they provide
real filtering value. Do not create one merely for completeness.

## Idea, Decision, and Feature Lifecycle

```text
idea -> decision (only when a real unresolved choice blocks implementation) -> feature / tech-debt / chore
```

An idea may remain incomplete indefinitely; not every idea needs a decision
step. Promote it to `type: decision` only when a choice blocks
implementation, and to `type: feature`, `type: tech-debt`, or `type: chore`
once it is implementation-ready. Do not create dedicated Idea or Decision
forms; maintainers create these from a blank issue using the lightweight
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

`Suggested approach` is optional. Small issues may use only `Summary`,
`Goal`, and `Done when`. Acceptance conditions should be observable or
testable where practical. Do not force sections such as Context, Scope,
Dependencies, Risks, Priority, Requirements, or Implementation Notes onto
every issue; add one only when the individual issue benefits from it.

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

Bug reports keep a dedicated diagnostic structure with reproduction,
expected, actual, and environment fields; see
`.github/ISSUE_TEMPLATE/bug_report.yml`.

## GitHub-Hosted Labels

The labels defined above are the shared repository convention.

Actual GitHub-hosted labels are external to repo-seed's normal file
synchronization. When hosted label configuration matters, verify it
directly through GitHub.

Legacy labels may remain on historical issues and should be migrated
deliberately rather than deleted automatically.

Legacy shared labels map to the canonical catalog as follows:

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

Legacy labels are reported, not deleted automatically: they may still be
attached to historical issues, and removing them is a separate, deliberate
migration.
