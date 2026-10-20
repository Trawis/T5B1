namespace Trainer_v5
{
	// Pure decisions behind the Trainer Settings window, kept apart from the Unity types so tests can reach them.
	internal static class SettingsWindowState
	{
		// Cells per row on the Actions and Toggles pages, and the window width they need: the column slots are
		// Constants.FIRST_COLUMN.. 161 px apart, with the same 1 px margin on the left and one more on the right.
		public const int Columns = 4;
		public const int ColumnPitch = Constants.THIRD_COLUMN - Constants.SECOND_COLUMN;
		public const int WindowWidth = Columns * ColumnPitch + 2;

		// Rows on the Settings page: label and drop-down for each efficiency, a gap, and the Discord button.
		public const int SettingsPageRows = 6;

		// The page to show: none chosen yet, or a value outside the pages, means Actions.
		public static SettingsTab ClampTab(SettingsTab tab)
		{
			return tab < SettingsTab.Actions || tab > SettingsTab.Settings ? SettingsTab.Actions : tab;
		}

		// Position of a page among the three, 0 for Actions.
		public static int PageIndex(SettingsTab tab)
		{
			return (int)ClampTab(tab) - (int)SettingsTab.Actions;
		}

		// Rows a section of cellCount controls occupies when laid out columns per row.
		public static int SectionRows(int cellCount, int columns)
		{
			return cellCount <= 0 ? 0 : (cellCount + columns - 1) / columns;
		}

		// Rows of a page: every section that has controls takes one header row plus its control rows.
		public static int PageRows(int[] sectionCellCounts, int columns)
		{
			var rows = 0;
			foreach (var count in sectionCellCounts)
			{
				rows += count <= 0 ? 0 : 1 + SectionRows(count, columns);
			}

			return rows;
		}

		// Sections of the Actions page, top to bottom.
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

		// Rows the window content shows at once, the same on every page so the window does not change size when the
		// page changes: the scrolling pages are capped at maxVisibleRows, but the window never gets shorter than the
		// page that does not scroll.
		public static int VisibleRows(int tallestScrollingPage, int nonScrollingPageRows, int maxVisibleRows)
		{
			return System.Math.Max(System.Math.Min(tallestScrollingPage, maxVisibleRows), nonScrollingPageRows);
		}
	}
}
