using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Trainer_v5
{
	internal enum ProductSelection
	{
		None = 0,
		One = 1,
		Several = 2,
		Unsupported = 3
	}

	internal static class ProductsWindowTrainer
	{
		private const int SoftwareProductsMode = 0;

		private static ProductWindow _installedOn;

		public static void Reset()
		{
			_installedOn = null;
		}

		public static void Install()
		{
			var target = HUD.Instance?.PlayerProductWindow;
			if (target == null || target.Window == null || target == _installedOn)
				return;

			var components = new List<Component>
			{
				UIFactory.Button("ProductsSetPriceButton".LocDef("Set Price"), SetPrice),
				UIFactory.Button("ProductsSetStockButton".LocDef("Set Stock"), SetStock),
				UIFactory.Button("ProductsSetUsersButton".LocDef("Set Active Users"), SetActiveUsers),
				UIFactory.Button("ProductsFixBugsButton".LocDef("Fix Product Bugs"), FixProductBugs),
				UIFactory.Button("ProductsMaxAwarenessButton".LocDef("Max Awareness"), MaxAwareness),
			};

			const int width = 110, height = 32, spacing = 4;
			var windowRect = ((RectTransform)target.Window.transform).rect;
			var slots = DetailButtonLayout.Place(components.Count, windowRect.width, width, height, spacing);
			var placed = false;
			try
			{
				for (var i = 0; i < components.Count; i++)
				{
					UIHelper.PlaceInElement(components[i].gameObject, target.Window.gameObject, slots[i]);
				}

				placed = true;
			}
			finally
			{
				if (!placed)
				{
					for (var i = 0; i < components.Count; i++)
					{
						UnityEngine.Object.Destroy(components[i].gameObject);
					}
				}
			}

			if (!placed)
				return;

			_installedOn = target;
		}

		public static ProductSelection Decide(int mode, int selectedRows)
		{
			if (mode != SoftwareProductsMode)
			{
				return ProductSelection.Unsupported;
			}

			if (selectedRows == 1)
			{
				return ProductSelection.One;
			}

			return selectedRows > 1 ? ProductSelection.Several : ProductSelection.None;
		}

		private static void SetPrice()
		{
			var product = RequireOneProduct();
			if (product == null)
				return;

			InputHelper.RequestFloat(
				CurrentValuePrompt(product.Price),
				string.Format("ProductsSetPriceTitle".LocDef("Set price for {0}"), product.Name),
				price =>
				{
					if (IsStillEditable(product))
					{
						ProductFeatures.SetProductPriceAction(product, price);
					}
				},
				0f,
				float.MaxValue);
		}

		private static void SetStock()
		{
			var product = RequireOneProduct();
			if (product == null)
				return;

			InputHelper.RequestUInt(
				CurrentValuePrompt(product.PhysicalCopies),
				string.Format("ProductsSetStockTitle".LocDef("Set stock for {0}"), product.Name),
				"100000",
				stock =>
				{
					if (IsStillEditable(product))
					{
						ProductFeatures.SetProductStockAction(product, stock);
					}
				});
		}

		private static void SetActiveUsers()
		{
			var product = RequireOneProduct();
			if (product == null)
				return;

			InputHelper.RequestInt(
				CurrentValuePrompt(product.Userbase),
				string.Format("ProductsSetUsersTitle".LocDef("Set active users for {0}"), product.Name),
				"100000",
				users =>
				{
					if (IsStillEditable(product))
					{
						ProductFeatures.AddActiveUsersAction(product, users);
					}
				},
				0,
				int.MaxValue);
		}

		private static void FixProductBugs()
		{
			var product = RequireOneProduct();
			if (product == null)
				return;

			var bugs = product.Bugss;
			if (bugs <= 0)
			{
				Notification.ShowInfo(string.Format("ProductNoBugs".LocDef("Trainer: {0} has no bugs to fix."), product.Name));
				return;
			}

			product.FixBugs(bugs);
			Notification.Popup(string.Format("ProductBugsFixed".LocDef("Trainer: {0} bug(s) of {1} have been fixed!"), bugs, product.Name));
		}

		private static void MaxAwareness()
		{
			var product = RequireOneProduct();
			if (product == null)
				return;

			var max = GameSettings.Instance.simulation.GetMaxAwareness(product);
			if (product.GetRealAwareness() >= max)
			{
				Notification.ShowInfo(string.Format("ProductAwarenessAlreadyMax".LocDef("Trainer: Awareness of {0} is already at its maximum."), product.Name));
				return;
			}

			product.SetAwareness(max, false);
			Notification.Popup(string.Format("ProductAwarenessMaxed".LocDef("Trainer: Awareness of {0} is now at its maximum. It fades over time as usual."), product.Name));
		}

		private static SoftwareProduct RequireOneProduct()
		{
			var window = HUD.Instance != null ? HUD.Instance.PlayerProductWindow : null;
			var selection = ProductSelection.None;
			if (window != null && window.Window != null && window.Window.Shown && window.ProductList != null)
			{
				selection = Decide(window.Mode, window.ProductList.GetSelected<object>().Length);
			}

			if (selection == ProductSelection.One)
			{
				var products = window.ProductList.GetSelected<SoftwareProduct>();
				if (products.Length == 1)
				{
					return IsEditable(products[0]) ? products[0] : null;
				}

				selection = ProductSelection.Unsupported;
			}

			Notification.ShowError(selection == ProductSelection.Unsupported
				? "ProductsSoftwareOnly".LocDef("These actions support software products only, not add-ons or frameworks.")
				: "ProductsSelectOne".LocDef("Select exactly one product in your products list for this action."));
			return null;
		}

		private static bool IsEditable(SoftwareProduct product)
		{
			if (!ProductFeatures.IsLiveProduct(product))
			{
				Notification.ShowError("ProductsNotLive".LocDef("This product is archived or no longer available and cannot be edited."));
				return false;
			}

			if (!IsOwnedByPlayer(product))
			{
				Notification.ShowError("ProductsNotOwned".LocDef("Only your own products can be edited here."));
				return false;
			}

			return true;
		}

		private static bool IsStillEditable(SoftwareProduct product)
		{
			if (ProductFeatures.IsLiveProduct(product) && IsOwnedByPlayer(product))
			{
				return true;
			}

			Notification.ShowError(SharedMessages.ProductChangedNothingEdited());
			return false;
		}

		private static bool IsOwnedByPlayer(SoftwareProduct product)
		{
			var settings = GameSettings.Instance;
			return settings != null && settings.MyCompany != null && product.DevCompany == settings.MyCompany;
		}

		private static string CurrentValuePrompt(IFormattable value)
		{
			return string.Format("ProductsCurrentValue".LocDef("Current is {0}"), value.ToString(null, CultureInfo.InvariantCulture));
		}
	}
}
