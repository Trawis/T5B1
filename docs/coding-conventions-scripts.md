# Script Coding Conventions

Shared conventions for standalone automation scripts.

**Version**: 1.22  
**Status**: Active  
**Last Updated**: 2026-06-19

Use repository-specific script conventions first. If the target repository or child `AGENTS.md` defines different rules, follow the nearest applicable project rule.

---

## Scope

These rules apply to standalone utility scripts such as:

- shell scripts
- Python scripts
- one-off automation scripts that are versioned as files
- scripts that generate local output files

Language-specific details live in:

- `docs/coding-conventions-python.md`
- `docs/coding-conventions-shell.md`

If a script is part of a package/application with its own packaging/versioning system, follow the repository standard and keep script-specific versions consistent with it.

---

## Indentation Preservation

Preserve the existing indentation style of the script being edited.

Rules:

- If the existing script uses spaces, continue using spaces.
- If the existing script uses tabs, continue using tabs.
- Do not convert indentation style unless the task explicitly asks for formatting/style cleanup.
- For new scripts, follow the language-specific convention file and nearest `.editorconfig`.

---

## Filename and Versioning Rules

Prefer stable script filenames for scripts committed to a repository. Git tracks the script history, so avoid creating a new filename for every script revision.

Rules for repository-maintained scripts:

- Use a stable descriptive filename such as `sync-agent-guidelines.py`, `backup-photos.py`, or `cleanup-downloads.sh`.
- Do not rename the script only because its implementation changed.
- Keep the script extension unchanged: `.sh`, `.bash`, `.py`, etc.
- If the script has an internal version constant, update that constant when behavior changes materially.
- Use semantic versioning in `MAJOR.MINOR.PATCH` format for internal script version constants when the script is reused across repositories.
- Do not use vague suffixes such as `final`, `final2`, `new`, `latest`, `copy`, `fixed`, `v2`, or date-only versions.

Rules for distributed standalone copies, archive artifacts, and script-generated output files:

- Put the version suffix immediately before the file extension when the file is distributed outside normal Git history.
- Script-generated output files should use the same `<script-name>_<major>.<minor>.<patch>.<extension>` pattern unless the task or an existing project format requires a different output name.
- If the output format needs a descriptive suffix, place it after the version and keep it stable.

Repository script filename pattern:

```text
<script-name>.<extension>
```

Distributed/output filename pattern:

```text
<script-name>_<major>.<minor>.<patch>.<extension>
<script-name>_<major>.<minor>.<patch>_<stable-output-name>.<extension>
```

Examples:

```text
sync-agent-guidelines.py
backup-photos.py
cleanup-downloads.sh
agent-guidelines-pack_1.22.0.zip
backup-photos_1.1.0.csv
backup-photos_1.1.0_report.txt
cleanup-downloads_2.0.0_log.txt
```

Version meaning:

- `MAJOR`: breaking CLI, behavior, output format, config format, or compatibility change.
- `MINOR`: backwards-compatible feature or option.
- `PATCH`: bug fix, small internal improvement, comments, or non-breaking cleanup.

---

## Script Behavior

- Prefer explicit CLI arguments over hardcoded local paths.
- Provide `--help` or clear usage text when practical.
- Validate inputs before destructive or expensive operations.
- Print useful progress/error messages.
- Exit with non-zero status on failure.
- Avoid hidden network calls or destructive operations unless clearly documented and requested.
- Do not embed secrets, tokens, passwords, or machine-specific paths.
- Prefer dry-run support for scripts that move, delete, rename, upload, copy into repositories, or modify many files.
- Do not auto-commit, auto-push, auto-open PRs, or auto-merge unless the task explicitly requires it.

---

## CLI Output Text Style

Preserve the existing CLI/user-facing output style of the script.

Rules:

- Do not mix plain ASCII separators with Unicode/box-drawing separators in the same output area.
- If the script already uses ASCII-only output, keep new output ASCII-only unless asked otherwise.
- If the script intentionally uses Unicode symbols, headings, or separators, keep additions consistent with that style.
- Do not add emoji, decorative Unicode, colors, or box-drawing output unless the script already uses them or the task asks for them.
- Copy the style of nearby headings, prompts, status messages, and error messages.

Examples:

```text
-- Backup Summary --
-- Errors --
```

```text
── Backup Summary ───────────────────────────────────
── Errors ────────────────────────────────────────────
```

Do not mix both styles in the same script output.

---

## Output Files

When a script writes output files:

- Use stable, predictable filenames.
- Include the script version in generated filenames unless an existing project format requires otherwise.
- Do not overwrite existing output files silently unless the script clearly documents that behavior.
- Prefer writing to an explicit output directory or explicit output file path.
- For logs/reports, use stable suffixes like `_report`, `_log`, `_errors`, `_summary`.

Examples:

```text
backup-photos_1.1.0_report.txt
backup-photos_1.1.0_errors.csv
cleanup-downloads_2.0.0_log.txt
```

---

## Validation

When practical, run:

```bash
python script-name_1.0.0.py --help
./script-name_1.0.0.sh --help
```

For scripts with behavior changes:

- run with representative safe inputs
- run dry-run mode when available
- test missing/invalid input paths
- verify output filenames
- verify exit code behavior
- verify generated files are not overwritten unexpectedly
