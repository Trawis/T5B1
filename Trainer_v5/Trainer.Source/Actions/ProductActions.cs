using System; // Needed for Math.Max
using System.Collections.Generic;
using System.Linq;
using OrbCreationExtensions;
using UnityEngine;

namespace Trainer_v5.Actions
{
	public static class ProductActions
	{
		private static GameSettings Settings => GameSettings.Instance;

		public static void SellProductStock()
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

			long totalRevenue = 0;
			for (int i = 0; i < Products.Length; i++)
			{
				SoftwareProduct product = Products[i];
				// Use long for intermediate calculation to prevent overflow
				long revenueFromProduct = (long)product.PhysicalCopies * (long)(product.Price / 2);
				totalRevenue += revenueFromProduct;

				product.PhysicalCopies = 0;
			}

			if (totalRevenue > 0)
			{
				Settings.MyCompany.MakeTransaction(totalRevenue, Company.TransactionCategory.Sales, "Sold obsolete stock");
			}
		}

		public static void RemoveSoft()
		{
			// Currently broken in original code, keeping return statement.
			// If fixed, logic would go here.
			WindowManager.SpawnDialog("RemoveSoft function is currently disabled (marked as broken in original code).", false, DialogWindow.DialogType.Information);
			return;

			/* // Original broken code:
			SDateTime time = new SDateTime(1, 70);
			CompanyType type = new CompanyType();
			var dict = new Dictionary<string, string[]>();
			SimulatedCompany simComp = new SimulatedCompany("Trainer Company", time, type, dict, 0f, MarketSimulation.Active);
			simComp.CanMakeTransaction(2139095030f); // This doesn't seem right, CanMakeTransaction usually returns bool

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
			*/
		}

		#region Fix Bugs

