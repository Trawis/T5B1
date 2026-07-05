# Shell Script Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/shell.md`
**Local editing**: Do not customize this synced copy

Apply repository-specific rules first and use this with `.agents/conventions/scripts.md`.

## Style and Safety

- Preserve local indentation and shell choice.
- Prefer POSIX `sh` for simple portable scripts; use Bash when its features improve safety.
- For compatible Bash scripts, use `set -euo pipefail`.
- Quote variables unless splitting is intentional.
- Validate commands, arguments, and paths before bulk or destructive operations.
- Print errors to stderr and return non-zero on failure.
- Keep help text and comments concise.

## Validation

Run `--help`, dry-run mode, invalid-input cases, and representative safe inputs when applicable. Run ShellCheck when the repository uses it or it is readily available.
