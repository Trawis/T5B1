# Python Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/python.md`
**Local editing**: Do not customize this synced copy

Apply repository rules first. This covers Python libraries, applications, and
scripts. For command-line scripts, also use `.agents/conventions/scripts.md`.
Framework conventions, when selected, add to this file.

## Compatibility and Tooling

- Target the project's supported Python versions, from `requires-python`, CI,
  or tool configuration. Do not use newer syntax or standard-library APIs than
  the minimum allows.
- Use the project's configured formatter, linter, type checker, test runner,
  and dependency manager. Do not add or switch tools unless asked.
- Declare dependencies where the project already does, such as
  `pyproject.toml` or a requirements or lock file, and update its lock file
  with the project's tool. Prefer the standard library when it does the job
  well; that is a preference, not a ban on dependencies.

## Style and Naming

- Follow the existing formatting. Without a local rule, follow PEP 8 with four
  spaces per indent.
- Use `snake_case` for functions, variables, and modules, `CapWords` for
  classes, and `UPPER_CASE` for module constants. A leading underscore marks a
  name as internal.
- Keep imports at the top of the module, grouped as standard library,
  third-party, then local. Avoid wildcard imports.
- Use `is` and `is not` for `None` comparisons.

## Structure and APIs

- Keep side effects out of import time: no I/O, network calls, argument
  parsing, or global configuration changes at module level.
- Respect existing package boundaries and public names. Treat names exported
  from a package's public modules or `__all__` as a public contract.
- Avoid mutable default arguments; use `None` and create the value inside.
- Add type hints for public functions and new code when the project uses them,
  and keep them valid for the minimum supported version.
- Document public modules, classes, and functions with docstrings following
  PEP 257 and the project's docstring style. State behavior, parameters,
  return values, raised exceptions, and side effects that callers rely on.
  Keep comments for constraints and non-obvious intent.

## Errors and Resources

- Raise specific exceptions with useful messages. Catch only exceptions you can
  handle, and avoid bare `except:` and broad `except Exception` that hide
  failures.
- Preserve the cause when translating an exception with `raise ... from err`.
  Libraries should raise exceptions rather than print, log and continue, or
  call `sys.exit()`.
- Manage files, locks, connections, and temporary resources with `with` or
  `contextlib`. Do not rely on garbage collection to release them.
- Pass `encoding=` explicitly when opening text files.
- Do not use `assert` for input validation or security checks; assertions can
  be disabled with `-O`.
- Never build shell commands, SQL, or file paths from untrusted strings. Pass
  argument lists to `subprocess` without `shell=True`, use parameterized
  queries, and do not `pickle.load` or `eval` untrusted data.

## Scripts

For executable scripts:

- Use `argparse` for non-trivial command lines and `pathlib` for paths.
- Keep work in functions and guard the entry point with
  `if __name__ == "__main__":`.
- Return an exit code from `main()` and call it with
  `raise SystemExit(main())`.
- Validate paths and inputs before writing, and do not silently overwrite files
  unless the documented interface allows it.

## Validation

Run the project's configured tests, linters, and type checkers on the affected
code. Cover pure logic, error paths, and boundaries such as the filesystem,
network, and time. For scripts, include the failure cases in `scripts.md`.

## References

- [PEP 8: Style Guide for Python Code](https://peps.python.org/pep-0008/)
- [PEP 257: Docstring Conventions](https://peps.python.org/pep-0257/)
- [Python typing documentation](https://typing.python.org/en/latest/)
- [contextlib](https://docs.python.org/3/library/contextlib.html)
- [Writing your pyproject.toml](https://packaging.python.org/en/latest/guides/writing-pyproject-toml/)
