# Python Script Conventions

Python-specific conventions for standalone scripts.

**Version**: 1.22  
**Status**: Active  
**Last Updated**: 2026-06-19

Use this file together with `docs/coding-conventions-scripts.md`.

---

## Indentation and Formatting

- Preserve existing indentation style when editing existing Python files.
- For new Python scripts, use 4 spaces.
- Do not reformat unrelated code.
- Keep imports at the top of the file.
- Prefer standard-library modules before adding dependencies.

---

## Structure

Prefer small functions over one long top-level script.

Recommended structure:

```python
from pathlib import Path

VERSION = "1.0.0"


def main() -> int:
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
```

Rules:

- Use `if __name__ == "__main__":` for executable scripts.
- Keep side effects out of import time.
- Return process exit codes from `main()`.
- Use `argparse` for non-trivial CLI arguments.
- Use `pathlib` for filesystem paths.
- Use explicit file encodings for text I/O when practical.
- Prefer clear exceptions and user-facing error messages.
- Avoid broad `except Exception` unless the error is logged/reported and re-raised or converted to a clear exit code.
- Avoid mutable default argument values.

---

## Typing

- Use type hints for non-trivial functions.
- Keep type hints readable; do not add complex typing only for style.
- Prefer `Path` over raw path strings once paths enter the script logic.

---

## Dependencies

- Prefer the Python standard library first.
- Do not add third-party dependencies unless they materially simplify the task or are already used by the repository.
- If dependencies are added, document installation and update the relevant requirements/pyproject file.

---

## Safety

- Validate input files/directories before modifying anything.
- Prefer `--dry-run` for scripts that copy, move, rename, delete, sync, or rewrite files.
- Do not embed secrets, tokens, credentials, or machine-specific absolute paths.
- Do not overwrite files silently unless the script documents the behavior and accepts an explicit option such as `--force`.

---

## Testing and Validation

When feasible:

- add unit tests for parsing, transformations, validation, filename generation, and conflict handling
- run the script with `--help`
- run dry-run mode
- test missing inputs and invalid paths
- test representative safe inputs

For repositories using `pytest`, prefer focused tests for pure functions and edge cases.
