using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	internal static class Confirmation
	{
		// The only Yes/No question. No questionID: with one the game offers "Don't ask again" and then skips the dialog.
		// findChange re-resolves the targets on Yes and returns why nothing may be applied, or null; it may be null.
		// apply returns its result message, or null when it showed its own.
		internal static void AskAndApply(string actionName, string question, Func<string> findChange, Func<string> apply)
		{
			GameSettings game = GameSettings.Instance;
			WindowManager.Instance.ShowMessageBox(question, false, DialogWindow.DialogType.Warning, () => Run(
				() => Helpers.RequireGameReady(actionName),
				() => !ReferenceEquals(game, GameSettings.Instance) ? ScopeChangedText() : findChange != null ? findChange() : null,
				apply,
				Notification.ShowError,
				Notification.Popup));
		}

		// The dialog stays open while the game runs, so everything is decided again on Yes.
		internal static void Run(Func<bool> isReady, Func<string> findChange, Func<string> apply, Action<string> showChange, Action<string> showResult)
		{
			if (!isReady())
			{
				return;
			}

			string change = findChange();
			if (change != null)
			{
				showChange(change);
				return;
			}

			string result = apply();
			if (result != null)
			{
				showResult(result);
			}
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

		internal static string ChangedUnless(bool unchanged)
		{
			return unchanged ? null : ScopeChangedText();
		}

		internal static string ScopeChangedText()
		{
			return "ConfirmScopeChanged".LocDef("The situation changed while the question was open. Nothing was changed. Run the action again.");
		}

		internal static void ShowScopeChanged()
		{
			Notification.ShowError(ScopeChangedText());
		}
	}
}
