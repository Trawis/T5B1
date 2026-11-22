using Xunit;

namespace Trainer_v5.Tests
{
	public class StockSaleTests
	{
		[Fact]
		public void AnOddPriceIsNotTruncatedBeforeMultiplying()
		{
			Assert.Equal(2.25, StockSale.Proceeds(3, 1.5f), 6);
			Assert.Equal(7.5, StockSale.Proceeds(3, 5f), 6);
		}

		[Fact]
		public void HugeStockDoesNotWrapToANegativeAmount()
		{
			var proceeds = StockSale.Proceeds(uint.MaxValue, 100f);

			Assert.True(proceeds > 2e11, proceeds.ToString());
		}
	}
}
