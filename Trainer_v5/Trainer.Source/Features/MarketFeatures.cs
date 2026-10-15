using System.Linq;

namespace Trainer_v5
{
	internal static class MarketFeatures
	{
		internal static void InstantMarketing()
		{
			var plans = Settings.MyCompany.WorkItems.OfType<MarketingPlan>()
				.Where(plan => plan.Type == MarketingPlan.TaskType.PressRelease)
				.ToList();

			if (plans.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: No active press release campaigns to complete!", false, DialogWindow.DialogType.Information);
				return;
			}

			foreach (MarketingPlan plan in plans)
			{
				CompletePressRelease(plan);
			}

			HUD.Instance.AddPopupMessage("Trainer: Press release campaigns have been completed!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void PushReward()
		{
			var Deals = HUD.Instance.dealWindow.GetActiveDeals().Where(deal => deal.ToString() == "ServerDeal").ToArray();

			if (!Deals.Any())
			{
				return;
			}

			for (int i = 0; i < Deals.Length; i++)
			{
				Settings.MyCompany.MakeTransaction(Helpers.Random.Next(500, 50000), Company.TransactionCategory.Deals);
			}

			Helpers.RewardIsGained = true;
		}

		internal static void PushDeal()
		{
			SoftwareProduct[] Products = Settings.simulation.GetAllProducts(false).Where(pr =>
				  MarketSimulation.Active.SoftwareTypes.ContainsKey(pr.Type.ToString())
				&& pr.Userbase > 0
				&& pr.DevCompany.Name != Settings.MyCompany.Name
				&& pr.ServerReq > 0
				&& !pr.ExternalHostingActive
				&& !pr.DevCompany.IsSubsidiary()
				&& SDateTime.GetMonths(pr.Release, SDateTime.Now()) <= MaxHostedProductAgeMonths)
					  .ToArray();

			if (Products.Length == 0)
				return;

			int index = Helpers.Random.Next(0, Products.Length);
			var dealExist = HUD.Instance.dealWindow.AllDeals.Values.AnyOf<ServerDeal>(x => x.Product.Name == Products[index].Name);
			if (!dealExist)
			{
				// Built like the game's own offers (MarketSimulation): the developer is the client. Setting Request
				// afterwards would read the bidder, which this constructor leaves unset.
				ServerDeal deal = new ServerDeal(Products[index]);
				deal.PerPower *= 2f;

				HUD.Instance.dealWindow.InsertDeal(deal);

				Helpers.DealIsPushed = true;
			}
		}

		internal static void ApplyHostingDealTiming()
		{
			int inGameHour = TimeOfDay.Instance.Hour;

			if ((inGameHour == 9 || inGameHour == 15) && !Helpers.DealIsPushed)
			{
				PushDeal();
			}
			else if (inGameHour != 9 && inGameHour != 15 && Helpers.DealIsPushed)
			{
				Helpers.DealIsPushed = false;
			}

			if (!Helpers.RewardIsGained && inGameHour == 12)
			{
				PushReward();
			}
			else if (inGameHour != 12 && Helpers.RewardIsGained)
			{
				Helpers.RewardIsGained = false;
			}
		}

		// PostMarket spend is settled into LastSpent daily; this refunds it.
		internal static void ApplyFreeMarketing()
		{
			foreach (MarketingPlan plan in Settings.MyCompany.WorkItems.OfType<MarketingPlan>())
			{
				if (plan.Type == MarketingPlan.TaskType.PostMarket && plan.LastSpent > 0f)
				{
					Settings.MyCompany.MakeTransaction(plan.LastSpent, Company.TransactionCategory.Marketing);
				}
			}
		}

		internal static void ApplyDigitalDistributionMonopol()
		{
			foreach (var company in Settings.simulation.Companies.Values.ToList())
			{
				if (company.Bankrupt && company.Distribution != null)
				{
					MarketSimulation.Active.DistributionPlatforms.Remove(company.Distribution);
					HUD.Instance.digitalDistributionWindow.PlatformList.Items.Remove(company.Distribution);
				}

				if (company == Settings.MyCompany || company.Distribution == null || !company.Distribution.Open)
					continue;

				company.Distribution.SetCut(1f);
				company.Distribution.SetAutoAcceptClients(false);
				company.Distribution.AvailableBandwidth = 0f;
				company.Distribution.ItemSales = 0f;
				company.Distribution.ActualItemSales = 0f;
				company.Distribution.LastLoad = 0f;
				company.Distribution.MarketShare = 0f;
				MarketSimulation.Active.ClosePlatform(company.Distribution);
			}
		}

		internal static void ApplyAutoAcceptHostingDeals()
		{
			var serverGroups = Settings.GetAllServerGroups().ToList();
			var serverDeals = HUD.Instance.dealWindow.AllDeals.Values.OfType<ServerDeal>().ToList();

			if (serverGroups.Count > 0 && serverDeals.Count > 0)
			{
				ServerGroup mostPowerfulServerGroup = new ServerGroup("TRAINER") { PowerSum = 0f };
				foreach (var serverGroup in serverGroups)
				{
					if (serverGroup.PowerSum > mostPowerfulServerGroup.PowerSum)
						mostPowerfulServerGroup = serverGroup;
				}

				var dealWindow = HUD.Instance.dealWindow;
				var activeServerDeals = dealWindow.GetActiveDeals().OfType<ServerDeal>().ToList();
				foreach (var serverDeal in serverDeals)
				{
					// AllDeals also keeps cancelled deals until the game's cleanup. One that was accepted once has a
					// timestamp; an invalid one would only be cancelled again by ActuallyAcceptDeal.
					if (activeServerDeals.Contains(serverDeal) || HasBeenAccepted(serverDeal) || !serverDeal.StillValid(false))
					{
						continue;
					}

					dealWindow.ActuallyAcceptDeal(serverDeal, true);
					if (serverDeal.Active && dealWindow.GetActiveDeals().Contains(serverDeal))
					{
						Settings.RegisterWithServer(mostPowerfulServerGroup.Name, serverDeal);
					}
				}
			}
		}

		private static bool HasBeenAccepted(ServerDeal deal)
		{
			return !deal.Accepted.Equals(default(SDateTime));
		}

		// ServerDeal.StillValid rejects a product released more than 24 months ago.
		private const float MaxHostedProductAgeMonths = 24f;

		private static GameSettings Settings => GameSettings.Instance;

		// Only press-release campaigns have a finish state to force.
		private static void CompletePressRelease(MarketingPlan plan)
		{
			MarkAllProgressComplete(plan.Progress);

			plan.StopMarketing();
		}

		// Progress holds one slot per enabled press option (compact), not one per Text/Image/Video bit (#196).
		internal static void MarkAllProgressComplete(float[] progress)
		{
			if (progress == null)
			{
				return;
			}

			for (int i = 0; i < progress.Length; i++)
			{
				progress[i] = 1f;
			}
		}
	}
}
