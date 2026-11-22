using Xunit;

namespace Trainer_v5.Tests
{
	// Which product the Products-list buttons act on. Liveness and ownership need game objects and are covered in-game.
	public class ProductsWindowTrainerTests
	{
		[Theory]
		[InlineData(0, 1, "One")]
		[InlineData(0, 2, "Several")]
		[InlineData(1, 1, "Unsupported")]
		public void OnlyOneSelectedRowOfTheSoftwareProductsListIsATarget(int mode, int selectedRows, string expected)
		{
			Assert.Equal(expected, ProductsWindowTrainer.Decide(mode, selectedRows).ToString());
		}
	}
}
