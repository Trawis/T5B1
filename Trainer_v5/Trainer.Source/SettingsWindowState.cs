namespace Trainer_v5
{
	// Pure decisions behind the Trainer Settings window, kept apart from the Unity types so tests can reach them.
	internal static class SettingsWindowState
	{
		// The field's text is real text, not a placeholder: show the name the actions will use when there is one.
		public static string ProductNameFieldText(string currentName, string hint)
		{
			return string.IsNullOrEmpty(currentName) ? hint : currentName;
		}
	}
}
