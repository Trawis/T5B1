namespace Trainer_v5
{
	public static class Notification
	{
		public static void Popup(string msg, string icon)
		{
			HUD.Instance.AddPopupMessage(msg, icon, PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}


		public static void ShowError(string msg)
		{
			WindowManager.SpawnDialog(msg, false, DialogWindow.DialogType.Error);
		}
	}
}
