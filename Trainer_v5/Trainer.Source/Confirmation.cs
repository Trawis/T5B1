using System;
using System.Collections.Generic;

namespace Trainer_v5
{
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

		// The same non-empty targets in any order: nothing added, removed or replaced, even at the same count.
		internal static bool IsSameTargets<T>(List<T> shown, List<T> current) where T : class
		{
			if (shown.Count == 0 || shown.Count != current.Count)
			{
				return false;
			}

			var remaining = new HashSet<T>(shown);
			foreach (var target in current)
			{
				if (!remaining.Remove(target))
				{
					return false;
				}
			}

			return remaining.Count == 0;
		}

		internal static void ShowScopeChanged()
		{
			Notification.ShowError("ConfirmScopeChanged".LocDef("The situation changed while the question was open. Nothing was changed. Run the action again."));
		}
	}
}
