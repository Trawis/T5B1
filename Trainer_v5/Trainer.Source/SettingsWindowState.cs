namespace Trainer_v5
{
	internal static class SettingsWindowState
	{
		public const int Columns = 4;
		public const int ColumnPitch = Constants.THIRD_COLUMN - Constants.SECOND_COLUMN;
		public const int WindowWidth = Constants.FOURTH_COLUMN + Constants.ELEMENT_WIDTH + Constants.FIRST_COLUMN;

		public const int SettingsPageRows = 5;

		public const int EfficiencyCellCount = 4;

		public static int SettingsCellColumn(int cellIndex)
		{
			return cellIndex < EfficiencyCellCount ? cellIndex / 2 : 0;
		}

		public static int SettingsCellRow(int cellIndex)
		{
			return cellIndex < EfficiencyCellCount ? 1 + cellIndex % 2 : SettingsPageRows - 1;
		}

		public static int WindowHeight(int visibleRows)
		{
			return (visibleRows + 2) * Constants.ELEMENT_HEIGHT;
		}

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

		public static int PageRowsWithExperimental(int regularRows, int experimentalRows)
		{
			return experimentalRows > 0 ? regularRows + 1 + experimentalRows : regularRows;
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

		public static TrainerDomain[] ToggleDomains()
		{
			return new[] { TrainerDomain.Employee, TrainerDomain.Environment, TrainerDomain.Work, TrainerDomain.Company, TrainerDomain.Product };
		}

		public static string ProductNameFieldText(string currentName, string hint)
		{
			return string.IsNullOrEmpty(currentName) ? hint : currentName;
		}

		public static int VisibleRows(int tallestScrollingPage, int nonScrollingPageRows, int maxVisibleRows)
		{
			return System.Math.Max(System.Math.Min(tallestScrollingPage, maxVisibleRows), nonScrollingPageRows);
		}
	}
}
