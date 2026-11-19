namespace Trainer_v5
{
	internal static class StockSale
	{
		// Half the price per copy, in double: a uint copy count times an int price overflowed (#233).
		public static double Proceeds(uint copies, float price)
		{
			return copies * (price / 2.0);
		}
	}
}
