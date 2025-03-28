using System;
using System.Collections.Generic;
using System.Linq;
using OrbCreationExtensions; // For ConvertToIntDef
using UnityEngine; // Assuming WindowManager, HUD, etc. are in UnityEngine or related namespaces

namespace Trainer_v5.Actions
{
	/// <summary>
	/// Contains static methods for miscellaneous trainer actions.
	/// </summary>
	public static class MiscActions
	{
		private static GameSettings Settings => GameSettings.Instance;

		public static void ClearLoans()
		{
			Settings.Loans.Clear();
			HUD.Instance.AddPopupMessage("Trainer: All loans are cleared!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void PushReward()
		{
			// Assuming ServerDeal is accessible or defined elsewhere
			var Deals = HUD.Instance.dealWindow.GetActiveDeals().Where(deal => deal is ServerDeal).ToArray();

			if (!Deals.Any())
			{
				return;
			}

			for (int i = 0; i < Deals.Length; i++)
			{
				// Use Helpers.Random if it's accessible or pass Random instance
				Settings.MyCompany.MakeTransaction(Helpers.Random.Next(500, 50000), Company.TransactionCategory.Deals);
			}

			Helpers.RewardIsGained = true;
		}

		public static void PushDeal()
		{
			SoftwareProduct[] Products = Settings.simulation.GetAllProducts(false).Where(pr =>
				  MarketSimulation.Active.SoftwareTypes.ContainsKey(pr.Type.ToString())
				&& pr.Userbase > 0
				&& pr.DevCompany.Name != Settings.MyCompany.Name
				&& pr.ServerReq > 0
				&& !pr.ExternalHostingActive)
					  .ToArray();

			if (Products.Length == 0)
				return;

			int index = Helpers.Random.Next(0, Products.Length);
			// Assuming ServerDeal is accessible
			var dealExist = HUD.Instance.dealWindow.AllDeals.Values.Any(x => x is ServerDeal && ((ServerDeal)x).Product.Name == Products[index].Name);
			if (!dealExist)
			{
				ServerDeal deal = new ServerDeal(Products[index]) { Request = true };
				deal.StillValid(true);
				deal.PerPower *= 2f;

				HUD.Instance.dealWindow.InsertDeal(deal);

				Helpers.DealIsPushed = true;
			}
		}

		public static void UnlockAllSpace()
		{
			if (!Helpers.IsGameLoaded)
			{
				return;
			}

			Example.TakeAllLand(); // Assuming Example class is accessible
			HUD.Instance.AddPopupMessage("Trainer: All plots has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void UnlockFurniture()
		{
			if (!Helpers.IsGameLoaded)
			{
				return;
			}

			Example.UnlockFurniture(); // Assuming Example class is accessible
			Cheats.UnlockFurn = true; // Assuming Cheats class is accessible
			HUD.Instance.UpdateFurnitureButtons();
			HUD.Instance.AddPopupMessage("Trainer: All furniture has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		#region MonthDays

		public static void MonthDaysAction(string input)
		{
			int i;
			if (!int.TryParse(input, out i))
			{
				return;
			}

			GameSettings.DaysPerMonth = i;
			WindowManager.SpawnDialog("You have changed days per month. Please restart the game.", false, DialogWindow.DialogType.Warning);
		}

		public static void MonthDays()
		{
			WindowManager.SpawnInputDialog("How many days per month do you want?", "Days per month", "2", MonthDaysAction);
		}

		#endregion

		#region Extend Deadline

		public static void ExtendDeadline()
		{
			foreach (var work in Settings.MyCompany.WorkItems)
			{
				var contract = work.contract;
				if (contract == null) continue;
				var deadline = contract.Deadline;
				contract.Deadline = new SDateTime(deadline.Year + 1, deadline.Month, deadline.Day);
			}
			HUD.Instance.AddPopupMessage("Trainer: Deadlines extended by 1 year for active contracts!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		#endregion

		#region Increase Money

		public static void IncreaseMoneyAction(string input)
		{
			Settings.MyCompany.MakeTransaction(input.ConvertToIntDef(100000), Company.TransactionCategory.Deals);
			HUD.Instance.AddPopupMessage("Trainer: Money has been added in category Deals!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}
		public static void IncreaseMoney()
		{
			WindowManager.SpawnInputDialog("How much money do you want to add?", "Add Money", "100000", IncreaseMoneyAction);
		}

		#endregion

		#region Add Rep

		public static void MaxReputation()
		{
			Action<string> action = (input) =>
			{
				if (input.Equals("YES", StringComparison.OrdinalIgnoreCase))
				{
					Settings.MyCompany.ChangeBusinessRep(1f, "Publisher", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Deal", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Printing", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Lawsuit", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Contract", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Hosting", 1f);
					WindowManager.SpawnDialog("Trainer: Max reputation is applied to all categories", false, DialogWindow.DialogType.Information);
				}
			};

			// Use the private helper method defined below
			AreYouSureAction("Are you sure you want to max out all business reputations?", action);
		}

		public static void MaxMarketRecognition()
		{
			var softwareTypes = MarketSimulation.Active.SoftwareTypes.Values.Where(value => !value.OneClient).ToList();
			foreach (var softwareType in softwareTypes)
			{
				foreach (var category in softwareType.Categories.ToList())
				{
					Example.AddReputation(softwareType.Name, category.Key, int.MaxValue); // Assuming Example class is accessible
				}
			}

			WindowManager.SpawnDialog("Trainer: Max market recognition is applied to all software types and categories.", false, DialogWindow.DialogType.Information);
		}

		#endregion

		#region UnlockAllRewards

		public static void UnlockAndClaimAllRewards()
		{
			// Assuming GameData is accessible
			GameSettings.Instance.CompletedTasks.AddRange(GameData.Tasks.Select(x => x.Name));
			GameSettings.Instance.ClaimedRewards.AddRange(GameData.Tasks.Select(x => x.Name));
			HUD.Instance.RefreshBuildButtons();

			WindowManager.SpawnDialog("Trainer: All rewards are unlocked and claimed.", false, DialogWindow.DialogType.Information);
		}

		#endregion

		// Private helper for confirmation dialogs
		private static void AreYouSureAction(string question, Action<string> confirmationAction)
		{
			WindowManager.SpawnInputDialog(question, "Confirmation", "YES", confirmationAction);
		}

		#region Experimental/Test

		public static void Test()
		{
			var designDocuments = Settings.MyCompany.WorkItems.OfType<DesignDocument>().Where(d => !d.HasFinished).ToList();

			foreach (var designDocument in designDocuments)
			{
				for (int i = 0; i < DesignDocument.MaxIteration; i++)
				{
					if (designDocument.Parent == null && designDocument.Iteration < 3)
					{
						for (int index = 0; index < designDocument.Features.Length; index++)
						{
							designDocument.Features[index].ArtDone = designDocument.Features[index].CodeDone = false;
							designDocument.Features[index].Progress = 1f;
							designDocument.Features[index].DevTime = 1f;
							designDocument.Features[index].Qual = 1f;
							designDocument.Features[index].LastIterationProg = 1f;
						}
						DevConsole.Console.Log(designDocument.GetProgress()); // Assuming DevConsole is accessible
						designDocument.Iteration++;
					}
				}
			}
			HUD.Instance.AddPopupMessage("Trainer: Test action executed (Design Doc Iteration).", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}


		public static void TestAction(string input)
		{
			WorkItem workItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name.Equals(input, StringComparison.OrdinalIgnoreCase) && !item.InBeta);

			if (workItem == null)
			{
				WindowManager.SpawnDialog($"Trainer: Alpha product '{input}' (not in beta) not found.", false, DialogWindow.DialogType.Error);
				return;
			}

			var softwareAlpha = ((SoftwareAlpha)workItem);
			softwareAlpha.AddQuality(10f, 10f, false);
			HUD.Instance.AddPopupMessage($"Trainer: Test action executed (Add Quality) for '{input}'.", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void TestButton()
		{
			WindowManager.SpawnInputDialog("Type product name in alpha (not beta):", "Add Quality (Test)", "", TestAction);
		}

		#endregion
	}
}
