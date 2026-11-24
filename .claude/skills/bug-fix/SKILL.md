---
name: bug-fix
description: Diagnoses and fixes incorrect existing behavior with a root-cause-first workflow and regression validation. Use when asked to fix a bug, regression, crash, failing edge case, incorrect result, visual defect, or reproducible test failure.
---

# Bug Fix

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Load the relevant language conventions and specialized guidance the
repository provides for the affected code.

## Workflow

1. Build a feedback loop before theorizing. Use the cheapest signal that fails
   on this bug: an existing or new failing test, a focused command against a
   fixture, a captured input replayed in isolation, a differential run of two
   versions, or a concrete traced failure path when execution is unavailable.
   For a visual defect, a capture of the wrong state can be that signal. The
   loop must assert the reported symptom, not merely "runs without error".
   Run it once and keep its output.
2. Make the loop trustworthy: confirm it shows the symptom the user described
   rather than a nearby failure, make it deterministic (pin time and seeds,
   isolate state), and keep it fast. For intermittent bugs, raise the
   reproduction rate rather than accepting a flaky signal.
3. Minimize the reproduction by removing inputs, steps, and configuration one at
   a time until every remaining element is needed to fail.
4. If no loop can be built, state what was tried and ask for what is missing,
   such as access to the failing environment or a redacted captured artifact.
   Do not add temporary production instrumentation without permission.
5. Inspect the affected implementation, callers, tests, configuration, and
   recent relevant changes only as needed. Separate the root cause from
   downstream symptoms and trace bad values back to where they originate.
6. State two or more falsifiable hypotheses, each with the prediction that would
   confirm or reject it, before testing any. Test one variable at a time and
   keep any temporary debug output under a unique greppable marker.
7. Add or update a regression test when a seam exists that exercises the real
   failure path. If the only available seam is too shallow to catch the bug,
   say so instead of adding a test that gives false confidence. Confirm the test
   fails for the original defect before the fix.
8. Make the smallest coherent fix at the source. Do not combine it with
   unrelated cleanup or redesign.
9. Avoid compatibility wrappers, fallback branches, or legacy behavior unless a
   supported version, public contract, stored data format, or explicit
   requirement still needs them.
10. Re-run the original loop and the regression test, then the narrowest
    relevant broader validation. Remove temporary debug output and throwaway
    harnesses.

## Visual Defects

A visual defect is a wrong rendered result: overlapping or clipped layout,
misplaced or missing elements, wrong colors, or an incorrect state on screen.
Treat it as incorrect behavior, not as a cosmetic detail.

- Use the evidence the report already carries, such as screenshots, annotations,
  recordings, and reproduction steps, to establish both the reported symptom and
  the expected behavior. Ask for the viewport size, resolution, zoom, theme, and
  application state when a missing detail changes the result.
- Reproduce the defect before changing code, and compare before and after under
  the same conditions: viewport, resolution, zoom, theme, data, and state. A
  capture taken at another size or state is not evidence that the defect is
  fixed.
- A successful build or a passing test that never renders the affected view does
  not show that a visual defect is corrected. If the only available seam is too
  shallow to render it, say so instead of adding a test that gives false
  confidence.
- Keep the before and after artifacts with the run and describe what changed.
  When the result can be judged only by eye, that comparison is the validation.
- If visual verification is impossible, for example in a headless environment or
  without the affected device, report what was verified and what was not.
- Non-visual fixes need no screenshots, recordings, or visual tooling.

## Investigation Rules

- Do not guess at a cause because an error message appears nearby.
- Do not suppress exceptions, validation, or warnings merely to make a failing
  test pass.
- Do not weaken assertions or delete meaningful tests unless the expected
  behavior itself is proven wrong.
- If two fix attempts fail, stop adding fixes. Re-examine the assumptions behind
  the diagnosis, and if each fix exposes problems elsewhere, report a possible
  design-level cause instead of attempting a third patch.
- Redact secrets, tokens, and credentials from any command output, logs, or
  captured artifacts before showing them. Read credentials from the
  environment rather than writing them into scripts.
- Use Git history only when it helps explain a concrete regression or contract;
  do not scan history by default.

## Completion

Report the root cause, the fix, regression coverage, and validation performed,
including the before and after evidence when the defect was visual. Name the
confirmed hypothesis so later debugging benefits from it. Call out any
failure mode that could not be reproduced or any broader check that was
unavailable.
