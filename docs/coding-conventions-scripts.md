# Shell and Python Script Conventions

Detailed conventions for standalone shell and Python scripts.

**Version**: 1.16  
**Status**: Active  
**Last Updated**: 2026-06-15

Use repository-specific script conventions first. If the target repository or child `AGENTS.md` defines different rules, follow the nearest applicable project rule.

---

## Scope

These rules apply to standalone utility scripts such as:

- `.sh`
- `.bash`
- `.py`
- one-off automation scripts that are versioned as files
- scripts that generate local output files

If a script is part of a package/application with its own packaging/versioning system, follow the repository standard and keep script-specific versions consistent with it.

---

## Indentation Preservation

Preserve the existing indentation style of the script being edited.

Rules:

- If the existing script uses spaces, continue using spaces.
- If the existing script uses tabs, continue using tabs.
- Do not convert indentation style unless the task explicitly asks for formatting/style cleanup.
- For new Python scripts, use 4 spaces.
- For new shell scripts, follow the nearest repository convention; tabs are acceptable for indentation when no local style exists.

---

## Filename and Versioning Rules

For standalone shell or Python scripts, keep script filenames and script-generated output filenames stable and versioned.

Rules:

- Use the same script-name prefix for every version of the same script.
- Put the version suffix immediately before the file extension.
- Use semantic versioning in `MAJOR.MINOR.PATCH` format.
- Do not use vague suffixes such as `final`, `final2`, `new`, `latest`, `copy`, `fixed`, `v2`, or date-only versions.
- Keep the script extension unchanged: `.sh`, `.bash`, `.py`, etc.
- When the script has an internal version constant, keep it aligned with the filename version.
- Script-generated output files must use the same `<script-name>_<major>.<minor>.<patch>.<extension>` pattern unless the task or an existing project format requires a different output name.
- If the output format needs a descriptive suffix, place it after the version and keep it stable.

Filename pattern:

```text
<script-name>_<major>.<minor>.<patch>.<extension>
```

Output filename pattern:

```text
<script-name>_<major>.<minor>.<patch>.<extension>
<script-name>_<major>.<minor>.<patch>_<stable-output-name>.<extension>
```

Examples:

```text
backup-photos_1.0.0.py
backup-photos_1.0.1.py
backup-photos_1.1.0.py
cleanup-downloads_2.0.0.sh
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
- Prefer dry-run support for scripts that move, delete, rename, upload, or modify many files.

---

## Python Script Defaults

- Use 4 spaces for new Python scripts when no local style exists. Preserve existing indentation style when editing existing Python files.
- Use readable, standard-library-first Python unless a dependency is clearly justified.
- Prefer `argparse` for CLI arguments.
- Use `pathlib` for filesystem paths.
- Use functions instead of one long top-level script.
- Use `if __name__ == "__main__":` for executable scripts.
- Keep side effects out of import time.
- Prefer clear exceptions and user-facing error messages.
- Add tests for parsing, transformations, validation, and edge cases when feasible.

Minimal structure:

```python
from pathlib import Path

VERSION = "1.0.0"


def main() -> int:
    # implementation
    return 0


if __name__ == "__main__":
	raise SystemExit(main())
```

---

## Shell Script Defaults

- Preserve existing shell-script indentation style when editing existing files.
- Prefer POSIX-compatible `sh` unless Bash-specific features are useful and documented.
- Use a shebang appropriate to the script.
- Use `set -euo pipefail` for Bash scripts when compatible with the script logic.
- Quote variables unless word splitting is explicitly intended.
- Validate required commands and input paths.
- Avoid destructive commands without confirmation or dry-run support.
- Print clear errors to stderr.

Bash starter:

```bash
#!/usr/bin/env bash
set -euo pipefail

VERSION="1.0.0"

main() {
	# implementation
	return 0
}

main "$@"
```

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

- run a small smoke test
- use a temporary directory or sample input
- avoid modifying real user data during validation
- report exactly what was tested and what was skipped
