using System.Linq;
using UnityEngine;

namespace Trainer_v5.Actions
{
	public static class CompanyActions
	{
		private static GameSettings Settings => GameSettings.Instance;

		public static void AIBankrupt()
		{
			SimulatedCompany[] Companies = Settings.simulation.Companies.Values.ToArray();

			for (int i = 0; i < Companies.Length; i++)
			{
				// Don't bankrupt the player's company
				if (Companies[i] != Settings.MyCompany)
				{
					Companies[i].Bankrupt = true;
				}
			}
			HUD.Instance.AddPopupMessage("Trainer: All AI companies forced into bankruptcy!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void TakeoverCompanyAction(string input)
		{
			var simulatedCompany = Settings.simulation.Companies
				.FirstOrDefault(simCompany => simCompany.Value.Name.Equals(input, System.StringComparison.OrdinalIgnoreCase)).Value;

			if (simulatedCompany == null)
			{
				WindowManager.SpawnDialog("Trainer: Company '" + input + "' not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (simulatedCompany == Settings.MyCompany)
			{
				WindowManager.SpawnDialog("Trainer: Cannot takeover your own company!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (!simulatedCompany.CanBuyOut(Settings.MyCompany))
			{
				WindowManager.SpawnDialog("Trainer: Company '" + input + "' can't be bought out (already owned, too expensive, or other reason).", false, DialogWindow.DialogType.Information);
				return;
			}

			var simulatedCompanyWorth = simulatedCompany.GetPossibleStockWorth();

			simulatedCompany.BuyOut(
				new Company[] { Settings.MyCompany }, // companies buying out
				false,                                // not broke
				SDateTime.Now(),                      // current time
				true                                  // can disconnect (default value)
			);

			// Deduct cost from player - MakeTransaction handles negative values for costs
			Settings.MyCompany.MakeTransaction(-simulatedCompanyWorth, Company.TransactionCategory.Stocks, "Takeover: " + simulatedCompany.Name, false);
			WindowManager.SpawnDialog("Trainer: Company '" + input + "' has been taken over!", false, DialogWindow.DialogType.Information);
		}

		public static void TakeoverCompany()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Takeover Company", "", TakeoverCompanyAction);
		}

		public static void SubDCompanyAction(string input)
		{
			SimulatedCompany companyToSub =
				Settings.simulation.Companies.FirstOrDefault(company => company.Value.Name.Equals(input, System.StringComparison.OrdinalIgnoreCase)).Value;

			if (companyToSub == null)
			{
				WindowManager.SpawnDialog("Trainer: Company '" + input + "' not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (companyToSub == Settings.MyCompany)
			{
				WindowManager.SpawnDialog("Trainer: Cannot make your own company a subsidiary!", false, DialogWindow.DialogType.Information);
				return;
			}

			// Check if the company is already a subsidiary of the player's company
			bool isAlreadySubsidiary = false;
			if (Settings.MyCompany.Subsidiaries != null && companyToSub != null)
			{
				// Assuming Subsidiaries is a collection of uint IDs and SimulatedCompany has a uint ID property
				foreach (uint subId in Settings.MyCompany.Subsidiaries)
				{
					// Compare the ID from the collection with the target company's ID
					if (subId == companyToSub.ID) 
					{
						isAlreadySubsidiary = true;
						break;
					}
				}
			}

			if (isAlreadySubsidiary)
			{
				WindowManager.SpawnDialog("Trainer: Company '" + input + "' is already your subsidiary!", false, DialogWindow.DialogType.Information);
				return;
			}

			// You might need to buy out the company first if not already owned.
			// This implementation assumes you already own it or making it a subsidiary doesn't require full ownership.
			// Add buyout logic here if necessary, similar to TakeoverCompanyAction.

			companyToSub.MakeSubsidiary(Settings.MyCompany, SDateTime.Now());
			HUD.Instance.AddPopupMessage("Trainer: Company '" + companyToSub.Name + "' is now your subsidiary!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void SubDCompany()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Make Subsidiary", "", SubDCompanyAction);
		}

		public static void ForceBankruptAction(string input)
		{
			SimulatedCompany companyToBankrupt =
				Settings.simulation.Companies.FirstOrDefault(company => company.Value.Name.Equals(input, System.StringComparison.OrdinalIgnoreCase)).Value;

			if (companyToBankrupt == null)
			{
				WindowManager.SpawnDialog("Trainer: Company '" + input + "' not found!", false, DialogWindow.DialogType.Information);
				return;
			}

			if (companyToBankrupt == Settings.MyCompany)
			{
				WindowManager.SpawnDialog("Trainer: Cannot force bankrupt your own company!", false, DialogWindow.DialogType.Information);
				return;
			}

			companyToBankrupt.Bankrupt = !companyToBankrupt.Bankrupt; // Toggle bankruptcy state
			string status = companyToBankrupt.Bankrupt ? "bankrupt" : "no longer bankrupt";
			HUD.Instance.AddPopupMessage($"Trainer: Company '{companyToBankrupt.Name}' is now {status}!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void ForceBankrupt()
		{
			WindowManager.SpawnInputDialog("Type company name:", "Toggle Force Bankrupt", "", ForceBankruptAction);
		}
	}
}
