---
name: issue-implementation
description: Implements a specific repository issue or ticket from current repository evidence through focused changes and explicit acceptance-condition validation. Use when asked to implement, work on, complete, address, or fix a numbered issue or ticket.
---

# Issue Implementation

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Read `.agents/guidelines/issues.md` for the issue contract this skill consumes.
Load the language conventions and guidance for the affected code.

The issue defines the requested scope and acceptance conditions. The live
repository is the source of truth for current behavior. When they conflict,
keep the requested outcome, report the conflict, and do not redesign unrelated
areas.

## Workflow

1. Read the complete governing issue and its relevant discussion, and state its
   title and number before starting. If the reference is ambiguous, ask.
2. Verify before editing, and stop and report if any check fails:
   - the issue is still open and current, unless the user asked for historical
     or replay work;
   - the requested behavior does not already exist on the current branch;
   - no open pull request, active branch, or other issue already owns the same
     outcome;
   - dependencies and blocking decisions named by the issue are resolved;
   - the issue's assumptions still match the current implementation, tests,
     documentation, configuration, and workflows.
   If the tracker cannot be reached, say which of these checks were skipped.
3. Judge whether the issue is implementation-ready. If acceptance conditions are
   missing, untestable, or depend on an unmade product or design decision, report
   what is missing rather than inventing it.
4. Choose the specialized workflow that fits and follow it instead of restating
   it here: a defect or regression uses `bug-fix`; internal restructuring uses
   `refactor`; a dependency or toolchain change uses `dependency-upgrade`;
   substantial unresolved design work uses `technical-design`, or is reported as
   not yet implementation-ready. If a named skill is unavailable, apply its
   intent with the repository's own guidance.
5. Make the smallest coherent change that satisfies the issue. Do not add
   unrelated cleanup, dependency churn, renaming, reformatting, or architecture
   changes. If the issue prescribes an approach the repository has since
   outgrown, deliver the outcome by the current approach and report the
   difference.
6. Add or update focused tests for the changed behavior where practical. Never
   weaken existing validation or tests to make the issue pass.
7. Run narrow checks while working, then the validation the repository requires
   for the files changed, plus the narrowest broader checks needed for
   confidence.
8. Check every `Done when` or acceptance condition individually against the
   final state. Mark each as satisfied, unsatisfied, or not verifiable, with the
   evidence or the reason.
9. Review the final diff against the issue for scope, correctness, and
   accidental changes, and fix concrete problems before reporting.

## Permissions

A request to implement authorizes the working-tree edits that implementation
needs and nothing else. Do not create or switch branches, commit, push, open or
update pull requests, merge, comment on or close issues, or change hosted
settings unless the user explicitly asks or the applicable repository or user
instructions clearly authorize it. Finishing the local implementation does not
close the governing issue; the hosted issue lifecycle is a separate action.

## Completion

Report what changed, each acceptance condition with its status, the checks run
and their results, any skipped or blocked validation and why, conflicts between
the issue and the repository, and remaining risks or follow-up. Never claim a
check, tracker state, or condition was verified when it was not.
