---
name: documentation-bootstrap
description: Bootstraps accurate live project documentation for an existing repository from repository evidence. Use when asked to document an existing codebase, populate repo-seed project docs, create initial architecture/FSD/GDD/user guidance, or replace untouched documentation scaffolds with verified project content.
---

# Documentation Bootstrap

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists,
then read `.agents/guidelines/documentation.md`.

This skill is for documenting an existing repository from evidence, not for
inventing product intent.

## Workflow

1. Inspect only the evidence needed for the applicable documents: source, tests,
   configuration, schemas, workflows, assets, package metadata, existing docs,
   and useful Git history.
2. Determine which project documents actually apply. Do not create every
   available template as busywork.
3. Use managed templates as structure references, never as requirements.
4. Distinguish:
   - explicit intended behavior from accepted requirements;
   - verified as-built behavior from code/configuration/tests;
   - reasonable inference;
   - unknown or unverified information.
5. Populate live documents with current project truth. Cite concrete repository
   evidence for meaningful inferences where useful and do not promote inferred
   intent to an authoritative requirement without confirmation.
6. For applications, document accepted functional behavior only when supported
   by evidence or requirements. For games, do not infer intended player
   experience, balance goals, or future design from code alone.
7. Do not manufacture historical changelog entries, old technical designs,
   architecture decisions, roadmaps, or rationale.
8. Keep stable entry-point documents concise and move detail into subdocuments
   only when the content genuinely needs it.

## Validation

Verify commands, paths, component names, configuration, links, and documented
behavior against the repository. Check that living documentation describes the
current state rather than the documentation process.

## Completion

Report which documents were populated, what evidence they were based on, and
which important facts remain inferred or unknown.
