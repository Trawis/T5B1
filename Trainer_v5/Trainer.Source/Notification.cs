using System;

namespace Trainer_v5
{
	public static class Notification
	{
		public const string Prefix = "Trainer: ";

		private const string PrefixWithoutSpace = "Trainer:";

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
