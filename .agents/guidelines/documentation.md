# Documentation Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/documentation.md`

## Authority

- `README.md` and `CHANGELOG.md` are project-owned entry points.
- `docs/project/` contains authoritative live documentation.
- `docs/templates/` contains managed reference templates, not requirements.
- Never edit target templates or copy placeholders and unverified claims into live documents.
- An untouched scaffold is not a requirement. Treat `Draft`, inferred, and
  placeholder content as non-authoritative until the project confirms it.
- An accepted FSD or GDD controls intended behavior. Architecture describes the
  verified current technical system. An accepted technical design controls only
  its scoped change.

## Update the Relevant Document

- setup, commands, configuration, or public usage: `README.md`
- meaningful changes: `CHANGELOG.md`
- accepted application behavior and capabilities: `docs/project/fsd.md`
- accepted gameplay intent and capabilities: `docs/project/gdd.md`
- current components, boundaries, runtime, data, deployment, quality approaches,
  decisions, and technical risks: `docs/project/architecture.md`
- visible workflows and troubleshooting: `docs/project/user-guide.md`
- one substantial proposed technical change:
  `docs/project/designs/<short-name>.md`, created from
  `docs/templates/tsd.template.md`
- optional public capability index: `docs/project/features.md`

Update only documents affected by verified behavior. Do not create documentation as busywork.

## Growth Without Reorganization

Keep `fsd.md`, `gdd.md`, `architecture.md`, and `user-guide.md` as stable
entry points. When one becomes difficult to navigate, retain a concise overview
there and link detail under the applicable folder:

- functional detail: `docs/project/functional/`
- gameplay detail: `docs/project/game-design/`
- technical detail: `docs/project/architecture/`
- change designs: `docs/project/designs/`
- user guidance: `docs/project/guides/`

Do not create empty folders or index files preemptively.

## Technical Designs

Create a design only for a change involving multiple components, public
contracts, persistence or migration, security, concurrency, deployment,
compatibility, high uncertainty, or difficult rollback. Routine fixes and
localized refactors do not need one.

Use `Proposed`, `Accepted`, `Implemented`, `Superseded`, or `Rejected` status.
After implementation, update architecture with the resulting current state and
leave the design as historical rationale. Current truth must not remain only in
an old design.

## Existing-Project Bootstrap

When asked to populate documentation for an existing repository:

1. inspect source, tests, configuration, schemas, workflows, assets, and useful
   Git history;
2. populate only applicable documents from relevant evidence: FSD for
   applications or GDD for games, architecture for libraries/applications/games,
   user guidance for user-facing products, plus README and changelog;
3. distinguish intended behavior, verified as-built behavior, local inference,
   and unknowns where the difference matters;
4. cite repository evidence for meaningful inferences and request confirmation
   before treating inferred intent as authoritative;
5. do not invent historical technical designs or reconstruct changelog entries
   without evidence.

Game code can establish implemented mechanics but not reliably establish
intended player experience, balance goals, or future design.

## Template Review

Newly scaffolded Markdown contains source-path and content-hash markers. Sync
may upgrade it only while those markers prove the live document is unchanged.
Known older scaffolds may also be upgraded when their recorded provenance or
an approved legacy content hash verifies the original content.

When a live document cannot be verified, compare it manually with the managed
template, using Git history when useful. Apply only relevant structural
improvements and preserve verified project content.

## Style

- Be concise, factual, and example-driven.
- Link to detailed documents instead of duplicating them.
- Keep current state in living documents and proposal history in design records.
- Curate changelogs; do not paste commit history.
- Omit unknown information rather than inventing it.
- Preserve the repository's established formatting and punctuation.
