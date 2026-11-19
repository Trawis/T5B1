using Xunit;

namespace Trainer_v5.Tests
{
	public class ProductsWindowTrainerTests
	{
		[Theory]
		[InlineData(0, 0, "None")]
		[InlineData(0, 1, "One")]
		[InlineData(0, 2, "Several")]
		[InlineData(0, 9, "Several")]
		[InlineData(1, 1, "Unsupported")]
		[InlineData(2, 1, "Unsupported")]
		[InlineData(1, 0, "Unsupported")]
		public void OnlyOneSelectedRowOfTheSoftwareProductsListIsATarget(int mode, int selectedRows, string expected)
		{
			Assert.Equal(expected, ProductsWindowTrainer.Decide(mode, selectedRows).ToString());
		}
	}
}
