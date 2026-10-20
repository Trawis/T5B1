using System;

namespace Trainer_v5
{
	// The one place a trainer action asks Yes/No, through the game's own dialog; prompts have no "Trainer: " prefix.
	internal static class Confirmation
	{
		// No questionID: with one the game offers "Don't ask again" and then skips the dialog.
		internal static void Ask(string actionName, string message, Action onConfirm)
		{
			WindowManager.Instance.ShowMessageBox(message, false, DialogWindow.DialogType.Warning, () =>
			{
				if (Helpers.RequireGameReady(actionName))
					onConfirm();
			});
		}

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
