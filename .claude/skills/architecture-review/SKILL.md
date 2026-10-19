---
name: architecture-review
description: Reviews the architecture of an existing codebase and reports a small, ranked set of evidence-backed friction points, without designing a new system or changing files. Use when asked to review, audit, or assess a codebase's architecture, find coupling, shallow abstractions, weak test seams, or structure that makes routine changes spread. Use technical-design to design a specific proposed change.
---

# Architecture Review

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Load the language conventions the repository provides for the code under review.

This skill evaluates the system as it is. It is read-only: do not edit files,
and do not implement a recommendation unless the user separately asks for it.
It does not design a replacement. When a recommendation needs a substantial
design, hand it to `technical-design`.

## Workflow

1. Set the scope. Use the module, subsystem, or pain point the user named. If
   none was named, look at recent history for the areas that change most and
   start there, widening only if changes are scattered. Use history for this
   purpose only.
2. Read what the project already says about its architecture before judging it:
   architecture and design documents, accepted decisions, and agent guidance.
   Do not re-raise a settled decision unless the friction it causes is real, and
   if you do, say that it contradicts the decision and why that is worth
   revisiting.
3. Inspect the implementation, tests, configuration, and public contracts in
   scope. Trace a few routine changes, such as adding a case or fixing a typical
   bug, to see how many places they touch and where understanding one concept
   forces jumping between many small modules.
4. Look for concrete friction:
   - callers carrying complexity that belongs behind a stable interface;
   - excessive coupling or dependencies pointing the wrong way;
   - abstractions that add surface area without hiding useful complexity, which
     the deletion test exposes: if removing it would only move the complexity
     rather than concentrate or eliminate it, it earns its place, and if
     removing it makes the complexity vanish, it is a pass-through;
   - the same responsibility owned in several places;
   - missing or poor test seams, such as logic reachable only through its
     internals, or behavior that tests cannot observe through a public boundary;
   - structure that makes routine changes spread more than they should.
5. Verify each candidate against the code before reporting it. Cite the files
   and symbols involved and the routine change it makes harder. Separate what
   you verified in the current code from what is only a possible improvement.
6. Apply seam discipline to any recommendation: an interface with one
   implementation and no realistic second one is indirection, not a seam. Do
   not turn a preference, a fashionable pattern, or speculative future scale
   into a defect.
7. Rank a small set of high-leverage findings, usually three to five. Leave out
   style complaints and low-value tidying.

## Report

Give the findings in the response, not as a file or a generated report. For
each finding include:

- a short title and its status: verified friction, or a possible improvement;
- the evidence: files, symbols, and the observed behavior or routine change;
- why it matters, in terms of change cost, testability, or risk;
- the direction a fix could take, without specifying a new interface;
- the cost or risk of acting, and any accepted decision it conflicts with.

Then state which finding you would address first and why, and which areas you
reviewed and found sound. If nothing significant turned up, say so rather than
manufacturing findings.

## Completion

State the scope reviewed, what was inspected, and what could not be verified.
Name any finding that needs a `technical-design` before work starts. Do not
create branches, commit, push, or change hosted state.
