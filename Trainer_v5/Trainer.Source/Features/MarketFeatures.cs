using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	internal static class MarketFeatures
	{
		private const float MaxHostedProductAgeMonths = 24f;

		private static bool _warnedAboutPendingList;

		private static GameSettings Settings => GameSettings.Instance;

		internal static void InstantMarketing()
		{
			var plans = Settings.MyCompany.WorkItems.OfType<MarketingPlan>()
				.Where(plan => plan.Type == MarketingPlan.TaskType.PressRelease)
				.ToList();

			if (plans.Count == 0)
			{
				Notification.ShowInfo("NoPressReleaseCampaigns".LocDef("Trainer: No active press release campaigns to complete!"));
				return;
			}

			foreach (MarketingPlan plan in plans)
			{
				CompletePressRelease(plan);
			}

			Notification.Popup("PressReleaseCampaignsCompleted".LocDef("Trainer: Press release campaigns have been completed!"));
		}

		internal static void PushReward()
		{
			var Deals = HUD.Instance.dealWindow.GetActiveDeals().OfType<ServerDeal>().ToArray();

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
				ServerDeal deal = new ServerDeal(Products[index]);
				deal.PerPower *= 2f;

				HUD.Instance.dealWindow.InsertDeal(deal);

				Helpers.DealIsPushed = true;
			}
		}

		internal static int RepairLegacyHostingRequests()
		{
			var dealWindow = HUD.Instance.dealWindow;
			var repaired = 0;
			if (dealWindow == null)
			{
				return repaired;
			}

			foreach (var deal in dealWindow.AllDeals.Values.OfType<ServerDeal>().ToList())
			{
				var product = deal.Product;
				var client = deal.Client;
				if (product == null || client == null)
				{
					continue;
				}

				if (IsLegacyRequestDeal(deal.Request, deal.Bidder != null, client == product.DevCompany, client == Settings.MyCompany))
				{
					deal.Request = false;
					repaired++;
				}
			}

			return repaired;
		}

		internal static bool IsLegacyRequestDeal(bool request, bool hasBidder, bool clientIsDeveloper, bool clientIsPlayer)
		{
			return request && !hasBidder && clientIsDeveloper && !clientIsPlayer;
		}

		internal static void ApplyHostingDealTiming()
		{
			ApplyHostingDealCatchUp();

			int inGameHour = TimeOfDay.Instance.Hour;
			if (!Helpers.RewardIsGained && inGameHour == 12)
			{
				PushReward();
			}
			else if (inGameHour != 12 && Helpers.RewardIsGained)
			{
				Helpers.RewardIsGained = false;
			}
		}

		internal static void ApplyHostingDealCatchUp()
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
		}

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
			var serverGroups = Settings.GetAllServerGroups().Where(group => Settings.GetServerGroup(group.Name) != null).ToList();
			var strongest = FindStrongestIndex(serverGroups.Select(group => group.PowerSum).ToList());
			if (strongest < 0)
			{
				return;
			}

			var targetGroup = serverGroups[strongest];

			var dealWindow = HUD.Instance.dealWindow;
			var pending = FindPendingDeals(dealWindow);
			if (pending == null)
			{
				WarnOnce("Auto Accept Hosting Deals: the pending deal list could not be identified, so no deal was accepted.");
				return;
			}

			foreach (var serverDeal in pending.OfType<ServerDeal>().ToList())
			{
				if (!serverDeal.StillValid(false))
				{
					continue;
				}

				dealWindow.ActuallyAcceptDeal(serverDeal, true);
				if (serverDeal.Active && dealWindow.GetActiveDeals().Contains(serverDeal))
				{
					Settings.RegisterWithServer(targetGroup.Name, serverDeal);
				}
			}
		}

		internal static int FindStrongestIndex(IList<float> powers)
		{
			var strongest = -1;
			for (var i = 0; i < powers.Count; i++)
			{
				if (strongest < 0 || powers[i] > powers[strongest])
				{
					strongest = i;
				}
			}

			return strongest;
		}

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

		private static List<Deal> FindPendingDeals(DealWindow dealWindow)
		{
			var lists = new List<List<object>>();
			foreach (var view in dealWindow.Window.GetComponentsInChildren<GUIListView>(true))
			{
				lists.Add(new List<object>(view.Items));
			}

			return PendingDeals.Select(lists, dealWindow.GetActiveDeals());
		}

		internal static void ResetGameState()
		{
			Helpers.DealIsPushed = false;
			Helpers.RewardIsGained = false;
			_warnedAboutPendingList = false;
		}

		private static void WarnOnce(string message)
		{
			if (_warnedAboutPendingList)
			{
				return;
			}

			_warnedAboutPendingList = true;
			message.Log();
		}

		private static void CompletePressRelease(MarketingPlan plan)
		{
			MarkAllProgressComplete(plan.Progress);

			plan.StopMarketing();
		}
	}
}
