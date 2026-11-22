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

			List<Loan> loans = new List<Loan>(Settings.Loans);
			if (loans.Count == 0)
			{
				Notification.ShowInfo("NoLoansToClear".LocDef("There are no loans to clear."));
				return;
			}

			Confirmation.AskAndApply("Clear all loans", string.Format("ClearLoansConfirm".LocDef("Remove all {0} loan(s) without repaying them?\nNo payment is made, the remaining payments are cancelled and this cannot be undone."), loans.Count), () => Confirmation.ChangedUnless(Confirmation.IsSameTargets(loans, Settings.Loans)), ApplyClearLoans);
		}

		internal static void AIBankrupt()
		{
			if (!Helpers.RequireGameReady("AI Bankrupt All"))
				return;

			List<SimulatedCompany> companies = FindAllBankruptTargets();
			if (companies.Count == 0)
			{
				Notification.ShowInfo("NoAICompaniesToBankrupt".LocDef("There is no AI company left to bankrupt."));
				return;
			}

			Confirmation.AskAndApply("AI Bankrupt All", string.Format("BankruptAllConfirm".LocDef("Bankrupt all {0} AI company(ies)?\nThe game cancels their deals and removes them from the market. This cannot be undone.\nYour own company and its subsidiaries are not affected."), companies.Count), () => Confirmation.ChangedUnless(Confirmation.IsSameTargets(companies, FindAllBankruptTargets())), () => ApplyAIBankrupt(companies));
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
			var simulatedCompany = FindCompanyByName(input);
			if (simulatedCompany == null)
			{
				return;
			}

			TakeOver(simulatedCompany);
		}

		internal static void TakeOver(SimulatedCompany simulatedCompany)
		{
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
			Notification.ShowInfo(string.Format("CompanyTakenOver".LocDef("Trainer: Company {0} has been takovered by you!"), simulatedCompany.Name));
		}

		internal static void TakeoverCompany()
		{
			WindowManager.SpawnInputDialog(SharedMessages.CompanyNamePrompt(), "TakeoverCompanyTitle".LocDef("Takeover Company"), "", TakeoverCompanyAction);
		}

		internal static void SubDCompanyAction(string input)
		{
			SimulatedCompany Company = FindCompanyByName(input);
			if (Company == null)
			{
				return;
			}

			MakeSubsidiary(Company);
		}

		internal static void MakeSubsidiary(SimulatedCompany Company)
		{
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

			AskForceBankrupt(matches[0]);
		}

		internal static void AskForceBankrupt(SimulatedCompany company)
		{
			if (company.Bankrupt)
			{
				Notification.ShowInfo(SharedMessages.CompanyAlreadyBankrupt(company.Name));
				return;
			}

			// One-way: the flag is only ever set, never toggled back.
			Confirmation.AskAndApply("Bankrupt", string.Format("BankruptConfirmPrompt".LocDef("Bankrupt {0}?\nThe game cancels its deals and removes it from the market. This cannot be undone."), company.Name), () => AICompanyChange(company), () => ConfirmForceBankrupt(company));
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

			AdjustAIFunds(matches[0]);
		}

		internal static void AddAIFunds()
		{
			WindowManager.SpawnInputDialog("AICompanyNamePrompt".LocDef("Type AI company name:"), SharedMessages.AdjustAIFundsTitle(), "", AddAIFundsAction);
		}

		internal static void AdjustAIFunds(SimulatedCompany company)
		{
			InputHelper.RequestInt(string.Format("AdjustAIFundsPrompt".LocDef("How much money do you want to add to {0}? A negative amount subtracts.\nCurrent funds: {1}"), company.Name, company.Money.Currency()), SharedMessages.AdjustAIFundsTitle(), "100000", amount => AdjustAIFundsBy(company, amount));
		}

		internal static void MaxReputation()
		{
			Confirmation.AskAndApply("Max Reputation", "MaxReputationConfirm".LocDef("Set your business reputation to the maximum in all categories?"), null, ApplyMaxReputation);
		}

		internal static void SetBusinessReputation()
		{
			if (!Helpers.RequireGameReady("Set Business Reputation"))
				return;

			GameSettings game = Settings;
			Company company = game.MyCompany;
			RequestBusinessStars(company, () =>
			{
				if (ReferenceEquals(game, Settings) && Settings.MyCompany == company)
					return true;

				Confirmation.ShowScopeChanged();
				return false;
			});
		}

		internal static void SetAIBusinessReputation(SimulatedCompany company)
		{
			RequestBusinessStars(company, () => IsAdjustableAICompany(company));
		}

		// Just above the star boundary: the game's float rounding could otherwise show one star less.
		internal static float BusinessReputationFor(int stars)
		{
			return stars >= Company.BusinessRepStars ? 1f : (stars + 0.001f) / Company.BusinessRepStars;
		}

		internal static bool IsBusinessReputationAt(float reputation, int stars)
		{
			return Math.Abs(reputation * Company.BusinessRepStars - stars) < 0.01f;
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

			List<string> rewards = UnclaimedRewards();
			if (rewards.Count == 0)
			{
				Notification.ShowInfo("NoRewardsToUnlock".LocDef("Every reward is already unlocked and claimed."));
				return;
			}

			Confirmation.AskAndApply("Unlock and Claim Rewards", string.Format("UnlockRewardsConfirm".LocDef("Mark {0} reward task(s) as completed and claimed in this save?\nThe game has no way to undo this."), rewards.Count), () => Confirmation.ChangedUnless(Confirmation.IsSameTargets(rewards, UnclaimedRewards())), ApplyUnlockAndClaimAllRewards);
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
			return IsBankruptEligible(company == Settings.MyCompany, company.IsPlayerOwned());
		}

		// The player's company and its subsidiaries (player-owned) are never targets.
		internal static bool IsBankruptEligible(bool isPlayersCompany, bool isPlayerOwned)
		{
			return !isPlayersCompany && !isPlayerOwned;
		}

		private static string ApplyMaxReputation()
		{
			Settings.MyCompany.ChangeBusinessRep(1f, "Publisher", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Deal", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Printing", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Lawsuit", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Contract", 1f);
			Settings.MyCompany.ChangeBusinessRep(1f, "Hosting", 1f);
			return "MaxReputationApplied".LocDef("Trainer: Max reputation is applied to all categories");
		}

		private static List<SimulatedCompany> FindAllBankruptTargets()
		{
			return Settings.simulation.Companies.Values.Where(company => IsBankruptTarget(company) && !company.Bankrupt).ToList();
		}

		private static string ApplyAIBankrupt(List<SimulatedCompany> targets)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				targets[i].Bankrupt = true;
			}

			return string.Format("AIBankruptAllDone".LocDef("Trainer: {0} AI company(ies) are going bankrupt."), targets.Count);
		}

		private static string ApplyClearLoans()
		{
			Settings.Loans.Clear();
			return "AllLoansCleared".LocDef("Trainer: All loans are cleared!");
		}

		private static List<string> UnclaimedRewards()
		{
			return GameData.Tasks
				.Where(task => !Settings.CompletedTasks.Contains(task.Name) || !Settings.ClaimedRewards.Contains(task.Name))
				.Select(task => task.Name)
				.Distinct()
				.ToList();
		}

		private static string ApplyUnlockAndClaimAllRewards()
		{
			Settings.CompletedTasks.AddRange(GameData.Tasks.Select(x => x.Name));
			Settings.ClaimedRewards.AddRange(GameData.Tasks.Select(x => x.Name));
			HUD.Instance.RefreshBuildButtons();
			HUD.Instance.UpdateFurnitureButtons();

			return "RewardsUnlockedClaimed".LocDef("Trainer: All rewards are unlocked and claimed.");
		}

		// The game bankrupts an AI company whose money a transaction would take below zero, so that case is warned about.
		private static void AdjustAIFundsBy(SimulatedCompany company, int amount)
		{
			if (!IsAdjustableAICompany(company))
				return;

			if (amount == 0)
			{
				Notification.ShowInfo("AIFundsZero".LocDef("The amount is 0. Nothing was changed."));
				return;
			}

			if (amount > 0)
			{
				Notification.Popup(ApplyAIFunds(company, amount));
				return;
			}

			double reduction = -(double)amount;
			double money = company.Money;
			bool bankrupts = !company.CanMakeTransaction(amount);
			string message = bankrupts
				? string.Format("AIFundsBankruptConfirm".LocDef("Subtract {1} from {0}?\nThat is more than its funds of {2}: the game bankrupts the company at once, cancels its deals and removes it from the market. This cannot be undone."), company.Name, reduction.Currency(), money.Currency())
				: string.Format("AIFundsReduceConfirm".LocDef("Subtract {1} from {0}?\nIts funds go from {2} to {3}."), company.Name, reduction.Currency(), money.Currency(), (money + amount).Currency());
			// Its funds may have dropped while the question was open; a bankruptcy needs its own warning.
			Confirmation.AskAndApply("Adjust AI Funds", message, () => AICompanyChange(company) ?? Confirmation.ChangedUnless(bankrupts || company.CanMakeTransaction(amount)), () => ApplyAIFunds(company, amount));
		}

		private static bool IsAdjustableAICompany(SimulatedCompany company)
		{
			string change = AICompanyChange(company);
			if (change != null)
			{
				Notification.ShowError(change);
				return false;
			}

			return true;
		}

		private static string AICompanyChange(SimulatedCompany company)
		{
			if (!IsLiveAICompany(company))
			{
				return "CompanyChangedNothingEdited".LocDef("The company is no longer available. Nothing was changed.");
			}

			return company.Bankrupt ? SharedMessages.CompanyAlreadyBankrupt(company.Name) : null;
		}

		private static void RequestBusinessStars(Company company, Func<bool> isStillTarget)
		{
			string prompt = string.Format("SetBusinessRepPrompt".LocDef("How many business reputation stars (0 to 6) should {0} have?\nCurrent stars: {1}"), company.Name, company.BusinessStars);
			InputHelper.RequestInt(prompt, "SetBusinessRepTitle".LocDef("Set Business Reputation"), company.BusinessStars.ToString(), stars =>
			{
				if (isStillTarget())
				{
					ApplyBusinessStars(company, stars);
				}
			}, 0, Company.BusinessRepStars);
		}

		// As the game's own reputation cheat; history is recorded only for the player.
		private static void ApplyBusinessStars(Company company, int stars)
		{
			if (IsBusinessReputationAt(company.BusinessReputation, stars))
			{
				Notification.ShowInfo(string.Format("BusinessRepUnchanged".LocDef("{0} already has {1} business reputation star(s). Nothing was changed."), company.Name, stars));
				return;
			}

			company.ChangeBusinessRep((BusinessReputationFor(stars) - company.BusinessReputation) * Company.BusinessRepStars, "Cheat", Company.BusinessRepStars);
			Notification.Popup(string.Format("BusinessRepSet".LocDef("Trainer: Business reputation of {0} is set to {1} star(s)!"), company.Name, company.BusinessStars));
		}

		private static string ApplyAIFunds(SimulatedCompany company, int amount)
		{
			company.MakeTransaction(amount, Company.TransactionCategory.Deals);
			return amount > 0
				? string.Format("AIFundsAdded".LocDef("Trainer: Money has been added to {0}!"), company.Name)
				: string.Format("AIFundsSubtracted".LocDef("Trainer: Money has been subtracted from {0}!"), company.Name);
		}

		private static string ConfirmForceBankrupt(SimulatedCompany company)
		{
			company.Bankrupt = true;
			return string.Format("BankruptDone".LocDef("Trainer: {0} is going bankrupt."), company.Name);
		}

		internal static bool IsLiveAICompany(SimulatedCompany company)
		{
			SimulatedCompany current;
			return Settings != null && Settings.simulation.Companies.TryGetValue(company.ID, out current) && current == company && IsBankruptTarget(company);
		}

		private static SimulatedCompany FindCompanyByName(string input)
		{
			List<SimulatedCompany> matches = Settings.simulation.Companies.Values.Where(company => company.Name == input).ToList();
			if (matches.Count == 0)
			{
				Notification.ShowError(SharedMessages.CompanyNotFound(input));
				return null;
			}

			if (matches.Count > 1)
			{
				Notification.ShowError(SharedMessages.CompanyNameAmbiguous(input));
				return null;
			}

			return matches[0];
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
