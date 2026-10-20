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

		// 28 rows is the tallest column; 1366x768 leaves about 17, 1920x1080 about 27.
		[Theory]
		[InlineData(28, 11, 17, 17)]
		[InlineData(28, 11, 27, 27)]
		[InlineData(28, 11, 40, 28)]
		[InlineData(28, 11, 6, 11)]
		public void TheWindowIsCappedByTheScreenButNeverShorterThanTheStaticColumns(int tallest, int other, int maxVisible, int expected)
		{
			Assert.Equal(expected, SettingsWindowState.VisibleRows(tallest, other, maxVisible));
		}
	}
}
