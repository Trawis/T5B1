using UnityEngine;
using Xunit;

namespace Trainer_v5.Tests
{
	public class UIHelperLayoutTests
	{
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
	}
}
