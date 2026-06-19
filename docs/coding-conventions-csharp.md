# C# / .NET Coding Conventions

Detailed C#/.NET conventions for repositories that use this agent guideline pack.

**Version**: 1.24  
**Status**: Active  
**Last Updated**: 2026-06-19

Use repository-specific conventions first. If the target repository or child `AGENTS.md` defines different C# rules, follow the nearest applicable project rule.

---

## Quick Reference

| Area | Rule |
|------|------|
| Usings | `System` first, remaining directives alphabetized, no blank lines |
| Namespace | File-scoped namespace preferred |
| Indentation | Preserve existing file/repo style; default to tabs for new `*.cs` files only when no local style exists |
| Constants | `PascalCase`, placed before fields |
| Private fields | `_camelCase` |
| Properties | `PascalCase` |
| Interfaces | `I` prefix |
| Enums | Project-specific `Enum` suffix, real values start at `1` |
| Async methods | `Async` suffix for `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>` |
| Control flow | Single-statement guards may omit braces, but the statement must be on the next line; multi-statement blocks use braces; add a blank line after completed control blocks before independent statements |
| User-facing text | Preserve existing ASCII/Unicode punctuation and decorative separator style |
| Member order | Constants → Fields → Properties → Events → Constructor → Expression-bodied → Public → Protected → Protected event raisers → Private |

---

## Using Directives

Rules:

- Place all `using` directives at the top of the file, before the namespace.
- Use Visual Studio **Remove and Sort Usings** (`Ctrl+R, Ctrl+G`) as the default cleanup behavior.
- Place `System` namespace directives first.
- Alphabetize all remaining using directives.
- Do not manually separate framework, NuGet, and local project namespaces unless a specific project requires it.
- Do not add blank lines between using directives.
- Remove unused using directives.

Correct pattern:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCompany.Sales.Models;
using MyCompany.Sales.Services;

namespace MyCompany.Sales.OrderManagement;
```

---

## Indentation

Preserve the existing indentation style of the file or repository.

Rules:

- If the existing C# file uses spaces, continue using spaces.
- If the existing C# file uses tabs, continue using tabs.
- Do not convert a file from spaces to tabs or tabs to spaces unless the task explicitly asks for formatting/style cleanup.
- For new C# files in repositories without an established style, prefer tabs with width 4.
- Follow the nearest `.editorconfig` when one exists.
- Do not force C# indentation preferences onto Markdown, JSON, YAML, XML, project files, or generated files.

---

## Naming

- Public members, types, namespaces, constants, and properties: `PascalCase`.
- Private fields: `_camelCase`.
- Parameters and local variables: `camelCase`.
- Interfaces: `I` prefix, e.g. `IOrderService`.
- Type parameters: `T` prefix, e.g. `TEntity`, `TInput`.
- Async methods returning `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>` must use the `Async` suffix.
- Boolean properties should start with `Is`, `Has`, `Can`, or `Should` where natural.
- Avoid Hungarian notation.
- Avoid abbreviations unless they are widely understood.
- Avoid single-letter names except loop counters and very small lambda scopes.

---

## Enums

Project-specific rule: enum type names must use the `Enum` suffix.

Enum values must start from `1`. Value `0` is reserved only for special fallback values such as `Unknown`, `Unspecified`, or `None` for flags.

Correct pattern:

```csharp
public enum OrderStatusEnum
{
	Unknown = 0,
	Pending = 1,
	Processing = 2,
	Shipped = 3,
	Delivered = 4,
	Cancelled = 5
}
```

Flags enum exception:

```csharp
[Flags]
public enum FilePermissionsEnum
{
	None = 0,
	Read = 1,
	Write = 2,
	Execute = 4,
	Delete = 8
}
```

Shared guideline examples should use generic enum names like `OrderStatusEnum` or `FilePermissionsEnum`. Do not introduce project-specific enum names into reusable templates unless the document is for that specific project.

---

## Member Organization

Use this order inside classes:

```text
Constants
Fields
Properties
Events
Constructor
Expression-bodied members
Public methods
Protected methods
Protected event raisers
Private methods
```

Rules:

- Constants go before fields.
- Constructors come after constants, fields, properties, and events.
- Expression-bodied members go after constructors and before normal methods.
- Public API should be easy to scan.
- Private implementation details belong near the bottom.
- Keep related members together when readability clearly improves.

Example:

```csharp
public class Order
{
	public const int MaxRetries = 3;

