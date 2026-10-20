using System;

namespace Trainer_v5
{
	// The one place a trainer action asks the player "Yes or No?" (#242). It is the game's own Yes/No dialog, not a new
	// framework. Only the audited high-impact actions use it (see "Confirmation Policy" in docs/project/architecture.md);
	// routine and value-entry actions never do. Prompts name the action and its scope and carry no "Trainer: " prefix.
	internal static class Confirmation
	{
		// questionID is never passed: with an id the game offers "Don't ask again" and then runs the action without a dialog.
		// pausing is false so the dialog does not touch the game's pause counters (Disable Force Pause overrides them anyway);
		// the callback checks the game state again instead. No / Esc / closing never reach onConfirm, so they change nothing.
		internal static void Ask(string actionName, string message, Action onConfirm)
		{
			WindowManager.Instance.ShowMessageBox(message, false, DialogWindow.DialogType.Warning, () =>
			{
				if (Helpers.RequireGameReady(actionName))
					onConfirm();
			});
		}

		// The scope the player was shown against the scope found when they answered: act only when it is the same
		// non-empty scope, never on more, fewer or other things than were shown.
		internal static bool IsScopeUnchanged(int shown, int current)
		{
			return current > 0 && current == shown;
		}

		internal static void ShowScopeChanged()
		{
			Notification.ShowError("ConfirmScopeChanged".LocDef("The situation changed while the question was open. Nothing was changed. Run the action again."));
		}
	}
}
