using UnityEngine;
using Xunit;

namespace Trainer_v5.Tests
{
	public class UIHelperLayoutTests
	{
		[Theory]
		[InlineData(1080, 32, 1, 26)]
		[InlineData(360, 32, 1, 6)]
		[InlineData(10, 32, 5, 1)]
		public void VisibleRowsKeepTheMinimumOnlyWhileTheWindowStillFitsTheCanvas(int canvasHeight, int rowStep, int reservedRows, int expected)
		{
			Assert.Equal(expected, UIHelper.FitRows(canvasHeight, rowStep, reservedRows));
		}

		[Theory]
		[InlineData(360, 2, 4, 4)]
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
