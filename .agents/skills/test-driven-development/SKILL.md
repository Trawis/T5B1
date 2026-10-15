---
name: test-driven-development
description: Builds or fixes behavior test-first with a red-green-refactor loop over public, observable behavior. Use only when the user explicitly asks for TDD, test-first development, or red-green-refactor, or when another workflow deliberately calls for it. Do not apply it to every implementation.
---

# Test-Driven Development

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Load the language conventions and testing guidance the repository provides. The
repository's own test commands, frameworks, layout, and naming conventions are
authoritative; do not assume a language or framework.

This skill applies when test-first work was requested. Normal implementation
work stays free to choose the most suitable testing strategy.

## Workflow

1. Name the seams to test: the public boundaries where the behavior can be
   observed without reaching into internals. Prefer existing seams and keep them
   few. If the right seam is unclear or would change the design, ask before
   writing tests.
2. Work in vertical slices: one behavior, one test, one minimal implementation,
   repeated. Do not write all the tests first and then all the code.
3. Red: write one test for one behavior, and before writing it name the
   realistic production change that should make it fail. Run it and confirm it
   fails for the expected reason, such as missing behavior, not a typo, import
   error, or broken setup. A test that passes immediately does not test new
   behavior; fix or replace it.
4. Green: write only enough code to make that test pass. Do not add behavior
   that no test demands.
5. Run the focused test, then the other tests that could be affected. Fix
   failures in the code, not by weakening the test.
6. Refactor only while everything is green: remove duplication, improve names,
   and simplify structure without changing behavior. Re-run the tests after each
   step.
7. Repeat from step 3 for the next behavior, letting each cycle inform the next.
8. Before finishing, run the repository's full relevant validation. Name every
   failure it shows, including failures you did not cause.

## What Makes a Good Test

- It verifies behavior through the public interface and would survive an
  internal rewrite. Its name reads as a specification of what the code does.
- It derives expected values independently, from a literal, a worked example, or
  the requirement, never by recomputing them with the code under test.
- It checks outcomes callers can observe, not call counts, call order, private
  methods, or state read through a side channel such as a database query.
- It exercises real code. Substitute test doubles only at system boundaries such
  as external services, time, randomness, or slow I/O, and never for code you
  own. Assert on the real component, not on the double.
- It does not merely restate a constant or the exact wording of a message.
  Test the behavior that depends on the decision.

## Final Check

Before reporting, ask for each realistic mutation of the new code, such as a
wrong constant, wrong branch, missing state change, empty return, or missing
validation for empty or malformed input, whether at least one test would fail.
Add a test for any behavior left unprotected.

## Completion

Report the behaviors covered, the seams used, confirmation that each new test was
seen failing for the right reason, the checks run, and any behavior that could
not be tested first and why. Do not create branches, commit, push, or change
hosted state unless that was separately requested.
