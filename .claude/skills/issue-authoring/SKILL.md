---
name: issue-authoring
description: Drafts a repository-grounded issue from a request, defect, idea, or finding using the canonical issue guidance. Use when asked to create, write, draft, or file an issue, turn a finding into an issue, capture a bug, feature, or tech-debt item, or make work implementation-ready.
---

# Issue Authoring

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists,
then read `.agents/guidelines/issues.md`. That file is the only source for issue
types, labels, and structure; do not restate or extend its taxonomy here.

Drafting is read-only. Do not create or update a hosted issue, comment, or label
without the explicit authorization described under Hosted Writes.

## Workflow

1. Identify what is being reported and whether it may be a security
   vulnerability. If it might be, do not draft a public issue. Point to the
   repository's `SECURITY.md` or private reporting process and stop.
2. Inspect current repository reality before drafting: the relevant
   implementation, nearby tests, documentation, and configuration. Search by
   the underlying concept, not only the wording of the request.
3. Search the tracker's open and closed issues and its open and closed pull
   requests for the same problem or outcome. If the tracker cannot be reached,
   say that duplicate checking was not done.
4. Decide whether an issue is warranted. The behavior may already exist, be
   partly implemented, have been removed on purpose, or already be tracked or
   in flight. If so, report the evidence and the existing issue or pull request
   instead of drafting another, unless the user explicitly asks for a separate
   follow-up.
5. For a reported defect, reproduce it or trace a concrete failure path so the
   issue states verified behavior, not a guess. Say what could not be verified.
6. Use external research only when the issue depends on current third-party
   behavior or standards the repository does not establish. Keep repository
   evidence primary and cite sources.
7. Classify with exactly one `type:*` label from the canonical guidance. If a
   needed choice is unresolved or the work is not yet defined, use the idea or
   decision type; do not label it a feature to make it look executable.
8. Draft using the canonical structure. For implementation-ready work, write
   `Done when` as concrete, observable or testable conditions so a later agent
   can tell when it is complete without inventing product decisions. Describe
   behavior and contracts rather than file paths or line numbers that will go
   stale. Include `Suggested approach` only when repository evidence supports
   it, and add optional sections only when this issue benefits. Do not add a
   priority unless asked.
9. Present the draft with a short evidence summary: what was inspected, any
   duplicates or related work found, assumptions, and open questions that could
   change the issue.

## Hosted Writes

Creating or updating a hosted issue needs explicit authorization for that
action. Permission to edit repository files does not imply it. Create only what
the user approved, and report the resulting issue reference.

## Completion

State what was inspected and which searches were run, and report anything that
could not be checked. Never claim repository, tracker, or runtime state was
verified when it was not.
