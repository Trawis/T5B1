# Unity / C# Conventions

**Document role**: Managed coding-agent reference
**Sync destination**: `.agents/conventions/unity.md`
**Local editing**: Do not customize this synced copy

Apply this with `.agents/conventions/csharp.md`. Repository rules and nearby code take precedence.

## Components and Lifecycle

- Keep one `MonoBehaviour` per matching filename.
- Use plain C# classes for logic that does not need Unity lifecycle or serialization.
- Prefer `[SerializeField] private` over public inspector fields.
- Order used callbacks: `Awake`, `OnEnable`, `Start`, `Update`, `LateUpdate`, `FixedUpdate`, `OnDisable`, `OnDestroy`.
- Cache component references in `Awake`; do not call `GetComponent`, `FindObjectOfType`, or `GameObject.Find` every frame.
- Subscribe in `OnEnable` and unsubscribe in `OnDisable`.
- Do not add empty lifecycle methods.

## Unity Objects and Coroutines

- Respect Unity's overloaded lifetime checks for `UnityEngine.Object`; do not use `is null` for Unity objects.
- In hot paths, use the implicit Unity object check when it is clear.
- Give coroutine methods a `Coroutine` suffix.
- Keep a `Coroutine` handle when early cancellation is required.
- Use explicit state machines for complex interruptible flows.

## Physics and Performance

- Perform physics work in `FixedUpdate`.
- Move rigid bodies through `Rigidbody` APIs rather than transforms.
- Avoid repeated allocations, LINQ, string construction, and component lookup in frame callbacks.
- Pool frequently created gameplay objects where profiling justifies it.
- Profile before making speculative optimizations.

## Project Structure

- Prefer C# events or `UnityEvent` over `SendMessage`.
- Use `ScriptableObject` for shared configuration, not mutable runtime state.
- Keep editor-only code under `Editor/` or guard it with `#if UNITY_EDITOR`.
- Do not reference `UnityEditor` from runtime assemblies.
- Centralize tags and layers; use `CompareTag` rather than direct tag-string equality.
- Follow existing asset naming and rename referenced assets through the Unity Editor.

## Testing

Use Edit Mode tests for pure logic and Play Mode tests for lifecycle, scenes, and runtime integration. Keep Unity-dependent behavior out of plain classes when practical so it remains easy to test.
