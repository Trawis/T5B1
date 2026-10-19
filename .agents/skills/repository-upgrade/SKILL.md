---
name: repository-upgrade
description: Upgrades a repository that already uses repo-seed to a newer pack using that pack's sync script, with a dry run first and verification afterwards. Use when asked to update, upgrade, migrate, or bring managed files to the latest pack. For first-time adoption use repository-bootstrap; for a read-only check use repository-audit.
---

# Repository Upgrade

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
This skill upgrades an existing repo-seed install. For a repository with no
repo-seed state, use `repository-bootstrap` instead.

Migration, ownership, and tombstone rules belong to the newer pack's manifest
and `sync-docs.py`. Do not restate or reimplement them here, and do not move,
copy, or delete managed files by hand. If an upgrade needs behavior the sync
script lacks, report that rather than improvising.

## Workflow

1. Locate the newer pack: an extracted release archive or repo-seed checkout
   containing `manifest.json` and `files/`, supplied by the user. Do not
   download one without permission. Note its `pack_version`.
2. Inspect the installed state before changing anything: `.repo-seed-state.json`
   (its schema version, pack version, profile, conventions, managed files, and
   tombstones), and the working tree for uncommitted changes. If there is no
   state file, stop and use `repository-bootstrap`.
3. Use the newer pack's own `scripts/sync-docs.py` (the one under the pack's
   `files/`), not the older copy in the target. Recorded profile and conventions
   are reused when you omit `--profile` and `--conventions`; omit them for a
   normal upgrade and do not change the recorded selection unless the user asks.
4. If the script stops and asks for an explicit selection, such as a
   `--conventions` list because a recorded convention is no longer in the pack,
   do not choose for the user. Show what the script says, explain what each
   option would install or remove, and ask. If it refuses an old state file,
   report its error to the user and ask; do not work around it and do not
   download a pack without permission.
5. Run the intended command with `--dry-run` first, and read every line: managed
   files it would replace, files it would remove, modified files it would
   preserve, tombstones, state changes, and any scaffold effects. Resolve
   anything unexpected before continuing, and show the user the plan. A copy
   line marked `and discard local changes` overwrites local edits: run the same
   command with `--diff` instead of `--dry-run`, show the user those diffs, and
   get their go-ahead before applying.
6. A `remove` line is a managed file the pack no longer selects and that is
   unchanged. A `preserve` line, sometimes also reported as a tombstone, is a
   file the script deliberately keeps: locally modified, dropped from the
   manifest, or now project-owned. Do not force a change to files the script
   preserves, never edit project-owned files by hand, and report each preserved
   file for review.
7. Apply by running the same command without `--dry-run` only after the dry run
   is acceptable and the user wants the upgrade.
8. Verify: the state file records the intended pack version, profile, and
   conventions; expected managed guidance and
   skills are present; removals happened only for unchanged files repo-seed
   owned; project-owned and locally modified files are intact; and a repeat
   `--dry-run` is clean or every remaining line is explained. Run the script
   with `--audit` as a final check and explain its findings.
9. Review the resulting diff with the user before anything is committed.

## Completion

Report the old and new pack versions, the command run, state and selection
changes, files replaced, removed, preserved, or tombstoned and why, anything that
needs human follow-up, and the verification results. Do not create branches,
commit, push, or change hosted state; leave the changes for the user to review.
