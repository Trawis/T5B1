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

		// Presets are shared for the whole game process, so every preset the trainer has halved is restored, not just the current one.
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

		// GameSettings.PaybackLoan charges Monthly (principal + MonthlyInterest) per loan, then removes paid-off loans, before OnMonthPassed fires.
		// Refunding the still-outstanding loans' MonthlyInterest here needs no captured baseline: Loan fields are never modified after creation.
		internal static void ApplyNoLoanInterest()
		{
			float totalInterest = Settings.Loans.Sum(loan => loan.MonthlyInterest);
			if (totalInterest > 0f)
			{
				Settings.MyCompany.MakeTransaction(totalInterest, Company.TransactionCategory.Interest);
			}
		}

		// PayDividends (paid monthly, despite being called from something named EndDay) records each payout in NewStock[i].Payout.
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

		public static void TransferOffshoreFunds()
		{
			double amount = Settings.OffshoreAccount;
			if (amount <= 0.0)
			{
				WindowManager.SpawnDialog("Trainer: No offshore funds to transfer!", false, DialogWindow.DialogType.Information);
				return;
			}

			// No dedicated transfer method exists; mirrors how the game's own funnel flow touches this field directly.
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			Settings.OffshoreAccount = 0.0;
			HUD.Instance.AddPopupMessage("Trainer: Offshore funds transferred!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
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

		// Refunds the daily insurance bill TimeOfDay.UpdateDay already charged this tick, leaving CurrentRate/coverage untouched.
		internal static void ApplyNoInsuranceCost()
		{
			if (!Settings.PassedFireInspection)
			{
				return;
			}

			float dailyBill = Settings.Insurance.GetContentBill(true) / GameSettings.DaysPerMonth;
			Settings.MyCompany.MakeTransaction(dailyBill, Company.TransactionCategory.Bills);
		}

		public static void ClearLoans()
		{
			Settings.Loans.Clear();
			HUD.Instance.AddPopupMessage("Trainer: All loans are cleared!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void AIBankrupt()
		{
			SimulatedCompany[] Companies = Settings.simulation.Companies.Values.ToArray();

			for (int i = 0; i < Companies.Length; i++)
			{
				Companies[i].Bankrupt = true;
			}
		}

		public static void UnlockAllSpace()
		{
			if (!Helpers.IsGameReady()) return;

			Example.TakeAllLand();
			HUD.Instance.AddPopupMessage("Trainer: All plots has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void UnlockFurniture()
		{
			if (!Helpers.IsGameReady()) return;

			Example.UnlockFurniture();
			Cheats.UnlockFurn = true;
			HUD.Instance.UpdateFurnitureButtons();
			HUD.Instance.AddPopupMessage("Trainer: All furniture has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		// Cheats.InfiniteSubs is read directly by CompanyDetailWindow.TakeOverSub's subsidiary-count gate, so it is bound like the room flags in the environment domain.
		public static void ApplyUnlimitedSubsidiaries(bool enabled)
		{
			Cheats.InfiniteSubs = enabled;
		}

		public static void MonthDaysAction(int i)
		{
			GameSettings.DaysPerMonth = i;
			WindowManager.SpawnDialog("You have changed days per month. Please restart the game.", false, DialogWindow.DialogType.Warning);
		}

		public static void MonthDays()
		{
			InputHelper.RequestInt("How many days per month do you want?", "Days per month", "2", MonthDaysAction, 1, 31);
		}

		public static void TakeoverCompanyAction(string input)
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
				WindowManager.SpawnDialog("Trainer: Company can't be bought!", false, DialogWindow.DialogType.Information);
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

		public static void TakeoverCompany()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Takeover Company", "", TakeoverCompanyAction);
		}

		public static void SubDCompanyAction(string input)
		{
			SimulatedCompany Company =
				Settings.simulation.Companies.FirstOrDefault(company => company.Value.Name == input).Value;

			if (Company == null)
			{
				return;
			}

			Company.MakeSubsidiary(Settings.MyCompany, SDateTime.Now());
			HUD.Instance.AddPopupMessage("Trainer: Company " + Company.Name + " is now your subsidiary!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void SubDCompany()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Subsidiary Company", "", SubDCompanyAction);
		}

		public static void ForceBankruptAction(string input)
		{
			SimulatedCompany Company =
				Settings.simulation.Companies.FirstOrDefault(company => company.Value.Name == input).Value;

			DevConsole.Console.Log("input => " + input + " Company: " + Company);

			if (Company == null)
			{
				return;
			}

			Company.Bankrupt = !Company.Bankrupt;
		}

		public static void ForceBankrupt()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Force Bankrupt", "", ForceBankruptAction);
		}

		public static void IncreaseMoneyAction(int amount)
		{
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			HUD.Instance.AddPopupMessage("Trainer: Money has been added in category Deals!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void IncreaseMoney()
		{
			InputHelper.RequestInt("How much money do you want to add?", "Add Money", "100000", IncreaseMoneyAction);
		}

		public static void AddAIFundsAction(string input)
		{
			List<SimulatedCompany> matches = Settings.simulation.Companies.Values
				.Where(simCompany => simCompany.Name == input && simCompany != Settings.MyCompany)
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

		public static void AddAIFunds()
		{
			WindowManager.SpawnInputDialog("Type AI company name:", "Add AI Funds", "", AddAIFundsAction);
		}

		public static void MaxReputation()
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
					WindowManager.SpawnDialog("Trainer: Max reputation is applied to all categories", false, DialogWindow.DialogType.Information);
				}
			};

			Func<InputDialog> areYouSure = () =>
			{
				return WindowManager.SpawnInputDialog("Are you sure?", "Confirmation", "YES", action);
			};

			areYouSure();
		}

		public static void MaxMarketRecognition()
		{
			var softwareTypes = MarketSimulation.Active.SoftwareTypes.Values.Where(value => !value.OneClient).ToList();
			foreach (var softwareType in softwareTypes)
			{
				foreach (var category in softwareType.Categories.ToList())
				{
					Example.AddReputation(softwareType.Name, category.Key, int.MaxValue);
				}
			}

			WindowManager.SpawnDialog("Trainer: Max market recognition is applied to all software types and categories.", false, DialogWindow.DialogType.Information);
		}

		public static void UnlockAndClaimAllRewards()
		{
			GameSettings.Instance.CompletedTasks.AddRange(GameData.Tasks.Select(x => x.Name));
			GameSettings.Instance.ClaimedRewards.AddRange(GameData.Tasks.Select(x => x.Name));
			HUD.Instance.RefreshBuildButtons();

			WindowManager.SpawnDialog("Trainer: All rewards are unlocked and claimed.", false, DialogWindow.DialogType.Information);
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
	}
}
