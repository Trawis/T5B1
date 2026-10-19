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
	}
}
