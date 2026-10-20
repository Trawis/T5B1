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
				WindowManager.SpawnDialog("NoOffshoreFunds".LocDef("Trainer: No offshore funds to transfer!"), false, DialogWindow.DialogType.Information);
				return;
			}

			// The game has no transfer method; it adjusts this field directly.
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			Settings.OffshoreAccount = 0.0;
			HUD.Instance.AddPopupMessage("OffshoreFundsTransferred".LocDef("Trainer: Offshore funds transferred!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void ClearLoans()
		{
			Settings.Loans.Clear();
			HUD.Instance.AddPopupMessage("AllLoansCleared".LocDef("Trainer: All loans are cleared!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void AIBankrupt()
		{
			SimulatedCompany[] Companies = Settings.simulation.Companies.Values.Where(IsBankruptTarget).ToArray();

			for (int i = 0; i < Companies.Length; i++)
			{
				Companies[i].Bankrupt = true;
			}
		}

		internal static void UnlockAllSpace()
		{
			if (!Helpers.IsGameReady())
				return;

			Example.TakeAllLand();
			HUD.Instance.AddPopupMessage("AllPlotsUnlocked".LocDef("Trainer: All plots has been unlocked!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void UnlockFurniture()
		{
			if (!Helpers.IsGameReady())
				return;

			Example.UnlockFurniture();
			Cheats.UnlockFurn = true;
			HUD.Instance.UpdateFurnitureButtons();
			HUD.Instance.AddPopupMessage("AllFurnitureUnlocked".LocDef("Trainer: All furniture has been unlocked!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		// Read directly by CompanyDetailWindow.TakeOverSub, so it is bound when the setting changes rather than polled.
		internal static void ApplyUnlimitedSubsidiaries(bool enabled)
		{
			Cheats.InfiniteSubs = enabled;
		}

		internal static void MonthDaysAction(int i)
		{
			GameSettings.DaysPerMonth = i;
			WindowManager.SpawnDialog("DaysPerMonthRestart".LocDef("You have changed days per month. Please restart the game."), false, DialogWindow.DialogType.Warning);
		}

		internal static void MonthDays()
		{
			InputHelper.RequestInt("How many days per month do you want?", "Days per month", "2", MonthDaysAction, 1, 31);
		}

		internal static void TakeoverCompanyAction(string input)
		{
			var simulatedCompany = Settings.simulation.Companies
				.FirstOrDefault(simCompany => simCompany.Value.Name == input).Value;

			if (simulatedCompany == null)
			{
				WindowManager.SpawnDialog("Trainer: Company " + input + " not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (!simulatedCompany.CanBuyOut(Settings.MyCompany))
			{
				WindowManager.SpawnDialog("CompanyCannotBeBought".LocDef("Trainer: Company can't be bought!"), false, DialogWindow.DialogType.Information);
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
			WindowManager.SpawnDialog("Trainer: Company " + input + " has been takovered by you!", false, DialogWindow.DialogType.Information);
		}

		internal static void TakeoverCompany()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Takeover Company", "", TakeoverCompanyAction);
		}

		internal static void SubDCompanyAction(string input)
		{
			SimulatedCompany Company =
				Settings.simulation.Companies.FirstOrDefault(company => company.Value.Name == input).Value;

			if (Company == null)
			{
				WindowManager.SpawnDialog("Trainer: Company " + input + " not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			// The player's own company is in the simulation too; making it a subsidiary of itself is invalid.
			if (!IsBankruptTarget(Company))
			{
				WindowManager.SpawnDialog("Trainer: " + Company.Name + " already belongs to you.", false, DialogWindow.DialogType.Information);
				return;
			}

			Company.MakeSubsidiary(Settings.MyCompany, SDateTime.Now());
			HUD.Instance.AddPopupMessage("Trainer: Company " + Company.Name + " is now your subsidiary!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void SubDCompany()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Subsidiary Company", "", SubDCompanyAction);
		}

		internal static void ForceBankruptAction(string input)
		{
			List<SimulatedCompany> matches = Settings.simulation.Companies.Values
				.Where(simCompany => simCompany.Name == input && IsBankruptTarget(simCompany))
				.ToList();

			if (matches.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: Company " + input + " not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (matches.Count > 1)
			{
				WindowManager.SpawnDialog("Trainer: Company name " + input + " is ambiguous. Rename the company or use a unique name.", false, DialogWindow.DialogType.Information);
				return;
			}

			SimulatedCompany company = matches[0];
			if (company.Bankrupt)
			{
				WindowManager.SpawnDialog("Trainer: " + company.Name + " is already going bankrupt.", false, DialogWindow.DialogType.Information);
				return;
			}

			// One-way: the flag is only ever set, never toggled back.
			WindowManager.SpawnInputDialog("Bankrupt " + company.Name + "? Type YES to confirm.", "Confirmation", "YES", answer => ConfirmForceBankrupt(company, answer));
		}

		internal static void ForceBankrupt()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Force Bankrupt", "", ForceBankruptAction);
		}

		internal static void IncreaseMoneyAction(int amount)
		{
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			HUD.Instance.AddPopupMessage("MoneyAddedDeals".LocDef("Trainer: Money has been added in category Deals!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void IncreaseMoney()
		{
			InputHelper.RequestInt("How much money do you want to add?", "Add Money", "100000", IncreaseMoneyAction);
		}

		internal static void AddAIFundsAction(string input)
		{
			List<SimulatedCompany> matches = Settings.simulation.Companies.Values
				.Where(simCompany => simCompany.Name == input && IsBankruptTarget(simCompany))
				.ToList();

			if (matches.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: Company " + input + " not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (matches.Count > 1)
			{
				WindowManager.SpawnDialog("Trainer: Company name " + input + " is ambiguous. Rename the company or use a unique name.", false, DialogWindow.DialogType.Information);
				return;
			}

			SimulatedCompany company = matches[0];
			InputHelper.RequestInt("How much money do you want to add to " + company.Name + "?", "Add AI Funds", "100000",
				amount =>
				{
					company.MakeTransaction(amount, Company.TransactionCategory.Deals);
					HUD.Instance.AddPopupMessage("Trainer: Money has been added to " + company.Name + "!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
				}, 0, int.MaxValue);
		}

		internal static void AddAIFunds()
		{
			WindowManager.SpawnInputDialog("Type AI company name:", "Add AI Funds", "", AddAIFundsAction);
		}

		internal static void MaxReputation()
		{
			Action<string> action = (input) =>
			{
				if (input == "YES")
				{
					Settings.MyCompany.ChangeBusinessRep(1f, "Publisher", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Deal", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Printing", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Lawsuit", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Contract", 1f);
					Settings.MyCompany.ChangeBusinessRep(1f, "Hosting", 1f);
					WindowManager.SpawnDialog("MaxReputationApplied".LocDef("Trainer: Max reputation is applied to all categories"), false, DialogWindow.DialogType.Information);
				}
			};

			Func<InputDialog> areYouSure = () =>
			{
				return WindowManager.SpawnInputDialog("Are you sure?", "Confirmation", "YES", action);
			};

			areYouSure();
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

			WindowManager.SpawnDialog("MaxMarketRecognitionApplied".LocDef("Trainer: Max market recognition is applied to all software types and categories."), false, DialogWindow.DialogType.Information);
		}

		internal static void UnlockAndClaimAllRewards()
		{
			GameSettings.Instance.CompletedTasks.AddRange(GameData.Tasks.Select(x => x.Name));
			GameSettings.Instance.ClaimedRewards.AddRange(GameData.Tasks.Select(x => x.Name));
			HUD.Instance.RefreshBuildButtons();
			HUD.Instance.UpdateFurnitureButtons();

			WindowManager.SpawnDialog("RewardsUnlockedClaimed".LocDef("Trainer: All rewards are unlocked and claimed."), false, DialogWindow.DialogType.Information);
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

		internal static void RestoreReduceExpansionCost()
		{
			Settings.ExpansionCost = Constants.EXPANSION_COST;
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

		internal static void ApplyReduceExpansionCost()
		{
			Settings.ExpansionCost = Constants.EXPANSION_COST_HALF;
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

		// The dialog stays open while the game runs, so the company is checked again before the flag is set.
		private static void ConfirmForceBankrupt(SimulatedCompany company, string answer)
		{
			SimulatedCompany current;
			bool stillThere = Settings.simulation.Companies.TryGetValue(company.ID, out current) && current == company;
			if (answer == "YES" && stillThere && IsBankruptTarget(company) && !company.Bankrupt)
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
