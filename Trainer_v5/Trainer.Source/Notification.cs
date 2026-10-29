using System;

namespace Trainer_v5
{
	// The one place trainer-owned messages reach the game (#451). A HUD popup starts with "Trainer: " exactly once;
	// dialogs state their own context and get no prefix, and are not modal. Input dialogs (InputHelper) are separate.
	public static class Notification
	{
		public const string Prefix = "Trainer: ";

		private const string PrefixWithoutSpace = "Trainer:";

		// Cogs icon, no action. The game shows the message only for PopupIDs.None (-1), and NotificationSound 0 is Issue
		// (an undismissable entry in the Issues panel), so the sound is Neutral.
		public static void Popup(string msg)
		{
			HUD.Instance.AddPopupMessage(WithPrefix(msg), "Cogs", PopupManager.PopUpAction.None, 0, PopupManager.NotificationSound.Neutral, 0f, PopupManager.PopupIDs.None);
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
