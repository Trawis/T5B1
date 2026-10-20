using System;
using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	internal static class ProductFeatures
	{
		private static readonly PrinterSpeedBaselines _printerSpeeds = new PrinterSpeedBaselines();

		private static GameSettings Settings => GameSettings.Instance;

		internal static void SellProductStock()
		{
			SoftwareProduct[] Products = Settings.MyCompany.Products
												 .Where(product => product.Userbase == 0 && product.PhysicalCopies > 0)
												 .ToArray();

			double total = 0;
			for (int i = 0; i < Products.Length; i++)
			{
				SoftwareProduct product = Products[i];
				uint copies = product.PhysicalCopies;
				double proceeds = StockSale.Proceeds(copies, product.Price);

				// ChangeAllPhysicalStock also removes the stock of forced add-ons; its argument is an int.
				for (uint left = copies; left > 0;)
				{
					int chunk = (int)Math.Min(left, (uint)int.MaxValue);
					product.ChangeAllPhysicalStock(-chunk);
					left -= (uint)chunk;
				}

				Settings.MyCompany.MakeTransaction(proceeds, Company.TransactionCategory.Sales);
				total += proceeds;
			}

			// Reported after selling, so the player is never told about a sale that did not happen.
			WindowManager.SpawnDialog(StockSale.Summary(Products.Length, total), false, DialogWindow.DialogType.Information);
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

		internal static void ApplyFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = 0f);
		}

		internal static void RestoreFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = Constants.PRINTER_PRICE_DEFAULT);
		}

		// PrintSpeed has no game writer and is not saved (ProductPrinter.SerializeMe), so the value read before the first write is the native one.
		internal static void ApplyIncreasePrintSpeed()
		{
			RetainLivePrinterSpeeds();
			Settings.ProductPrinters.ForEach(p =>
			{
				_printerSpeeds.Capture(p.GetInstanceID(), p.PrintSpeed);
				p.PrintSpeed = 2f;
			});
		}

		internal static void RestoreIncreasePrintSpeed()
		{
			RetainLivePrinterSpeeds();
			Settings.ProductPrinters.ForEach(p =>
			{
				float baseline;
				if (_printerSpeeds.TryTake(p.GetInstanceID(), out baseline))
				{
					p.PrintSpeed = baseline;
				}
			});
		}

		// Printer baselines belong to one game; the next game's printers start without any.
		internal static void ResetGameState()
		{
			_printerSpeeds.Clear();
		}

		private static void RetainLivePrinterSpeeds()
		{
			HashSet<int> live = new HashSet<int>();
			Settings.ProductPrinters.ForEach(p => live.Add(p.GetInstanceID()));
			_printerSpeeds.Retain(live);
		}

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
