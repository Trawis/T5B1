# Python Script Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/python.md`
**Local editing**: Do not customize this synced copy

Apply repository-specific rules first and use this with `.agents/conventions/scripts.md`.

## Style and Structure

- Preserve existing formatting; use four spaces for new files when no local rule exists.
- Keep imports at the top and prefer the standard library before adding dependencies.
- Use small functions, `argparse` for non-trivial CLIs, and `pathlib` for filesystem paths.
- Keep side effects out of import time.
- Return an exit code from `main()` and call it with `raise SystemExit(main())`.
- Use explicit text encodings and readable type hints where useful.

## Reliability

- Validate paths and inputs before writing.
- Raise or report specific errors; avoid broad exception handling that hides failures.
- Avoid mutable default arguments.
- Do not silently overwrite files unless the documented interface explicitly permits it.
- Keep comments and docstrings for public contracts, constraints, and non-obvious behavior.

## Validation

When applicable, test pure transformations, invalid input, filesystem boundaries, dry-run behavior, generated output, and exit codes. Use the repository's existing test framework.
