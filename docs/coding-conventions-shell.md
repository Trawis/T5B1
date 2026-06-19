# Shell Script Conventions

Shell-specific conventions for standalone scripts.

**Version**: 1.22  
**Status**: Active  
**Last Updated**: 2026-06-19

Use this file together with `docs/coding-conventions-scripts.md`.

---

## Indentation and Formatting

- Preserve existing indentation style when editing existing shell files.
- For new shell scripts, follow the nearest repository convention; tabs are acceptable when no local style exists.
- Do not reformat unrelated code.
- Quote variables unless word splitting is explicitly intended.
- Prefer readable command blocks over dense one-liners.

---

## Shell Choice

- Prefer POSIX-compatible `sh` for simple portable scripts.
- Use Bash when arrays, `[[ ... ]]`, `pipefail`, process substitution, or other Bash features make the script safer or clearer.
- Use an appropriate shebang.

Bash starter:

```bash
#!/usr/bin/env bash
set -euo pipefail

VERSION="1.0.0"

main() {
	return 0
}

main "$@"
```

---

## Safety

- Use `set -euo pipefail` for Bash scripts when compatible with the script logic.
- Validate required commands before using them.
- Validate input paths before destructive or bulk operations.
- Prefer `--dry-run` for scripts that copy, move, rename, delete, sync, or rewrite files.
- Do not embed secrets, tokens, credentials, or machine-specific absolute paths.
- Do not overwrite files silently unless the script documents the behavior and accepts an explicit option such as `--force`.

---

## Output and Errors

- Print clear errors to stderr.
- Exit with non-zero status on failure.
- Keep CLI output style consistent with nearby output.
- Avoid colors, emoji, and Unicode decorations unless the script already uses them or the task asks for them.

---

## Validation

When feasible:

- run the script with `--help`
- run dry-run mode
- test missing inputs and invalid paths
- run ShellCheck when it is available
- test representative safe inputs
