---
name: code-review
description: Reviews diffs, pull requests, patches, or existing code for correctness, regressions, security, maintainability risks, and missing validation. Use when asked to review, audit, critique, inspect a change, find defects, or assess code without implementing fixes by default.
---

# Code Review

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
When the reviewed code requires them, load any language conventions or
specialized guidance the repository provides.

Do not modify files, issue or pull request state, labels, comments, or other
hosted resources unless the user separately asks for those actions.

## Workflow

1. Establish the review scope: changed files, diff or comparison base, requested
   behavior, and any acceptance criteria. When reviewing a branch or range,
   compare against the merge-base with a fixed reference, and confirm the
   reference resolves and the diff is non-empty before reviewing.
2. Inspect the changed implementation together with relevant callers, callees,
   tests, configuration, schemas, and public contracts. Do not review a diff in
   isolation when surrounding behavior determines correctness.
3. Prioritize concrete defects: incorrect behavior, regressions, data loss,
   security issues, broken compatibility, concurrency problems, resource leaks,
   unsafe error handling, and meaningful performance risks.
4. Distinguish actionable defects from preferences. Do not report style nits
   already handled by automated formatting or unsupported hypothetical risks.
   Where the repository documents coding standards (`AGENTS.md`,
   `CONTRIBUTING.md`, conventions), cite the rule for any violation, and label
   design-smell observations as judgement calls rather than defects; a
   documented repository rule always overrides a general smell.
5. Check whether tests exercise the changed behavior, what it must prevent, and
   important failure or boundary paths. For a bug fix, look for a regression test
   that failed before the change and passes after it; if the comparison cannot
   be run, use the available evidence and state that limit. Report missing
   coverage only when it creates a concrete regression risk.
6. Validate suspected findings with repository evidence or the narrowest useful
   checks when practical. Do not present speculation as a confirmed defect.
7. If a governing issue or explicit acceptance criteria exist, also complete the
   acceptance review below.

## Acceptance Review

Apply this only when the change is issue-driven or the user supplied explicit
acceptance criteria. For an arbitrary file, diff, or pull request with no
governing requirement, skip it and do not go looking for an issue.

1. Identify the governing issue from the request, pull request, branch, or
   commit references, and read the complete issue and relevant discussion when
   available. Do not rely only on the pull request description or the diff.
   If it cannot be read, say so.
2. Take its `Done when` list or the stated criteria as the contract to
   evaluate. A linked issue is not necessarily correct: when current repository
   evidence contradicts its assumptions, report the discrepancy. Distinguish an
   unexplained departure from the issue from an intentional, justified change;
   a documentation edit alone does not justify one.
3. Classify each condition as satisfied, unsatisfied, or not verifiable, with
   the evidence: the code, test, or check that shows it, or what is missing.
4. Report scope divergence when required work is omitted or the change adds
   material unrelated behavior. Differences in implementation detail that still
   meet the condition are not scope violations.
5. An unsatisfied condition is a finding only when it is a concrete
   implementation gap; do not inflate severity because wording differs. A
   not-verifiable condition is a validation gap, not a defect.

## Findings

Report findings before general commentary, ordered by severity. Keep
conformance to documented standards separate from correctness and requirement
findings so one cannot mask the other. Acceptance coverage supplements real
defect findings and never replaces them.

For each finding include:

- the affected file and location;
- the concrete failure mode or user impact;
- why the current code causes it;
- the smallest useful direction for fixing it.

If there are no actionable findings, say so explicitly and mention any relevant
validation gaps or residual risks.

For an issue-driven review, add an acceptance coverage list after the findings:
each condition with its status and evidence, followed by any scope omissions or
unrelated additions.

## Completion

State what was reviewed, including the governing issue when one was used, and
which checks were run. Make acceptance coverage visible next to the findings and
validation gaps. Never claim code, tests, hosted state, or runtime behavior was
inspected when it was not.
