using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	internal static class ProductFeatures
	{
		internal static void SellProductStock()
		{
			WindowManager.SpawnDialog("Stock of products with no active users were sold at half the price.",
				false, DialogWindow.DialogType.Information);

			SoftwareProduct[] Products = Settings.MyCompany.Products
												 .Where(product => product.Userbase == 0)
												 .ToArray();

			if (Products.Length == 0)
			{
				return;
			}

			for (int i = 0; i < Products.Length; i++)
			{
				SoftwareProduct product = Products[i];
				int st = (int)product.PhysicalCopies * (int)(product.Price / 2);

				product.PhysicalCopies = 0;
				Settings.MyCompany.MakeTransaction(st, Company.TransactionCategory.Sales);
			}
		}

		internal static void RemoveSoft()
		{
			return;

			//currently broken
			SDateTime time = new SDateTime(1, 70);
			CompanyType type = new CompanyType();
			var dict = new Dictionary<string, string[]>();
			SimulatedCompany simComp = new SimulatedCompany("Trainer Company", time, type, dict, 0f, MarketSimulation.Active);
			simComp.CanMakeTransaction(2139095030f);

			SoftwareProduct[] Products = Settings.simulation.GetAllProducts(true).Where(product =>
				product.DevCompany == Settings.MyCompany &&
				product.Inventor != Settings.MyCompany.Name).ToArray();

			if (Products.Length == 0)
			{
				return;
			}

			for (int i = 0; i < Products.Length; i++)
			{
				SoftwareProduct Product = Products[i];

				Product.Userbase = 0;
				Product.PhysicalCopies = 0;
				Product.Marketing = 0;
				Product.Trade(simComp, time);
			}

			WindowManager.SpawnDialog("Products that you didn't invent are removed.", false, DialogWindow.DialogType.Information);
		}

		internal static void SetProductPriceAction(float price)
		{
			string error;
			SoftwareProduct Product = ResolveProductByName(Helpers.ProductPriceName, out error);

			if (Product == null)
			{
				WindowManager.SpawnDialog(error, false, DialogWindow.DialogType.Information);
				return;
			}

			Product.Price = price;
			HUD.Instance.AddPopupMessage("Trainer: Price for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void SetProductPrice()
		{
			InputHelper.RequestFloat("Type product price:", "Product price", SetProductPriceAction, 0f, float.MaxValue);
		}

		internal static void SetProductStockAction(uint stock)
		{
			string error;
			SoftwareProduct Product = ResolveProductByName(Helpers.ProductPriceName, out error);

			if (Product == null)
			{
				WindowManager.SpawnDialog(error, false, DialogWindow.DialogType.Information);
				return;
			}

			Product.PhysicalCopies = stock;
			HUD.Instance.AddPopupMessage("Trainer: Stock for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void SetProductStock()
		{
			InputHelper.RequestUInt("Type product stock:", "Product stock", "100000", SetProductStockAction);
		}

		internal static void AddActiveUsersAction(int users)
		{
			string error;
			SoftwareProduct Product = ResolveProductByName(Helpers.ProductPriceName, out error);

			if (Product == null)
			{
				WindowManager.SpawnDialog(error, false, DialogWindow.DialogType.Information);
				return;
			}

			Product.Userbase = users;
			HUD.Instance.AddPopupMessage("Trainer: Active users for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void AddActiveUsers()
		{
			InputHelper.RequestInt("Type product active users:", "Product active users", "100000", AddActiveUsersAction, 0, int.MaxValue);
		}

		internal static void MaxMarketShare()
		{
			// Disabled, see #150
		}

		internal static void ApplyFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = 0f);
		}

		internal static void RestoreFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = Constants.PRINTER_PRICE_DEFAULT);
		}

		internal static void ApplyIncreasePrintSpeed()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintSpeed = 2f);
		}

		private static GameSettings Settings => GameSettings.Instance;

		// Shared by the price, stock and active-users actions; rejects ambiguous names.
		private static SoftwareProduct ResolveProductByName(string name, out string errorMessage)
		{
			List<SoftwareProduct> matches = Settings.simulation.GetAllProducts(false)
				.Where(product => product.Name == name)
				.ToList();

			if (matches.Count == 0)
			{
				errorMessage = "Trainer: Product " + name + " not found!";
				return null;
			}

			if (matches.Count > 1)
			{
				string owners = string.Join(", ", matches.Select(product => product.DevCompany != null ? product.DevCompany.Name : "Unknown").ToArray());
				errorMessage = "Trainer: Product name " + name + " is ambiguous (owned by: " + owners + "). Rename the product or use a unique name.";
				return null;
			}

			errorMessage = null;
			return matches[0];
		}
	}
}
