# Script Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/scripts.md`
**Local editing**: Do not customize this synced copy

Apply repository-specific rules first. Use this file with the applicable language convention.

## Design

- Preserve the existing indentation and output style.
- Use stable descriptive filenames for version-controlled scripts.
- Prefer explicit arguments over machine-specific paths.
- Provide concise `--help` text for non-trivial command-line interfaces.
- Keep logic in small functions and side effects in the executable path.
- Print useful errors and return a non-zero exit status on failure.

## Safety

- Validate inputs before expensive or destructive work.
- Support dry-run mode for bulk copy, move, rename, delete, upload, or rewrite operations.
- Do not embed secrets or make hidden network calls.
- Do not overwrite user files silently unless overwrite behavior is explicit and documented.
- Do not commit, push, open pull requests, or merge unless the task explicitly requires it.

## Files and Output

- Use predictable output names and explicit output locations.
- Preserve the surrounding ASCII or Unicode presentation style; do not add decoration gratuitously.
- Avoid creating versioned copies of scripts that Git already tracks.
- Version distributed artifacts only when the project release process requires it.

## Validation

Run `--help`, representative safe inputs, dry-run mode, and invalid-input cases when applicable. Verify exit codes and generated files.
