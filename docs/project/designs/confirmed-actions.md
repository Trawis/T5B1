# Technical Design: Shared Confirmed-Action Sequence

- **Change**: One implementation of confirm, re-check, apply and report
  for every confirmed action (#558)
- **Status**: Implemented
- **Owner**: Trawis
- **Last Updated**: 2026-11-21

## Context

Every high-impact action asks through `Confirmation.Ask`. The current rule is
in [architecture: UI and safety](../architecture.md#ui-and-safety): on Yes,
the game and the exact targets are checked again, and if anything changed,
nothing is applied.

At `develop` `cbec686` there are 14 call sites in:

- `CompanyFeatures`
- `EmployeeFeatures`
- `WorkItemFeatures`
- `FounderToggle`
- `Experimental/`

Each call site writes the sequence by hand:

1. Check that the game is ready.
2. Resolve the targets.
3. Report when there is nothing to do.
4. Ask.
5. On Yes, re-check.
6. Apply.
7. Report the result.

What the code shows:

- `Confirmation.Ask` re-runs only `Helpers.RequireGameReady`. Everything else
  is in the callback.
- The same-save check, `ReferenceEquals(game, Settings)`, is repeated in each
  callback.
- Three re-check forms are in use:
  - `IsSameTargets`: Clear all loans, AI Bankrupt All, Make Selected Happy.
  - `IsScopeUnchanged` (count only): Unlock and Claim Rewards.
  - `EmployeeTarget.IsStillValid`: Founder, Fire Quietly.

  Others use their own predicate: Bankrupt and Adjust AI Funds use
  `IsAdjustableAICompany`, Skip Time and Hire check their own state, and
  Max Reputation has no re-check.
- The result message drifted. `ApplyAIBankrupt` shows none. Clear all loans
  and Make Happy do.
- `ConfirmationTests` reads named method bodies. A new action means copying
  the sequence and editing the test's lists.

## Goals and Non-Goals

### Goals

- A confirmed action states only what is specific to it:
  - how to resolve its targets;
  - the question;
  - the change;
  - the result message.
- Implement these once, with the same behaviour for every action:
  - the same-save check;
  - the target re-check;
  - the "situation changed" message;
  - the result notification.
- Test the sequence by calling it, not by reading action bodies.

### Non-Goals

- Changing which actions ask, their wording or the dialog type.
- Changing the native `ShowMessageBox` call: no `questionID`, no pause.
- Touching unconfirmed actions or editor windows. `EmployeeTarget` use in the
  editors stays.
- The multiplayer rule: #560 enforces it at the button, before any question.

## Proposed Design

Recommended: **option (a), one sequence helper with an injectable dialog**.

| Component | Change |
|---|---|
| `Confirmation` | Add `AskAndApply(actionName, question, findChange, apply)`. It replaces `Ask` |
| `Confirmation` | Add pure `Run(isReady, findChange, apply, showChange, showResult)`, the testable sequence |
| `Confirmation` | Keep `IsSameTargets`. Retire `IsScopeUnchanged` (see below) |
| Feature classes | Each action resolves targets and builds the question, then calls `AskAndApply` |
| `ConfirmationTests` | Behavioural tests of `Run`, plus a source rule that only `Confirmation.cs` calls `ShowMessageBox` and that the pinned actions call `AskAndApply` |

Signatures, all non-generic, delegates only:

```csharp
// findChange re-resolves and returns why nothing may be applied, or null;
// apply returns the result message, or null when it showed its own.
internal static void AskAndApply(string actionName, string question,
	Func<string> findChange, Func<string> apply)

internal static void Run(Func<bool> isReady, Func<string> findChange,
	Func<string> apply, Action<string> showChange, Action<string> showResult)
```

`findChange` returns a message rather than a bool, so single-target actions
keep their specific reason (for example "already bankrupt").
`ChangedUnless(bool)` turns a comparison into the generic "situation changed"
text.

On Yes, `AskAndApply` passes `Run`:

- `isReady` = `RequireGameReady(actionName)`;
- the save check plus the caller's `findChange`;
- `Notification.ShowError`;
- `Notification.Popup`.

The save check is built in: `AskAndApply` captures `GameSettings.Instance` when
it asks and compares it on Yes. Callers no longer write
`ReferenceEquals(game, Settings)`.

`Run` order: if not ready, stop (the readiness message is shown). If
`findChange` returns a message, show it and stop. Otherwise apply and
show its result when it is not null.

**Target rule.** An action passes a predicate that resolves its targets
again and compares them exactly:

- Lists use `IsSameTargets`.
- A single target uses its own validity check (`EmployeeTarget.IsStillValid`
  or `IsAdjustableAICompany`).
- Unlock and Claim Rewards switches from a count to the list of unclaimed
  task names, compared with `IsSameTargets`. `IsScopeUnchanged` is then
  removed.
- Max Reputation has no targets; the built-in save check is its re-check.

Example, AI Bankrupt All:

```csharp
var shown = FindAllBankruptTargets();
Confirmation.AskAndApply("AI Bankrupt All", question,
	() => Confirmation.ChangedUnless(
		Confirmation.IsSameTargets(shown, FindAllBankruptTargets())),
	() => ApplyAIBankrupt(shown));
```

`ApplyAIBankrupt` then returns the new "N AI companies bankrupted" text.

## Options and Tradeoffs

- **(a) One sequence helper (recommended).**
  - It removes every hand-written re-check and result branch.
  - The sequence becomes unit-testable through `Run`.
  - A new action cannot skip the save check.
  - Cost: each action's re-check becomes a closure over the shown targets.
    The Workshop compiler already builds such closures (`Confirmation.Ask`
    callbacks today).
- **(b) Share pieces only.** Keep each `Apply*` method, and share the save
  check, one comparison rule and the result call.
  - The diff per action is smaller.
  - But the order of steps stays hand-written, and so do the tests that
    read action bodies.
  - The drift #558 describes can recur.

**Count-only re-check.** Recommend removing it. A count can match after a task
is claimed and another unlocked. Comparing task names costs one list.

## Data and Interfaces

- No save data, setting keys or localization keys change. The exception is
  one new result key for AI Bankrupt All in the English `.tyd`, under the
  messages section.
- `Confirmation.Ask` stops being called from feature code. The rule for
  `Notification` as the only popup caller is unchanged.

## Security, Reliability, and Observability

- The dialog stays open while the game runs, so `Run` decides only on Yes,
  with fresh targets.
- An exception in `apply` behaves as today:
  - In the DLL build it surfaces in the dialog callback.
  - In the Workshop build the game's injected `try` handles it
    ([architecture: Scheduling and errors](../architecture.md#scheduling-and-errors)).

## Performance and Compatibility

Workshop source rules
([architecture](../architecture.md#workshop-source-build)):

- No generic classes or fields. `IsSameTargets<T>` is a generic method and
  already compiles.
- No calls in field initializers.
- No local named `ex`.
- `GameCompilerHazardTests`, `GameCompilerCompatibilityTests` and
  `GameSourcePipelineTests` must pass.
- Delegates are `System.Func`/`Action`, as `Confirmation.Ask` uses today.

There is no runtime cost beyond one closure per click.

## Validation

- **Unit:** `Run` truth table:
  - not ready: nothing runs;
  - changed: the changed message, no apply;
  - unchanged: apply, then the result;
  - a null result shows nothing.

  `IsSameTargets` tests stay.
- **Source rule:**
  - `ShowMessageBox` appears only in `Confirmation.cs`.
  - Every action in the architecture confirmation list calls `AskAndApply`.
  - The list is pinned in one place.
- **Smoke/manual (game):**
  - Each of the 14 actions: answer No, then answer Yes. On Yes, change the
    target while the dialog is open.
  - Load the Workshop build to confirm that the game's compiler accepts the
    closures.

## Rollout and Rollback

One PR, migrating all 14 actions in the same change so the source rule can be
strict. Rollback is a revert; no data is affected. The changelog notes the new
AI Bankrupt All result message.

## Architecture Documentation Updates

- UI and safety, **Confirmations**: describe `AskAndApply`, the built-in save
  check and the exact-target rule. Keep the action list.
- Set this design's status to Implemented.

## Decisions

- Actions with partial results keep choosing between popup, warning and
  error themselves, and `apply` returns null:
  - Hire Employees;
  - Optimize Team Assignments;
  - Amazing Compatibility when nothing is left;
  - the skips.
