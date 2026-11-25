# Script Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/scripts.md`
**Local editing**: Do not customize this synced copy

Apply repository rules first. Use this with the applicable language convention.
It covers language-independent script and command-line practice. An existing
script's documented arguments, output, and exit codes are a public contract:
change them only when the task requires it, and treat new behavior below as
the default for new interfaces, not a retrofit.

## Design

- Preserve the existing indentation and output style.
- Use stable, descriptive filenames for version-controlled scripts.
- Prefer explicit arguments over machine-specific paths.
- Use the language's argument parser rather than hand-parsing options.
- Keep logic in small functions and side effects in the executable path.

## Help and Arguments

- Provide `-h` and `--help` for non-trivial interfaces: purpose, usage, the
  common options, and an example. Help exits zero and runs nothing else.
- Prefer named options over several positional arguments. Give options a long
  form and follow common names such as `--dry-run`, `--force`, `--output`, and
  `--verbose`.
- Report unknown options and invalid values with a short usage hint.
- Do not accept secrets as option values; they leak into shell history and
  process lists. Read them from the environment, a file, or standard input.

## Output and Exit Status

- Write the requested result to standard output. Write progress, warnings,
  errors, and diagnostics to standard error, so output stays safe to pipe.
- Keep machine-readable output stable when scripts or CI consume it. Offer a
  structured format such as JSON only when consumers need one, and keep it free
  of log lines.
- Do not emit color, spinners, or progress bars when the stream is not a
  terminal. Honor `NO_COLOR` where the project already supports color.
- Make errors actionable: what failed, the relevant path or value, and how to
  fix it. Keep stack traces for unexpected errors or a verbose mode.
- Return zero only on success and non-zero on failure. When callers need to
  tell failures apart, document distinct codes and keep them stable.

## Interactivity

- Prompt only when standard input is a terminal. Without one, use explicit
  options or fail with a message naming the missing value; never hang waiting
  for input.
- Provide a noninteractive path for every prompt, such as an option or
  `--yes`, so the script can run in CI and automation.
- Use bounded timeouts for network and external-process calls.

## Safety

- Validate inputs before expensive or destructive work.
- Support dry-run mode for bulk copy, move, rename, delete, upload, or rewrite
  operations.
- Require explicit confirmation or a flag before destructive actions.
- Do not embed secrets or make hidden network calls.
- Do not silently overwrite user files unless overwrite behavior is explicit and
  documented.
- Prefer operations that can be rerun safely after an interruption, and avoid
  leaving partial output that looks complete.
- Do not commit, push, open pull requests, or merge unless the task explicitly
  requires it.

## Files

- Use predictable output names and explicit output locations.
- Preserve the surrounding ASCII or Unicode presentation style. Do not add
  decoration gratuitously.
- Avoid creating versioned copies of scripts that Git already tracks.
- Version distributed artifacts only when the project release process requires
  it.

## Validation

Run `--help`, representative safe inputs, and dry-run mode when applicable.
Exercise failure paths too: invalid and missing arguments, missing files, and
noninteractive runs without a terminal. Check the exit code, that standard
output holds only the result, and that diagnostics went to standard error.
Verify generated files.

## References

- [Command Line Interface Guidelines](https://clig.dev/)
- [POSIX Utility Conventions](https://pubs.opengroup.org/onlinepubs/9799919799/basedefs/V1_chap12.html)
- [NO_COLOR](https://no-color.org/)
