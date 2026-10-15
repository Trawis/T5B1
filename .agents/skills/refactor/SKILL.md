---
name: refactor
description: Refactors existing code while preserving intended behavior and reducing unnecessary complexity, duplication, legacy paths, or weak abstractions. Use for cleanup, architectural simplification, modernization, compatibility removal, API-internal restructuring, or explicit refactoring work.
---

# Refactor

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Load any relevant language conventions the repository provides before editing.

## Workflow

1. Identify the behavior and contracts that must remain stable. Inspect affected
   callers, tests, configuration, persisted formats, and public interfaces.
2. Determine the supported runtime, framework, or dependency versions when the
   refactor involves compatibility code.
3. Define the simplification being made: fewer abstractions, clearer ownership,
   removed duplication, obsolete-path removal, simpler data flow, or a better
   boundary.
4. Prefer deleting obsolete layers and updating callers over adding forwarding
   wrappers, adapters, aliases, or parallel old/new paths.
5. Preserve behavior because it is required by a supported contract, not merely
   because the previous implementation happened to expose it.
6. Keep behavior changes separate from structural changes unless the task
   explicitly requires both.
7. Update tests to assert the intended behavior rather than the removed internal
   structure.
8. Run focused tests after each coherent change when practical, followed by the
   relevant broader validation.

## Guardrails

- Do not introduce a new abstraction unless it removes real duplication,
  isolates a real boundary, or represents a stable domain concept.
- Do not preserve deprecated compatibility for unsupported versions.
- Do not create speculative extension points.
- Do not rename, reformat, or reorganize unrelated code.
- If the requested refactor would materially change public behavior, surface
  that conflict rather than silently treating it as cleanup.

## Completion

Summarize the structural simplification, behavior preserved, obsolete code
removed, and validation performed.
