namespace Trainer_v5
{
	internal static class StockSale
	{
		public static double Proceeds(uint copies, float price)
		{
			return copies * (price / 2.0);
		}
	}
}
