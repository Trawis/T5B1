---
name: code-simplification
description: Simplifies working code for clarity while preserving its exact behavior. Use when asked to simplify, clean up, or reduce the complexity of code that works but is harder to read or maintain than necessary, such as deep nesting, nested ternaries, unclear names, duplication, dead code, or needless wrappers. Not for behavior changes, compatibility removal, or architecture redesign; use refactor for those.
---

# Code Simplification

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Load the language conventions the repository provides before editing.

The goal is code that is faster to understand, not fewer lines. Behavior stays
identical. This differs from `refactor`, which may restructure boundaries,
remove obsolete compatibility, or change internal design; if a change needs any
of that, stop and use `refactor` or report it.

## Workflow

1. Define the scope: recently changed code or what the user named. Do not
   simplify unrelated code on the way past.
2. Understand before touching. Learn the code's responsibility, its callers and
   callees, its edge cases and error paths, and the tests that define its
   behavior. Work out why it is written this way, since a strange construct may
   exist for performance, a platform limit, or a past bug. Use history only when
   that question stays open. If you cannot explain what the code does, read more
   first.
3. Match the project's conventions for naming, error handling, structure, and
   style by studying neighboring code and the repository guidance. Making code
   consistent with its surroundings is simplification; imposing a preference is
   churn.
4. Find concrete simplifications, such as:
   - deep nesting that guard clauses or a named helper would flatten;
   - nested ternaries or dense chained expressions that a plain conditional or
     named step would clarify;
   - boolean flag parameters, or the same condition repeated in several places;
   - generic, abbreviated, or misleading names;
   - comments that restate the code, while keeping comments that explain why;
   - real duplication, dead code verified to be unreachable or unused, wrappers
     that add nothing, and abstractions with a single user and no stable role.
5. Apply one simplification at a time and run the relevant tests after each. If
   a test fails, revert that change and reconsider rather than adjusting the
   test.
6. Keep the work separate from feature or bug-fix changes.
7. Review the whole diff. Confirm it is easier to understand, follows project
   conventions, and contains no unrelated changes. Revert any change that is
   not clearly better.
8. Finish with the repository's relevant broader checks, such as the full tests,
   build, and lint or format checks.

## Behavior Must Not Change

Before each change confirm it produces the same output for every input, the
same error behavior, and the same side effects and ordering. If you are not
sure, do not make the change. Existing tests should pass unmodified; needing to
change a test is a sign that behavior may have changed, so re-evaluate.

## Avoid

- Optimizing for line count or removing a helper that gave a concept a name.
- Merging unrelated logic, or replacing one abstraction with another.
- Removing error handling, validation, or an abstraction that exists for
  testability or extensibility.
- Simplifying code you do not fully understand, or performance-critical code
  where the clearer form is measurably slower.
- Speculative cleanup that no signal above justifies.

## Completion

Report what was simplified and why it preserves behavior, the tests and checks
run, any simplification skipped because behavior could not be confirmed
unchanged, and anything that needs `refactor`. Do not create branches, commit,
push, or change hosted state unless that was separately requested.
