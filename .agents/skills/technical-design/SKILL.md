---
name: technical-design
description: Investigates and writes a scoped technical design for a substantial proposed codebase change. Use when explicitly asked for a technical design/specification or when planning a change involving multiple components, public contracts, persistence, migration, security, concurrency, deployment, compatibility, high uncertainty, or difficult rollback.
---

# Technical Design

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists,
then read `.agents/guidelines/documentation.md`.

Do not implement the proposed change unless implementation is also requested.

## Workflow

1. Confirm the requested scope, acceptance criteria, constraints, and relevant
   requirement sources.
2. Inspect the existing implementation, architecture, tests, configuration,
   schemas, workflows, and contracts needed to describe the current system
   accurately.
3. Separate verified current state from proposed behavior and unresolved
   assumptions.
4. If a design was not explicitly requested, create one only when the change
   meets the repository's technical-design threshold in the documentation
   guidance.
5. Use `docs/templates/tsd.template.md` when it exists and fits the repository.
   Place a live design under `docs/project/designs/<short-name>.md` unless local
   project rules specify another location.
6. Cover the problem and goals, non-goals, current constraints, proposed
   components and data/control flow, affected contracts, persistence or
   migration, error/failure behavior, security and concurrency where relevant,
   rollout/backout, validation, risks, and open questions.
7. Prefer one clear recommended design. Include alternatives only when they
   represent meaningful tradeoffs.
8. Do not invent decisions, requirements, production behavior, or historical
   rationale. Mark unresolved facts as open questions.

## Completion

Check that the proposal is implementable, scoped, internally consistent, and
traceable to repository evidence or explicit requirements. Report unresolved
decisions that block implementation.