		public static void FixBugsAction(string input)
		{
			// Find the alpha work item by name, ensuring it's in beta
			SoftwareAlpha workItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>()
				.FirstOrDefault(item => item.Name.Equals(input, System.StringComparison.OrdinalIgnoreCase) && item.InBeta);

			if (workItem == null)
			{
				WindowManager.SpawnDialog($"Trainer: Alpha product '{input}' in beta not found.", false, DialogWindow.DialogType.Error);
				return;
			}

			workItem.FixedBugs = workItem.MaxBugs;
			HUD.Instance.AddPopupMessage($"Trainer: All bugs fixed for '{workItem.Name}'!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void FixBugs()
		{
			WindowManager.SpawnInputDialog("Type product name (in Beta):", "Fix Bugs", "", FixBugsAction);
		}

		#endregion

		#region Max Followers

		public static void MaxFollowersAction(string input)
		{
			// Find the alpha work item by name, ensuring it's not paused
			SoftwareAlpha alpha = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>()
				.FirstOrDefault(item => item.Name.Equals(input, System.StringComparison.OrdinalIgnoreCase) && !item.Paused);

			if (alpha == null)
			{
				WindowManager.SpawnDialog($"Trainer: Active alpha product '{input}' not found.", false, DialogWindow.DialogType.Error);
				return;
			}

			// Use a large but reasonable number to avoid potential overflow issues with float
			float followersToAdd = 100000000f; // 100 Million
			// Cast followersToAdd to uint to match MaxFollowers type
			alpha.MaxFollowers += (uint)followersToAdd; 
			alpha.ReEvaluateMaxFollowers();

			alpha.FollowerChange += followersToAdd;
			alpha.Followers += followersToAdd;

			HUD.Instance.AddPopupMessage($"Trainer: Followers maxed for '{alpha.Name}'!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void MaxFollowers()
		{
			WindowManager.SpawnInputDialog("Type product name (in Alpha):", "Max Followers", "", MaxFollowersAction);
		}

		#endregion

		#region Set Product Price

		// Helper to store the name temporarily between dialogs
		private static string _productNameForPrice;

		public static void SetProductPriceAction(string input)
		{
			SoftwareProduct product =
				Settings.MyCompany.Products.FirstOrDefault(p => p.Name.Equals(_productNameForPrice, System.StringComparison.OrdinalIgnoreCase));

			if (product == null)
			{
				// This case should ideally not happen if the name was validated before
				WindowManager.SpawnDialog($"Trainer: Product '{_productNameForPrice}' not found.", false, DialogWindow.DialogType.Error);
				return;
			}

			product.Price = input.ConvertToFloatDef(product.Price); // Default to current price if input is invalid
			HUD.Instance.AddPopupMessage($"Trainer: Price for '{product.Name}' set to {product.Price:C}!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			_productNameForPrice = null; // Clear temporary storage
		}

		public static void SetProductPrice()
		{
			// First, ask for the product name
			WindowManager.SpawnInputDialog("Type product name:", "Set Product Price - Name", "", (productName) =>
			{
				// Validate if product exists
				SoftwareProduct product = Settings.MyCompany.Products.FirstOrDefault(p => p.Name.Equals(productName, System.StringComparison.OrdinalIgnoreCase));
				if (product == null)
				{
					WindowManager.SpawnDialog($"Trainer: Product '{productName}' not found.", false, DialogWindow.DialogType.Error);
					return;
				}
				_productNameForPrice = productName; // Store name
				// Now ask for the price
				WindowManager.SpawnInputDialog($"Type new price for '{productName}':", "Set Product Price - Price", product.Price.ToString("F2"), SetProductPriceAction);
			});
		}

		#endregion

		#region Set Product Stock

		// Helper to store the name temporarily
		private static string _productNameForStock;
		public static void SetProductStockAction(string input)
		{
			SoftwareProduct product =
				Settings.MyCompany.Products.FirstOrDefault(p => p.Name.Equals(_productNameForStock, System.StringComparison.OrdinalIgnoreCase));

			if (product == null)
			{
				WindowManager.SpawnDialog($"Trainer: Product '{_productNameForStock}' not found.", false, DialogWindow.DialogType.Error);
				return;
			}

			int stockValue;
			if (int.TryParse(input, out stockValue))
			{
				// Ensure non-negative before casting to uint
				product.PhysicalCopies = (uint)Math.Max(0, stockValue); 
			}
			else
			{
				// Optional: Keep old value or show error if TryParse fails
				// WindowManager.SpawnDialog($"Invalid stock value: {input}", false, DialogWindow.DialogType.Error);
				// For now, let's keep the old value implicitly by doing nothing on parse failure
			}
			HUD.Instance.AddPopupMessage($"Trainer: Stock for '{product.Name}' set to {product.PhysicalCopies}!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			_productNameForStock = null;
		}

		public static void SetProductStock()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Set Product Stock - Name", "", (productName) =>
			{
				SoftwareProduct product = Settings.MyCompany.Products.FirstOrDefault(p => p.Name.Equals(productName, System.StringComparison.OrdinalIgnoreCase));
				if (product == null)
				{
					WindowManager.SpawnDialog($"Trainer: Product '{productName}' not found.", false, DialogWindow.DialogType.Error);
					return;
				}
				_productNameForStock = productName;
				WindowManager.SpawnInputDialog($"Type new stock for '{productName}':", "Set Product Stock - Amount", product.PhysicalCopies.ToString(), SetProductStockAction);
			});
		}

		#endregion

		#region Add Active Users

		// Helper to store the name temporarily
		private static string _productNameForUsers;
		public static void AddActiveUsersAction(string input)
		{
			SoftwareProduct product =
				Settings.MyCompany.Products.FirstOrDefault(p => p.Name.Equals(_productNameForUsers, System.StringComparison.OrdinalIgnoreCase));

			if (product == null)
			{
				WindowManager.SpawnDialog($"Trainer: Product '{_productNameForUsers}' not found.", false, DialogWindow.DialogType.Error);
				return;
			}

			int userbaseValue;
			if (int.TryParse(input, out userbaseValue))
			{
				// Ensure non-negative and assign directly (assuming Userbase is int)
				product.Userbase = Math.Max(0, userbaseValue); 
			}
			else
			{
				// Optional: Keep old value or show error if TryParse fails
			}
			HUD.Instance.AddPopupMessage($"Trainer: Active users for '{product.Name}' set to {product.Userbase}!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			_productNameForUsers = null;
		}

		public static void AddActiveUsers()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Set Active Users - Name", "", (productName) =>
			{
				SoftwareProduct product = Settings.MyCompany.Products.FirstOrDefault(p => p.Name.Equals(productName, System.StringComparison.OrdinalIgnoreCase));
				if (product == null)
				{
					WindowManager.SpawnDialog($"Trainer: Product '{productName}' not found.", false, DialogWindow.DialogType.Error);
					return;
				}
				_productNameForUsers = productName;
				WindowManager.SpawnInputDialog($"Type new active user count for '{productName}':", "Set Active Users - Amount", product.Userbase.ToString(), AddActiveUsersAction);
			});
		}

		#endregion
	}
}