	private readonly IOrderRepository _orderRepository;
	private bool _isProcessed;

	public int OrderId { get; set; }
	public string CustomerName { get; set; }
	public bool IsActive { get; set; }

	public event EventHandler<OrderEventArgs>? OrderStatusChanged;

	public Order(IOrderRepository orderRepository, int orderId, string customerName)
	{
		_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		OrderId = orderId;
		CustomerName = customerName;
		_isProcessed = false;
	}

	public bool IsReady => IsActive && OrderId > 0;

	public void Process()
	{
		if (!IsReady)
		{
			return;
		}

		_isProcessed = true;
	}

	protected virtual void OnOrderStatusChanged(OrderEventArgs e)
	{
		OrderStatusChanged?.Invoke(this, e);
	}

	private bool Validate()
	{
		return OrderId > 0;
	}
}
```

---

## Properties, Fields, and Constants

Properties:

- Use PascalCase.
- Group properties by type when it improves readability.
- Do not add blank lines between consecutive properties of the same type unless attributes are present.
- Separate attributed properties with blank lines.

Fields:

- Private fields use `_camelCase`.
- Prefer `readonly` when the value should not change after construction.
- Avoid public fields; use properties instead.
- Group fields by type and access level.
- Do not add blank lines between consecutive fields of the same type unless attributes are present.

Constants:

- Use PascalCase.
- Use `const` for compile-time constants.
- Use `static readonly` for runtime constants.
- Avoid public constants unless they are intentionally part of the public API.

---

## Control Flow and Spacing

Single-statement guard clauses may omit braces when the controlled statement is on the next line.

Use braces for multi-statement blocks and when braces improve readability. Preserve stricter project style if the local repository always uses braces.

Do not place control-flow actions inline after a condition. Put `return`, `throw`, `break`, `continue`, and similar statements on their own line.

Correct:

```csharp
if (order == null)
	throw new ArgumentNullException(nameof(order));

if (order.Status != OrderStatusEnum.Pending)
	return true;

if (!isValid)
{
	_logger.LogWarning(
		"Order {OrderId} cannot be processed in status {OrderStatus}.",
		order.Id,
		OrderStatusEnum.Cancelled);
	return false;
}
```

Incorrect:

```csharp
if (order == null) throw new ArgumentNullException(nameof(order));

if (order.Status != OrderStatusEnum.Pending) return true;
```

Spacing rules are preferred readability guidelines, not hard blockers.

Preferred style:

- Single variable followed by a simple guard `if`: no blank line.
- Multiple variables followed by an `if`: add a blank line before the `if`.
- `if/else` or `if/else if/else` blocks: add a blank line before the block.
- After a completed control block, add a blank line before the next independent statement.
- Do not add a blank line between `}` and a directly connected `else`, `catch`, `finally`, or `while` in a `do/while` block.
- When in doubt, optimize for readability and consistency with nearby code.

Correct spacing after a completed control block:

```csharp
if (!allowed)
{
	_logger.LogWarning(
		"Order {OrderId} is blocked in status {OrderStatus}.",
		order.Id,
		OrderStatusEnum.Cancelled);
}

return allowed;
```

Incorrect spacing after a completed control block:

```csharp
if (!allowed)
{
	_logger.LogWarning(
		"Order {OrderId} is blocked in status {OrderStatus}.",
		order.Id,
		OrderStatusEnum.Cancelled);
}
return allowed;
```

---

## Modern C# Defaults

Use modern C# features when they improve clarity. Do not rewrite working code only to use newer syntax.

Defaults:

- Prefer file-scoped namespaces.
- Use `var` when the type is obvious from the right-hand side.
- Use explicit types when they improve readability.
- Prefer records for immutable DTOs and value-like models.
- Prefer collection expressions where they are readable and supported by the target language version.
- Prefer pattern matching when it reduces branching or improves clarity.
- Use primary constructors only for simple classes where they improve readability.
- Avoid primary constructors in complex services with validation, many dependencies, or non-trivial initialization.
- Use `required` properties only when they improve object initialization safety.
- Avoid `#region` in normal application code; if a class needs regions, it is probably too large.
- Regions are acceptable in generated, interop, designer, or legacy code when they reduce noise.
- Do not introduce nullable reference type migrations unless the task explicitly asks for it.

