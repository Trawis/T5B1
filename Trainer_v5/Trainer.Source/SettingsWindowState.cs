namespace Trainer_v5
{
	// Pure decisions behind the Trainer Settings window, kept apart from the Unity types so tests can reach them.
	internal static class SettingsWindowState
	{
		// Window width: Columns slots at the column pitch plus 2 px of margin.
		public const int Columns = 4;
		public const int ColumnPitch = Constants.THIRD_COLUMN - Constants.SECOND_COLUMN;
		public const int WindowWidth = Columns * ColumnPitch + 2;

		// Two label + drop-down pairs (4 rows), a gap and the Discord button.
		public const int SettingsPageRows = 6;

		public static SettingsTab ClampTab(SettingsTab tab)
		{
			return tab < SettingsTab.Actions || tab > SettingsTab.Settings ? SettingsTab.Actions : tab;
		}

		public static int PageIndex(SettingsTab tab)
		{
			return (int)ClampTab(tab) - (int)SettingsTab.Actions;
		}

		public static int SectionRows(int cellCount, int columns)
		{
			return cellCount <= 0 ? 0 : (cellCount + columns - 1) / columns;
		}

		public static int PageRows(int[] sectionCellCounts, int columns)
		{
			var rows = 0;
			foreach (var count in sectionCellCounts)
			{
				rows += count <= 0 ? 0 : 1 + SectionRows(count, columns);
			}

			return rows;
		}

		public static TrainerDomain[] ActionDomains()
		{
			return new[] { TrainerDomain.Company, TrainerDomain.Employee, TrainerDomain.Product, TrainerDomain.Work, TrainerDomain.Environment };
		}

		// Sections of the Toggles page, largest first.
		public static TrainerDomain[] ToggleDomains()
		{
			return new[] { TrainerDomain.Employee, TrainerDomain.Environment, TrainerDomain.Work, TrainerDomain.Company, TrainerDomain.Product };
		}

		// The field's text is real text, not a placeholder: show the name the actions will use when there is one.
		public static string ProductNameFieldText(string currentName, string hint)
		{
			return string.IsNullOrEmpty(currentName) ? hint : currentName;
		}

		// Same on every page so the window keeps its size; never shorter than the page that does not scroll.
		public static int VisibleRows(int tallestScrollingPage, int nonScrollingPageRows, int maxVisibleRows)
		{
			return System.Math.Max(System.Math.Min(tallestScrollingPage, maxVisibleRows), nonScrollingPageRows);
		}
	}
}
