using System;
using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	internal static class CompanyFeatures
	{
		private static GameSettings Settings => GameSettings.Instance;
		private static readonly Dictionary<EnvironmentPreset, float> _ispCostBaselines = new Dictionary<EnvironmentPreset, float>();
		private static float[] _defaultEdCost;

		internal static void TransferOffshoreFunds()
		{
			double amount = Settings.OffshoreAccount;
			if (amount <= 0.0)
			{
				Notification.ShowInfo("NoOffshoreFunds".LocDef("Trainer: No offshore funds to transfer!"));
				return;
			}

			// The game has no transfer method; it adjusts this field directly.
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			Settings.OffshoreAccount = 0.0;
			Notification.Popup("OffshoreFundsTransferred".LocDef("Trainer: Offshore funds transferred!"));
		}

		internal static void ClearLoans()
		{
			if (!Helpers.RequireGameReady("Clear all loans"))
				return;

			int count = Settings.Loans.Count;
			if (count == 0)
			{
				Notification.ShowInfo("NoLoansToClear".LocDef("There are no loans to clear."));
				return;
			}

			GameSettings game = Settings;
			Confirmation.Ask("Clear all loans", string.Format("ClearLoansConfirm".LocDef("Remove all {0} loan(s) without repaying them?\nNo payment is made, the remaining payments are cancelled and this cannot be undone."), count), () => ApplyClearLoans(game, count));
		}

		internal static void AIBankrupt()
		{
			if (!Helpers.RequireGameReady("AI Bankrupt All"))
				return;

			int count = FindAllBankruptTargets().Count;
			if (count == 0)
			{
				Notification.ShowInfo("NoAICompaniesToBankrupt".LocDef("There is no AI company left to bankrupt."));
				return;
			}

			GameSettings game = Settings;
			Confirmation.Ask("AI Bankrupt All", string.Format("BankruptAllConfirm".LocDef("Bankrupt all {0} AI company(ies)?\nThe game cancels their deals and removes them from the market. This cannot be undone.\nYour own company and its subsidiaries are not affected."), count), () => ApplyAIBankrupt(game, count));
		}

		internal static void UnlockAllSpace()
		{
			if (!Helpers.IsGameReady())
				return;

			Example.TakeAllLand();
			Notification.Popup("AllPlotsUnlocked".LocDef("Trainer: All plots has been unlocked!"));
		}

		internal static void UnlockFurniture()
		{
			if (!Helpers.IsGameReady())
				return;

			Example.UnlockFurniture();
			Cheats.UnlockFurn = true;
			HUD.Instance.UpdateFurnitureButtons();
			Notification.Popup("AllFurnitureUnlocked".LocDef("Trainer: All furniture has been unlocked!"));
		}

		// Read directly by CompanyDetailWindow.TakeOverSub, so it is bound when the setting changes rather than polled.
		internal static void ApplyUnlimitedSubsidiaries(bool enabled)
		{
			Cheats.InfiniteSubs = enabled;
		}

		internal static void MonthDaysAction(int i)
		{
			GameSettings.DaysPerMonth = i;
			Notification.ShowWarning("DaysPerMonthRestart".LocDef("You have changed days per month. Please restart the game."));
		}

		internal static void MonthDays()
		{
			InputHelper.RequestInt("DaysPerMonthPrompt".LocDef("How many days per month do you want?"), "DaysPerMonthTitle".LocDef("Days per month"), "2", MonthDaysAction, 1, 31);
		}

		internal static void TakeoverCompanyAction(string input)
		{
			var simulatedCompany = Settings.simulation.Companies
				.FirstOrDefault(simCompany => simCompany.Value.Name == input).Value;

			if (simulatedCompany == null)
			{
				Notification.ShowError(SharedMessages.CompanyNotFound(input));
				return;
			}

			if (!simulatedCompany.CanBuyOut(Settings.MyCompany))
			{
				Notification.ShowError("CompanyCannotBeBought".LocDef("Trainer: Company can't be bought!"));
				return;
			}

			var simulatedCompanyWorth = simulatedCompany.GetPossibleStockWorth();

			simulatedCompany.BuyOut(
				new Company[] { Settings.MyCompany }, // companies buying out
				false,                                // not broke
				SDateTime.Now(),                      // current time
				true                                  // can disconnect (default value)
			);

			Settings.MyCompany.MakeTransaction(simulatedCompanyWorth, Company.TransactionCategory.Stocks, (string)null, false);
			Notification.ShowInfo(string.Format("CompanyTakenOver".LocDef("Trainer: Company {0} has been takovered by you!"), input));
		}

		internal static void TakeoverCompany()
		{
			WindowManager.SpawnInputDialog(SharedMessages.CompanyNamePrompt(), "TakeoverCompanyTitle".LocDef("Takeover Company"), "", TakeoverCompanyAction);
		}

		internal static void SubDCompanyAction(string input)
		{
			SimulatedCompany Company =
				Settings.simulation.Companies.FirstOrDefault(company => company.Value.Name == input).Value;

			if (Company == null)
			{
				Notification.ShowError(SharedMessages.CompanyNotFound(input));
				return;
			}

			// The player's own company is in the simulation too; making it a subsidiary of itself is invalid.
			if (!IsBankruptTarget(Company))
			{
				Notification.ShowInfo(string.Format("CompanyAlreadyYours".LocDef("Trainer: {0} already belongs to you."), Company.Name));
				return;
			}

			Company.MakeSubsidiary(Settings.MyCompany, SDateTime.Now());
			Notification.Popup(string.Format("CompanySubsidiary".LocDef("Trainer: Company {0} is now your subsidiary!"), Company.Name));
		}

		internal static void SubDCompany()
		{
			WindowManager.SpawnInputDialog(SharedMessages.CompanyNamePrompt(), "SubsidiaryCompanyTitle".LocDef("Subsidiary Company"), "", SubDCompanyAction);
		}

		internal static void ForceBankruptAction(string input)
		{
			List<SimulatedCompany> matches = Settings.simulation.Companies.Values
				.Where(simCompany => simCompany.Name == input && IsBankruptTarget(simCompany))
				.ToList();

			if (matches.Count == 0)
			{
				Notification.ShowError(SharedMessages.CompanyNotFound(input));
				return;
			}

			if (matches.Count > 1)
			{
				Notification.ShowError(SharedMessages.CompanyNameAmbiguous(input));
				return;
			}

			SimulatedCompany company = matches[0];
			if (company.Bankrupt)
			{
				Notification.ShowInfo(string.Format("CompanyAlreadyBankrupt".LocDef("Trainer: {0} is already going bankrupt."), company.Name));
				return;
			}

			// One-way: the flag is only ever set, never toggled back.
			Confirmation.Ask("Bankrupt", string.Format("BankruptConfirmPrompt".LocDef("Bankrupt {0}?\nThe game cancels its deals and removes it from the market. This cannot be undone."), company.Name), () => ConfirmForceBankrupt(company));
		}

		internal static void ForceBankrupt()
		{
			WindowManager.SpawnInputDialog(SharedMessages.CompanyNamePrompt(), "ForceBankruptTitle".LocDef("Force Bankrupt"), "", ForceBankruptAction);
		}

		internal static void IncreaseMoneyAction(int amount)
		{
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			Notification.Popup("MoneyAddedDeals".LocDef("Trainer: Money has been added in category Deals!"));
		}

		internal static void IncreaseMoney()
		{
			InputHelper.RequestInt("AddMoneyPrompt".LocDef("How much money do you want to add?"), "AddMoneyTitle".LocDef("Add Money"), "100000", IncreaseMoneyAction);
		}

		internal static void AddAIFundsAction(string input)
		{
			List<SimulatedCompany> matches = Settings.simulation.Companies.Values
				.Where(simCompany => simCompany.Name == input && IsBankruptTarget(simCompany))
				.ToList();

			if (matches.Count == 0)
			{
				Notification.ShowError(SharedMessages.CompanyNotFound(input));
				return;
			}

			if (matches.Count > 1)
			{
				Notification.ShowError(SharedMessages.CompanyNameAmbiguous(input));
				return;
			}

			SimulatedCompany company = matches[0];
			InputHelper.RequestInt(string.Format("AddAIFundsPrompt".LocDef("How much money do you want to add to {0}?"), company.Name), SharedMessages.AddAIFundsTitle(), "100000",
				amount =>
				{
					company.MakeTransaction(amount, Company.TransactionCategory.Deals);
					Notification.Popup(string.Format("AIFundsAdded".LocDef("Trainer: Money has been added to {0}!"), company.Name));
				}, 0, int.MaxValue);
		}

		internal static void AddAIFunds()
		{
			WindowManager.SpawnInputDialog("AICompanyNamePrompt".LocDef("Type AI company name:"), SharedMessages.AddAIFundsTitle(), "", AddAIFundsAction);
		}

		internal static void MaxReputation()
		{
			Confirmation.Ask("Max Reputation", "MaxReputationConfirm".LocDef("Set your business reputation to the maximum in all categories?"), ApplyMaxReputation);
		}

		internal static void MaxMarketRecognition()
		{
			var softwareTypes = MarketSimulation.Active.SoftwareTypes.Values.Where(value => !value.OneClient).ToList();
			foreach (var softwareType in softwareTypes)
			{
				foreach (var category in softwareType.Categories.ToList())
				{
					Example.AddReputation(softwareType.Name, category.Key, int.MaxValue);
				}
			}

			Notification.ShowInfo("MaxMarketRecognitionApplied".LocDef("Trainer: Max market recognition is applied to all software types and categories."));
		}

		internal static void UnlockAndClaimAllRewards()
		{
			if (!Helpers.RequireGameReady("Unlock and Claim Rewards"))
				return;

			int count = CountUnclaimedRewards();
			if (count == 0)
			{
				Notification.ShowInfo("NoRewardsToUnlock".LocDef("Every reward is already unlocked and claimed."));
				return;
			}

			GameSettings game = Settings;
			Confirmation.Ask("Unlock and Claim Rewards", string.Format("UnlockRewardsConfirm".LocDef("Mark {0} reward task(s) as completed and claimed in this save?\nThe game has no way to undo this."), count), () => ApplyUnlockAndClaimAllRewards(game, count));
		}

		// Presets are shared for the whole process, so every halved preset is restored, not only the current one.
		internal static void RestoreReduceISPCost()
		{
			foreach (KeyValuePair<EnvironmentPreset, float> baseline in _ispCostBaselines)
			{
				baseline.Key.ISPCostFactor = baseline.Value;
			}
		}

		internal static void RestoreIncreaseCourierCapacity()
		{
			AI.MaxBoxes = Constants.MAX_BOXES_DEFAULT;
			AI.MaxBoxCarry = Constants.MAX_CARRY_DEFAULT;
		}

		// Paid-off loans are already removed when this runs; only outstanding loans' interest is refunded.
		internal static void ApplyNoLoanInterest()
		{
			float totalInterest = Settings.Loans.Sum(loan => loan.MonthlyInterest);
			if (totalInterest > 0f)
			{
				Settings.MyCompany.MakeTransaction(totalInterest, Company.TransactionCategory.Interest);
			}
		}

		// PayDividends records each payout in NewStock[i].Payout.
		internal static void ApplyNoFounderDividends()
		{
			foreach (NewStock stock in Settings.MyCompany.NewStock)
			{
				if (stock.Buyer is FounderShareHolder && stock.Payout > 0f)
				{
					Settings.MyCompany.MakeTransaction(stock.Payout, Company.TransactionCategory.Dividends);
				}
			}
		}

		internal static void ApplyNoEducationCost()
		{
			if (_defaultEdCost == null)
			{
				_defaultEdCost = (float[])EducationWindow.EdCost.Clone();
			}
			EducationWindow.EdCost = new[] { 0f, 0f, 0f };
		}

		internal static void RestoreEducationCost()
		{
			if (_defaultEdCost != null)
			{
				EducationWindow.EdCost = (float[])_defaultEdCost.Clone();
			}
		}

		// Refunds the daily insurance bill already charged this tick.
		internal static void ApplyNoInsuranceCost()
		{
			if (!Settings.PassedFireInspection)
			{
				return;
			}

			float dailyBill = Settings.Insurance.GetContentBill(true) / GameSettings.DaysPerMonth;
			Settings.MyCompany.MakeTransaction(dailyBill, Company.TransactionCategory.Bills);
		}

		internal static void ApplyFreeStaff()
		{
			Settings.StaffSalaryDue = 0f;
		}

		internal static void ApplyNoServerCost()
		{
			Settings.ServerCost = 0f;
		}

		internal static void ApplyNoWaterElectricityBills()
		{
			Settings.ElectricityBill = 0f;
			Settings.Waterbill = 0f;
			Settings.Gasbill = 0f;
		}

		internal static void ApplyIncreaseCourierCapacity()
		{
			AI.MaxBoxes = Constants.MAX_BOXES_BOOSTED;
			AI.MaxBoxCarry = Constants.MAX_CARRY_BOOSTED;
		}

		internal static void ApplyReduceISPCost()
		{
			Settings.Environment.ISPCostFactor = GetDefaultISPCostFactor() / 2f;
		}

		internal static void ApplyDisableForcePause()
		{
			GameSettings.ForcePause = false;
		}

		internal static void ApplyDisableForceFreeze()
		{
			GameSettings.FreezeGame = false;
		}

		internal static void ApplyNoOffshoreHeat()
		{
			Settings.Heat = 0f;
		}

		// The player's own company and its subsidiaries are never bankrupted.
		private static bool IsBankruptTarget(SimulatedCompany company)
		{
			return company != Settings.MyCompany && !company.IsPlayerOwned();
		}

		private static void ApplyMaxReputation()
		{
			Settings.MyCompany.ChangeBusinessRep(1f, "Publisher", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Deal", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Printing", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Lawsuit", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Contract", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Hosting", 1f);
			Notification.ShowInfo("MaxReputationApplied".LocDef("Trainer: Max reputation is applied to all categories"));
		}

		private static List<SimulatedCompany> FindAllBankruptTargets()
		{
			return Settings.simulation.Companies.Values.Where(company => IsBankruptTarget(company) && !company.Bankrupt).ToList();
		}

		// The dialog stays open while the game runs, so game and targets are re-checked first.
		private static void ApplyAIBankrupt(GameSettings game, int shownCount)
		{
			if (!ReferenceEquals(game, Settings))
			{
				Confirmation.ShowScopeChanged();
				return;
			}

			List<SimulatedCompany> targets = FindAllBankruptTargets();
			if (!Confirmation.IsScopeUnchanged(shownCount, targets.Count))
			{
				Confirmation.ShowScopeChanged();
				return;
			}

			for (int i = 0; i < targets.Count; i++)
			{
				targets[i].Bankrupt = true;
			}
		}

		private static void ApplyClearLoans(GameSettings game, int shownCount)
		{
			if (!ReferenceEquals(game, Settings) || !Confirmation.IsScopeUnchanged(shownCount, Settings.Loans.Count))
			{
				Confirmation.ShowScopeChanged();
				return;
			}

			Settings.Loans.Clear();
			Notification.Popup("AllLoansCleared".LocDef("Trainer: All loans are cleared!"));
		}

		private static int CountUnclaimedRewards()
		{
			return GameData.Tasks
				.Where(task => !Settings.CompletedTasks.Contains(task.Name) || !Settings.ClaimedRewards.Contains(task.Name))
				.Select(task => task.Name)
				.Distinct()
				.Count();
		}

		private static void ApplyUnlockAndClaimAllRewards(GameSettings game, int shownCount)
		{
			if (!ReferenceEquals(game, Settings) || !Confirmation.IsScopeUnchanged(shownCount, CountUnclaimedRewards()))
			{
				Confirmation.ShowScopeChanged();
				return;
			}

			Settings.CompletedTasks.AddRange(GameData.Tasks.Select(x => x.Name));
			Settings.ClaimedRewards.AddRange(GameData.Tasks.Select(x => x.Name));
			HUD.Instance.RefreshBuildButtons();
			HUD.Instance.UpdateFurnitureButtons();

			Notification.ShowInfo("RewardsUnlockedClaimed".LocDef("Trainer: All rewards are unlocked and claimed."));
		}

		// The dialog stays open while the game runs, so the company is checked again before the flag is set.
		private static void ConfirmForceBankrupt(SimulatedCompany company)
		{
			SimulatedCompany current;
			bool stillThere = Settings.simulation.Companies.TryGetValue(company.ID, out current) && current == company;
			if (stillThere && IsBankruptTarget(company) && !company.Bankrupt)
			{
				company.Bankrupt = true;
			}
		}

		private static float GetDefaultISPCostFactor()
		{
			EnvironmentPreset environment = Settings.Environment;
			float baseline;
			if (!_ispCostBaselines.TryGetValue(environment, out baseline))
			{
				baseline = environment.ISPCostFactor;
				_ispCostBaselines[environment] = baseline;
			}

			return baseline;
		}
	}
}
