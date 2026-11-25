# Shell Script Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/shell.md`
**Local editing**: Do not customize this synced copy

Apply repository rules first. Use this with `.agents/conventions/scripts.md`.
It covers POSIX `sh` and Bash. Keep a script's existing dialect, shebang,
indentation, and minimum supported Bash version.

## Dialect

- The shebang declares the dialect: `#!/bin/sh` promises POSIX `sh`, so do not
  use Bash features there. Many systems run `sh` as dash or BusyBox.
- Prefer POSIX `sh` for small portable scripts. Use Bash when arrays, `[[ ]]`,
  `local`, or `pipefail` make the script safer, and say so with
  `#!/usr/bin/env bash` or the project's existing shebang.
- Bash-only constructs include arrays, `[[ ]]`, `(( ))`, `local`, `$'...'`,
  `<<<`, process substitution, `${var//x/y}`, `mapfile`, and `PIPESTATUS`.
- Check features against the oldest Bash the project supports; macOS ships
  Bash 3.2.
- Move logic to a structured language when a script grows complex control flow
  or data handling.

## Naming and Structure

- Use lowercase `snake_case` for functions and local variables, and uppercase
  for exported variables and constants.
- In Bash, declare function variables with `local`. POSIX `sh` has no `local`,
  so use distinct names or a subshell.
- In scripts with several functions, define them first and call
  `main "$@"` last.
- Give a non-obvious function a short comment stating its arguments, outputs,
  and return status.

## Quoting and Arguments

- Double-quote expansions: `"$var"`, `"$(cmd)"`, `"${arr[@]}"`. Leave one
  unquoted only when word splitting or globbing is intended.
- Pass arguments on with `"$@"`, never `$*` or unquoted `$@`.
- Build commands in a Bash array, not in a string. Avoid `eval` and
  `sh -c "$string"` with untrusted input. Use `--` before user-supplied paths
  when the command supports it.
- Use `$(...)`, not backticks.
- Read lines with `while IFS= read -r line`. Handle arbitrary filenames with
  `find -exec ... {} +`, or `find -print0` with `xargs -0` or
  `read -r -d ''` in Bash. Do not parse `ls` output.
- Use `printf` rather than `echo` for data that may start with `-` or contain
  backslashes.

## Exit Status and Strict Mode

`set -euo pipefail` helps, but it does not make error handling correct:

- `set -e` is ignored in `if`, `while`, and `until` conditions, in any command
  of an `&&` or `||` list except the last, and after `!`. That includes
  everything inside a function called from such a context.
- `local var=$(cmd)` and `export var=$(cmd)` return the status of `local` or
  `export`, hiding a failure. Declare first, then assign.
- Command substitutions do not inherit `set -e` unless Bash runs in POSIX mode
  or `shopt -s inherit_errexit` (Bash 4.4+) is set.
- `pipefail` is in POSIX.1-2024 but missing from many `sh` implementations,
  including dash. A pipeline whose reader exits early, such as `| head`, can
  fail with status 141.
- `set -u` treats unset variables as errors; use `${var:-}` where unset is
  valid. Before Bash 4.4, `"${arr[@]}"` on an empty array also fails.

Check critical commands explicitly, for example with
`cmd || { echo "error: cmd failed" >&2; exit 1; }` or `if ! cmd; then`. In
Bash, read `PIPESTATUS` immediately after the pipeline when a non-final stage
matters.

## Temporary Files and Cleanup

- Create temporary files and directories with `mktemp`, never with predictable
  names such as `/tmp/name.$$`.
- Remove them with a trap set right after creation, for example
  `trap 'rm -rf -- "$tmp"' EXIT`. Keep trap handlers short and safe to run
  more than once.
- Guard destructive paths: refuse empty or root values before `rm -rf`, for
  example with `"${dir:?}"`.

## Validation

Run the declared interpreter's syntax check, such as `sh -n` or `bash -n`, and
ShellCheck when it is configured or readily available. Run shfmt or Bats only
when the project uses them. Exercise the failure cases in `scripts.md`, and
test a portable script under a strict POSIX shell such as dash when one is
available.

## References

- [GNU Bash manual: The Set Builtin](https://www.gnu.org/software/bash/manual/html_node/The-Set-Builtin.html)
- [POSIX Shell Command Language](https://pubs.opengroup.org/onlinepubs/9799919799/utilities/V3_chap02.html)
- [ShellCheck wiki](https://www.shellcheck.net/wiki/)
- [Google Shell Style Guide](https://google.github.io/styleguide/shellguide.html)
  (Bash-only; a style reference, not a portability policy)
