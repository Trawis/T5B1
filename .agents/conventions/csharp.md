# C# / .NET Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/csharp.md`
**Local editing**: Do not customize this synced copy

Repository rules, `.editorconfig`, and nearby code take precedence. Do not restyle unrelated files.

## Core Style

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

Prefer stable, readable code over adopting newer syntax for its own sake.

## Naming and Organization

- Use meaningful names; avoid Hungarian notation and unclear abbreviations.
- Prefix boolean properties with `Is`, `Has`, `Can`, or `Should` when natural.
- Use the project convention of an `Enum` suffix for enum types.
- Reserve enum value `0` for `Unknown`, `Unspecified`, or `None`; real values start at `1`.
- Order members: constants, fields, properties, events, constructor, expression-bodied members, public methods, protected methods, event raisers, private methods.
- Prefer `readonly` fields and properties over public mutable fields.

## Control Flow

Single-statement guards may omit braces, but the action must be on the next line. Multi-statement blocks use braces. Follow stricter local rules when present.

```csharp
if (order == null)
	throw new ArgumentNullException(nameof(order));

if (!order.IsReady)
	return;
```

Never put `return`, `throw`, `break`, or `continue` on the same line as the condition. Add a blank line after a completed control block before the next independent statement; do not separate `else`, `catch`, or `finally` from its preceding brace.

## Modern C#

- Use `var` when the type is obvious; use explicit types when they aid reading.
- Prefer records for immutable value-like models.
- Use pattern matching, collection expressions, required members, and primary constructors only when supported and clearer.
- Avoid `#region` in normal application code.
- Do not introduce nullable-reference or language-version migrations incidentally.

## Errors, Logging, and Text

- Validate public inputs where invalid values would otherwise fail unclearly.
- Throw specific exceptions and preserve stack traces with `throw;`.
- Do not swallow exceptions.
- Use structured logging placeholders rather than interpolation.
- Preserve existing punctuation, Unicode, terminal separators, and UI wording style.

## Comments and Tests

- Comment intent, constraints, tradeoffs, and non-obvious behavior—not obvious code.
- Add focused tests for changed behavior, validation, mappings, parsing, and edge cases.
- Keep tests deterministic and report flaky or unavailable tests rather than hiding them.
