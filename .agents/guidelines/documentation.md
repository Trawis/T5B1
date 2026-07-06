# Documentation Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/documentation.md`

## Authority

- `README.md` and `CHANGELOG.md` are project-owned entry points.
- `docs/project/` contains authoritative live documentation.
- `docs/templates/` contains managed reference templates, not requirements.
- Never edit target templates or copy placeholders and unverified claims into live documents.

## Update the Relevant Document

- setup, commands, configuration, or public usage: `README.md`
- meaningful changes: `CHANGELOG.md`
- capability status and links: `docs/project/features.md`
- stable components, boundaries, integrations, or data flow: `docs/project/architecture.md`
- visible workflows and troubleshooting: `docs/project/user-guide.md`
- functional, technical, or gameplay requirements: the applicable FSD, TSD, or GDD

Update only documents affected by verified behavior. Do not create documentation as busywork.

## Template Review

Scaffolded Markdown contains a source-path marker. It does not track a hash or authorize automatic updates.

When a managed template changes, compare it manually with the live document, using Git history when useful. Apply only relevant structural improvements and preserve verified project content. Sync never rewrites live project documentation.

## Style

- Be concise, factual, and example-driven.
- Link to detailed documents instead of duplicating them.
- Curate changelogs; do not paste commit history.
- Omit unknown information rather than inventing it.
- Preserve the repository's established formatting and punctuation.
