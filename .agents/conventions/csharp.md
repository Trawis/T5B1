# C# / .NET Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/csharp.md`
**Local editing**: Do not customize this synced copy

Repository rules, `.editorconfig`, analyzer settings, and nearby code take
precedence. Do not restyle unrelated files. Framework conventions, such as
Unity, add to this file.

## Core Style

These are Repo Seed defaults for new code when no local rule exists. Some
differ from Microsoft's published conventions, which use four spaces, always
use braces, and forbid an `Enum` suffix. A project that follows Microsoft's
style, or any other configured style, keeps it.

| Area | Default when no local rule exists |
|---|---|
| Usings | Outside the namespace; `System` first; alphabetized; no blank groups |
| Namespace | File-scoped |
| Indentation | Preserve local style; tabs of width 4 for new C# files |
| Public members and types | `PascalCase` |
| Private fields | `_camelCase` |
| Parameters and locals | `camelCase` |
| Interfaces | `I` prefix |
| Async methods | `Async` suffix |
| Constants | `PascalCase` |

Prefer stable, readable code over new syntax for its own sake.

## Naming and Organization

- Use meaningful names. Avoid Hungarian notation and unclear abbreviations.
- Prefix boolean properties with `Is`, `Has`, `Can`, or `Should` when natural.
- Repo Seed default: give enum types an `Enum` suffix. Follow the project's
  existing enum names, which take precedence.
- Give simple enums a zero value such as `None` or `Unknown`, because zero is
  every enum's default. Repo Seed default: start real values at `1`. Do not
  renumber existing enums that are persisted or serialized.
- Order members: constants, fields, properties, events, constructor,
  expression-bodied members, public methods, protected methods, event raisers,
  private methods.
- Prefer `readonly` fields and properties over public mutable fields.

## Control Flow

Repo Seed default: a single-statement guard may omit braces, but the action
goes on the next line. Multi-statement blocks use braces. Follow stricter local
rules.

```csharp
if (order == null)
	throw new ArgumentNullException(nameof(order));

if (!order.IsReady)
	return;
```

Never put `return`, `throw`, `break`, or `continue` on the same line as the condition. Add a blank line after a completed control block before the next independent statement. Do not separate `else`, `catch`, or `finally` from the preceding brace.

## Modern C#

- Use language features supported by the project's `LangVersion` and target
  frameworks.
- Use `var` when the type is obvious. Use explicit types when they aid reading.
- Prefer records for immutable value-like models.
- Use pattern matching, collection expressions, required members, and primary constructors only when supported and clearer.
- Avoid `#region` in normal application code.
- Do not introduce nullable-reference or language-version migrations incidentally.
- Where nullable reference types are enabled, keep annotations accurate. Do not
  silence warnings with `!` unless a comment explains why the value cannot be
  null.

## Errors, Logging, and Text

- Validate public inputs where invalid values would otherwise fail unclearly.
- Throw specific exceptions. Preserve stack traces with `throw;`.
- Do not swallow exceptions.
- Use structured logging placeholders, not interpolation.
- Preserve existing punctuation, Unicode, terminal separators, and UI wording style.

## Resources and Async

- Dispose `IDisposable` and `IAsyncDisposable` objects you own with `using` or
  `await using`. Do not dispose objects owned by a caller or a dependency
  injection container.
- Accept a `CancellationToken` on public async APIs that do I/O or long work,
  and pass it to the calls they make.
- Await tasks. Avoid `async void` except for event handlers, and do not block
  on async code with `.Result` or `.Wait()`.
- In library code without a synchronization context dependency, follow the
  project's `ConfigureAwait` policy.

## Documentation Comments

- Give public types and members in libraries and shared APIs XML documentation
  comments: `<summary>`, plus `<param>`, `<returns>`, and `<exception>` where
  they add information. Use `<inheritdoc/>` for implementations that do not
  change the contract.
- Follow the project's documentation settings, such as
  `GenerateDocumentationFile` and CS1591 warnings. Do not add comments that
  only restate member names.

## Comments and Tests

- Comment intent, constraints, tradeoffs, and non-obvious behavior, not obvious code.
- Add focused tests for changed behavior, validation, mappings, parsing, and edge cases.
- Keep tests deterministic. Report flaky or unavailable tests; do not hide them.
- Validate with `dotnet build` and `dotnet test`, or the project's build
  scripts. Fix new analyzer and compiler warnings, and run `dotnet format` only
  when the project uses it, scoped to changed files.

## References

- [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [C# identifier naming rules and conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [Framework design guidelines: naming](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces)
- [XML documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)
- [.NET code analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
