namespace Trainer_v5
{
	public static class Notification
	{
		public static void ShowError(string msg)
		{
			WindowManager.SpawnDialog(msg, false, DialogWindow.DialogType.Error);
		}
	}
}
