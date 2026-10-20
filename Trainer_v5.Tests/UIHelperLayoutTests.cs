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
	}
}