---

## User-Facing Text and Output Style

Preserve the existing style of user-facing strings, terminal output, logs intended for humans, menu labels, headings, prompts, and UI text.

Rules:

- Do not mix plain ASCII separators with Unicode/box-drawing separators in the same output area.
- If nearby output uses plain ASCII dashes, keep using plain ASCII dashes.
- If nearby output uses Unicode box-drawing characters or em/en dashes, keep using that style consistently.
- Do not replace simple text with decorative Unicode, emoji, or box-drawing characters unless the task asks for that style.
- Do not downgrade existing intentional Unicode formatting to ASCII unless the project prefers ASCII-only output.
- When adding a new terminal/UI section, copy the style of the closest existing section.

Correct when the surrounding output uses plain ASCII:

```csharp
_output.WriteLine("-- Training & Capabilities --");
_output.WriteLine("-- Inventory --");
```

Correct when the surrounding output uses Unicode separators:

```csharp
_output.WriteLine("── Training & Capabilities ──────────────────────────");
_output.WriteLine("── Inventory ────────────────────────────────────────");
```

Incorrect mixed style:

```csharp
_output.WriteLine("── Training & Capabilities ──────────────────────────");
_output.WriteLine("-- Inventory --");
```

---

## Comments and Documentation Summaries

Use comments to explain useful context, not obvious code.

Rules:

- Prefer readable names and simple code before adding comments.
- Comment why code exists, non-obvious behavior, edge cases, external constraints, tradeoffs, or safety requirements.
- Do not comment obvious assignments, straightforward `if` checks, simple returns, or self-explanatory method calls.
- Do not add wide banner comments, decorative separators, or large comment blocks unless the local file already uses that style.
- Update or remove stale comments when editing nearby code.
- Use XML documentation for public APIs only when the project already uses it or when it clarifies behavior for external consumers. Do not add XML documentation to every member by default.
- Keep summaries concise and factual. Avoid filler, repeated points, and generic praise.

Good:

```csharp
// Keep this timeout below the gateway limit so retries happen client-side.
private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(25);
```

Bad:

```csharp
// Set allowed to false.
var allowed = false;

// If the order is null, throw an exception.
if (order == null)
	throw new ArgumentNullException(nameof(order));
```
---

## Error Handling and Logging

- Validate public method inputs where invalid values would create unclear failures.
- Throw specific exceptions where appropriate.
- Do not swallow exceptions silently.
- Preserve stack traces; use `throw;`, not `throw ex;`.
- Use structured logging placeholders instead of string interpolation in logger calls.

Preferred logging pattern:

```csharp
_logger.LogInformation("Order {OrderId} created successfully.", order.Id);
```

---

## Tests

- Add or update tests for behavior changes.
- Add unit tests when feasible for new logic, bug fixes, validation rules, mapping logic, parsing/formatting logic, and edge cases.
- Prefer focused unit tests for business logic.
- Prefer integration tests for persistence, API, and cross-component behavior.
- Keep tests deterministic.
- Do not add brittle tests that depend on timing, test execution order, external services, or local machine state.
- Do not remove tests unless they are obsolete and the reason is clear.
- If a test is flaky, report it instead of hiding it.
- If tests are not added for a code change, explain why in the completion summary or PR notes.

---

## `.editorconfig` Baseline

Use `.editorconfig` as the source of truth for formatting rules in repositories that adopt it. For existing repositories, preserve the local indentation style before adopting or changing `.editorconfig`; if the repo already uses spaces for C#, change the example before formatting.

```editorconfig
root = true

[*.cs]
# Default for new C# files when no local style already exists.
# Existing files should preserve their current indentation style.
indent_style = tab
indent_size = 4
tab_width = 4
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false
csharp_style_namespace_declarations = file_scoped:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion
csharp_style_var_for_built_in_types = false:suggestion

[*.{md,json,yml,yaml,xml,csproj,sln}]
indent_style = space
indent_size = 2
```

---

## StyleCop Compatibility

If StyleCop is used, configure it to match this document instead of accepting conflicting defaults.

Important alignment points:

- Using directives should remain outside the namespace.
- File-scoped namespaces should be allowed.
- `System` directives should be sorted first.
- Blank lines between using directive groups should not be required.
