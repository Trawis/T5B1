using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SettingsWindowStateTests
	{
		// 18 rows is the tallest page (Toggles); 1366x768 leaves about 16, 1920x1080 about 26.
		[Theory]
		[InlineData(18, 6, 26, 18)]
		[InlineData(18, 6, 6, 6)]
		public void TheWindowIsCappedByTheScreenButNeverShorterThanThePageThatDoesNotScroll(int tallest, int other, int maxVisible, int expected)
		{
			Assert.Equal(expected, SettingsWindowState.VisibleRows(tallest, other, maxVisible));
		}

		[Theory]
		[InlineData(2, 2)]
		[InlineData(4, 1)]
		public void AnUnknownTabFallsBackToActions(int stored, int expected)
		{
			Assert.Equal((SettingsTab)expected, SettingsWindowState.ClampTab((SettingsTab)stored));
		}

		[Fact]
		public void TheSettingsPageFitsBesideTheTallestRealPage()
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

			Assert.True(SettingsWindowState.SettingsPageRows <= SettingsWindowState.VisibleRows(System.Math.Max(actionRows, toggleRowCount), SettingsWindowState.SettingsPageRows, 6));
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
