using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SettingsWindowStateTests
	{
		[Theory]
		[InlineData(null, "Product Name Here", "Product Name Here")]
		[InlineData("", "Product Name Here", "Product Name Here")]
		[InlineData("My Product", "Product Name Here", "My Product")]
		public void ProductNameFieldShowsTheNameTheActionsUse(string current, string hint, string expected)
		{
			Assert.Equal(expected, SettingsWindowState.ProductNameFieldText(current, hint));
		}

		// 18 rows is the tallest page (Toggles); 1366x768 leaves about 16, 1920x1080 about 26.
		[Theory]
		[InlineData(18, 6, 16, 16)]
		[InlineData(18, 6, 26, 18)]
		[InlineData(18, 6, 40, 18)]
		[InlineData(18, 6, 6, 6)]
		[InlineData(4, 6, 6, 6)]
		public void TheWindowIsCappedByTheScreenButNeverShorterThanThePageThatDoesNotScroll(int tallest, int other, int maxVisible, int expected)
		{
			Assert.Equal(expected, SettingsWindowState.VisibleRows(tallest, other, maxVisible));
		}

		// Title bar + tab strip + content rows: the page viewports end exactly at the window's bottom edge.
		[Theory]
		[InlineData(6, 256)]
		[InlineData(16, 576)]
		public void TheWindowHeightIsTitleBarTabStripAndTheContentRows(int visibleRows, int expected)
		{
			Assert.Equal(expected, SettingsWindowState.WindowHeight(visibleRows));
			Assert.Equal(visibleRows * Constants.ELEMENT_HEIGHT + 2 * Constants.ELEMENT_HEIGHT, SettingsWindowState.WindowHeight(visibleRows));
		}

		[Fact]
		public void TheWindowWidthIsTheViewportWidthTheTabPagesUse()
		{
			Assert.Equal(634, SettingsWindowState.WindowWidth);
		}

		[Theory]
		[InlineData(0, 1)]
		[InlineData(1, 1)]
		[InlineData(2, 2)]
		[InlineData(3, 3)]
		[InlineData(4, 1)]
		[InlineData(-1, 1)]
		public void AnUnknownTabFallsBackToActions(int stored, int expected)
		{
			Assert.Equal((SettingsTab)expected, SettingsWindowState.ClampTab((SettingsTab)stored));
		}

		[Fact]
		public void ThePagesAreNumberedFromZeroInTabOrder()
		{
			Assert.Equal(0, SettingsWindowState.PageIndex(SettingsTab.Actions));
			Assert.Equal(1, SettingsWindowState.PageIndex(SettingsTab.Toggles));
			Assert.Equal(2, SettingsWindowState.PageIndex(SettingsTab.Settings));
			Assert.Equal(0, SettingsWindowState.PageIndex(SettingsTab.None));
		}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 1)]
		[InlineData(4, 1)]
		[InlineData(5, 2)]
		[InlineData(13, 4)]
		public void ASectionTakesOneRowPerFourControls(int count, int expectedRows)
		{
			Assert.Equal(expectedRows, SettingsWindowState.SectionRows(count, SettingsWindowState.Columns));
		}

		[Fact]
		public void EverySectionWithControlsAddsAHeaderRowAndEmptySectionsAddNothing()
		{
			// 9 control rows plus five section headers.
			Assert.Equal(14, SettingsWindowState.PageRows(new[] { 11, 2, 8, 5, 2 }, SettingsWindowState.Columns));
			Assert.Equal(2, SettingsWindowState.PageRows(new[] { 0, 4, 0 }, SettingsWindowState.Columns));
			Assert.Equal(0, SettingsWindowState.PageRows(new int[0], SettingsWindowState.Columns));
		}

		[Fact]
		public void ThePagesOfTheRealTablesFitTheNumbersTheLayoutWasDesignedFor()
		{
			var actions = new Dictionary<TrainerDomain, int>();
			foreach (var row in SettingActionRows.Create())
			{
				if (row.Tab == SettingsTab.Actions)
				{
					actions[row.Domain] = (actions.ContainsKey(row.Domain) ? actions[row.Domain] : 0) + 1;
				}
			}

			actions[TrainerDomain.Product]++; // the Product Name field is the first cell of that section
			var toggles = new Dictionary<TrainerDomain, int>();
			var toggleRows = SettingToggleRows.Create();
			toggleRows.AddRange(SettingToggleRows.CreateImmediate());
			foreach (var row in toggleRows)
			{
				toggles[row.Domain] = (toggles.ContainsKey(row.Domain) ? toggles[row.Domain] : 0) + 1;
			}

			var actionRows = SettingsWindowState.PageRows(Counts(actions, SettingsWindowState.ActionDomains()), SettingsWindowState.Columns);
			var toggleRowCount = SettingsWindowState.PageRows(Counts(toggles, SettingsWindowState.ToggleDomains()), SettingsWindowState.Columns);

			Assert.Equal(14, actionRows);
			Assert.Equal(18, toggleRowCount);
			Assert.True(SettingsWindowState.SettingsPageRows <= SettingsWindowState.VisibleRows(System.Math.Max(actionRows, toggleRowCount), SettingsWindowState.SettingsPageRows, 6));
		}

		[Theory]
		[InlineData(0, 0, 0)]
		[InlineData(1, 0, 1)]
		[InlineData(2, 1, 0)]
		[InlineData(3, 1, 1)]
		[InlineData(4, 0, 3)]
		public void EfficiencyGroupsSitSideBySideWithTheLabelAboveItsDropDownAndDiscordBelowAGap(int cell, int column, int row)
		{
			Assert.Equal(column, SettingsWindowState.SettingsCellColumn(cell));
			Assert.Equal(row, SettingsWindowState.SettingsCellRow(cell));
			Assert.True(SettingsWindowState.SettingsCellRow(cell) < SettingsWindowState.SettingsPageRows);
		}

		[Fact]
		public void TheDiscordButtonSitsOnTheLastPageRowWithAGapAboveIt()
		{
			Assert.Equal(SettingsWindowState.SettingsPageRows - 1, SettingsWindowState.SettingsCellRow(SettingsWindowState.EfficiencyCellCount));
			Assert.True(SettingsWindowState.SettingsCellRow(SettingsWindowState.EfficiencyCellCount) > SettingsWindowState.SettingsCellRow(3) + 1);
			Assert.True(SettingsWindowState.SettingsCellColumn(2) < SettingsWindowState.Columns);
		}

		[Fact]
		public void TheWindowIsFourSlotsWide()
		{
			Assert.Equal(161, SettingsWindowState.ColumnPitch);
			Assert.Equal(634, SettingsWindowState.WindowWidth);
		}

		[Fact]
		public void TheWindowHoldsTheLastColumnAndFitsTheSmallestCanvasWidth()
		{
			Assert.Equal(SettingsWindowState.WindowWidth, UIHelper.GetGridSlot(SettingsWindowState.Columns - 1, SettingsWindowState.Columns, 0).xMax + Constants.FIRST_COLUMN);
			Assert.True(SettingsWindowState.WindowWidth <= 1280 / 2);
		}

		private static int[] Counts(Dictionary<TrainerDomain, int> perDomain, TrainerDomain[] order)
		{
			var counts = new int[order.Length];
			for (int i = 0; i < order.Length; i++)
			{
				int count;
				counts[i] = perDomain.TryGetValue(order[i], out count) ? count : 0;
			}

			return counts;
		}
	}
}
