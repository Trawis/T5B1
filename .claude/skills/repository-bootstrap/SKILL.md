---
name: repository-bootstrap
description: Applies repo-seed to a new or existing repository using its sync script, choosing only the applicable profile, conventions, and scaffolds and verifying the result. Use when asked to apply, set up, initialize, install, or bring a repository under repo-seed management. Not for populating project documentation; use documentation-bootstrap for that.
---

# Repository Bootstrap

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
This skill is specific to repo-seed. It is not a general project generator, and
it does not write project documentation; `documentation-bootstrap` does that
afterwards, from repository evidence.

Use the pack's own `sync-docs.py` as the only mechanism. Do not copy managed
files, decide ownership, handle stale paths, or migrate state by hand.

## Workflow

1. Locate the pack: an extracted release archive or a repo-seed checkout
   containing `manifest.json` and `files/`, supplied by the user. Do not
   download one without permission. Read its `manifest.json` for the profiles,
   conventions, and scaffolds it supports. Only those values are valid.
2. Inspect the target before choosing any argument: the project type, the
   languages and tooling actually present, existing documentation, existing
   agent guidance (`AGENTS.md`, `CLAUDE.md`, `.agents/`, `.claude/`, and other
   assistants' instruction files), existing GitHub templates, and
   `.repo-seed-state.json` when present. A recorded state means this is an
   update, not a first install; keep its profile and conventions unless asked to
   change them.
3. Choose the profile from evidence: `minimal`, `library`, `app`, or `game`. If
   the evidence does not clearly decide it, stop and ask rather than assuming.
4. Choose conventions only for languages and tools the project actually uses and
   the manifest supports. Install none by default and never all of them.
5. Choose scaffolds as opt-in. Add each only when it is useful or requested:
   baseline project files for a repository that lacks a README or CHANGELOG,
   GitHub templates when requested, `.editorconfig` when appropriate, and
   optional documents only when the project needs them. Add the two scheduled
   workflows (`--scaffold-workflows`) only when the user asks for them: the
   drift-check issue and the sync pull request, which has write permissions.
6. Check what the sync would overwrite. `AGENTS.md` and `CLAUDE.md` are managed,
   so existing copies are replaced. If either holds project-specific content,
   tell the user and move that content to `.agents/project.md` first, with their
   agreement. Other project-owned files are not touched, and
   `.github/workflows/` changes only when `--scaffold-workflows` was requested,
   and then creates both callers.
7. Run `python <pack>/files/scripts/sync-docs.py --target <repo> --profile
   <profile> --conventions <list>` plus the chosen scaffold options with
   `--dry-run`. Read every line. Stop and resolve any unexpected overwrite,
   removal, or conflict before going further, and show the user the plan.
8. Run the same command without `--dry-run` only after the dry run is
   acceptable and the user wants the change applied.
9. Verify: `.repo-seed-state.json` records the intended profile and
   conventions; the expected managed guidance and skills are present; no
   project-owned file was replaced or removed; and a follow-up run with
   `--dry-run` is clean or every remaining line is explained.
10. Do not invent project facts. Scaffolded documents are templates. Leave them
    as placeholders where the repository gives no evidence, and suggest
    `documentation-bootstrap` for filling them in.

## Completion

Report the pack and version used, the profile, conventions, and scaffolds chosen
and why, any ambiguity you resolved by asking, the commands run, the
verification results, and what remains for the project to own. Do not create
branches, commit, push, or change hosted state; leave the new files for the
user to review and commit.
