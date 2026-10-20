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

		// Rows the window shows at once: the scrolling columns are capped at maxVisibleRows, but the window never
		// gets shorter than the columns that do not scroll.
		public static int VisibleRows(int tallestScrollingColumn, int otherColumnsRows, int maxVisibleRows)
		{
			return System.Math.Max(System.Math.Min(tallestScrollingColumn, maxVisibleRows), otherColumnsRows);
		}
	}
}
