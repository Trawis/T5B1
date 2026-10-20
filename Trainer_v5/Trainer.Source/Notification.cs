using System;

namespace Trainer_v5
{
	// The one place trainer-owned messages reach the game (#451). A HUD popup always starts with "Trainer: " exactly
	// once, whether or not the localized text already carries it. Dialogs state their own context and get no prefix, and
	// input dialogs (WindowManager.SpawnInputDialog, InputHelper) are separate: their title and prompt are never prefixed.
	// Information, warning and error dialogs are not modal, as before. Diagnostic logging (Logger) stays separate.
	public static class Notification
	{
		public const string Prefix = "Trainer: ";

		private const string PrefixWithoutSpace = "Trainer:";

		// Popup arguments are the ones the trainer has always used: Cogs icon, no action, and the default sound,
		// importance and popup id.
		public static void Popup(string msg)
		{
			HUD.Instance.AddPopupMessage(WithPrefix(msg), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void ShowInfo(string msg)
		{
			WindowManager.SpawnDialog(msg, false, DialogWindow.DialogType.Information);
		}

		public static void ShowWarning(string msg)
		{
			WindowManager.SpawnDialog(msg, false, DialogWindow.DialogType.Warning);
		}

		public static void ShowError(string msg)
		{
			WindowManager.SpawnDialog(msg, false, DialogWindow.DialogType.Error);
		}

		// Pure text rule, no game types: text that already starts with the prefix is returned unchanged (ordinal, case
		// sensitive); a translation that wrote "Trainer:" without the space gets the space; anything else gets the prefix.
		internal static string WithPrefix(string message)
		{
			if (message == null)
				message = string.Empty;

			if (message.StartsWith(Prefix, StringComparison.Ordinal))
				return message;

			if (message.StartsWith(PrefixWithoutSpace, StringComparison.Ordinal))
				return Prefix + message.Substring(PrefixWithoutSpace.Length);

			return Prefix + message;
		}
	}
}
