using UnityEngine;
using Xunit;

namespace Trainer_v5.Tests
{
	public class UIHelperLayoutTests
	{
		[Theory]
		[InlineData(100, 100, 0, 132)]
		[InlineData(100, 164, 0, 196)]
		[InlineData(164, 100, 4, 200)]
		[InlineData(0, 0, 4, 36)]
		[InlineData(0, 0, 0, 32)]
		public void ColumnWindowHeightIsTallestColumnPlusOriginOffsetPlusTitleBar(int first, int second, int originY, int expected)
		{
			Assert.Equal(expected, UIHelper.GetColumnWindowHeight(first, second, originY));
		}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 32)]
		[InlineData(5, 160)]
		public void PlainColumnsStackRowsAtTheElementHeight(int index, int expectedY)
		{
			Assert.Equal(new Rect(161, expectedY, Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT), UIHelper.GetColumnSlot(index, 161, false));
		}

		[Theory]
		[InlineData(0, -16)]
		[InlineData(1, 0)]
		[InlineData(2, 48)]
		[InlineData(3, 64)]
		public void ComboBoxColumnsShiftLabelsDownAndPairThemWithTheirDropdown(int index, int expectedY)
		{
			Assert.Equal(new Rect(644, expectedY, Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT), UIHelper.GetColumnSlot(index, 644, true));
		}

		[Theory]
		[InlineData(0, 1)]
		[InlineData(1, 161)]
		[InlineData(2, 322)]
		[InlineData(3, 483)]
		public void GridColumnsAreTheSlotsOfTheFourSettingsColumns(int column, int expectedX)
		{
			Assert.Equal(expectedX, UIHelper.GetGridColumnX(column));
		}

		[Theory]
		[InlineData(0, 0, 1, 0)]
		[InlineData(3, 0, 483, 0)]
		[InlineData(4, 0, 1, 32)]
		[InlineData(5, 2, 161, 96)]
		[InlineData(11, 1, 483, 96)]
		public void GridSlotsFillRowsOfFourFromTheFirstRow(int index, int firstRow, int expectedX, int expectedY)
		{
			Assert.Equal(new Rect(expectedX, expectedY, Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT), UIHelper.GetGridSlot(index, 4, firstRow));
		}

		[Fact]
		public void ASingleColumnGridStacksRows()
		{
			Assert.Equal(new Rect(1, 160, Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT), UIHelper.GetGridSlot(5, 1, 0));
		}

		[Theory]
		[InlineData(1080, 32, 1, 26)]
		[InlineData(720, 32, 1, 15)]
		[InlineData(540, 32, 5, 6)]
		[InlineData(360, 32, 1, 6)]
		[InlineData(360, 32, 5, 5)]
		[InlineData(360, 34, 4, 5)]
		[InlineData(1080, 34, 4, 21)]
		[InlineData(10, 32, 5, 1)]
		public void VisibleRowsKeepTheMinimumOnlyWhileTheWindowStillFitsTheCanvas(int canvasHeight, int rowStep, int reservedRows, int expected)
		{
			Assert.Equal(expected, UIHelper.FitRows(canvasHeight, rowStep, reservedRows));
		}

		[Theory]
		[InlineData(360, 0, 5, 0)]
		[InlineData(360, 2, 4, 4)]
		[InlineData(450, 0, 2, 0)]
		[InlineData(540, 0, 5, 0)]
		[InlineData(240, 0, 2, 0)]
		public void TheWindowFromVisibleRowsNeverExceedsTheCanvas(int canvasHeight, int gap, int reservedRows, int originY)
		{
			var step = Constants.ELEMENT_HEIGHT + gap;
			var rows = UIHelper.FitRows(canvasHeight, step, reservedRows);
			var columnHeight = (reservedRows + rows) * step - gap;

			Assert.True(UIHelper.GetColumnWindowHeight(columnHeight, 0, originY) <= canvasHeight);
		}
	}
}
