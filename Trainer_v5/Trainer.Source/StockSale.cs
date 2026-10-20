namespace Trainer_v5
{
	internal static class StockSale
	{
		// Half the price per copy, in double: a uint copy count times an int price overflowed (#233).
		public static double Proceeds(uint copies, float price)
		{
			return copies * (price / 2.0);
		}

		public static string Summary(int products, double total)
		{
			if (products == 0)
				return "Trainer: No product without active users has stock to sell. Nothing was sold.";

			return "Stock of " + products + " product(s) with no active users was sold at half the price for "
				+ total.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + ".";
		}
	}
}
