using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5
{
	public class SettingsWindow : MonoBehaviour
	{
		private static readonly string _title = $"Trainer Settings, by Trawis (v{Helpers.Version}) | GAME VERSION: {Helpers.GetGameVersion()}";

		public static GUIWindow Window { get; set; }
		public static bool Shown { get; set; }

		public static void Toggle()
		{
			if (Window == null)
			{
				Init();
			}
			else
			{
				Window.Toggle();
			}

			Shown = Window.Shown;
		}

		private static void Init()
		{
			var settings = Helpers.Settings;
			var storesSettings = Helpers.StoresSettings;
			var efficiencySelectItems = Helpers.EfficiencySelectItems;

			Window = UIHelper.SpawnWindow(_title, "TrainerSettings", "TrainerSettingsPanel");

			if (Window.name == "TrainerSettings")
			{
				var closeButtons = Window.GetComponentsInChildren<Button>()
				  .Where(x => x.name == "CloseButton")
				  .ToList();

				if (closeButtons.Count == 1)
				{
					closeButtons[0].onClick.AddListener(() => Shown = false);
				}
				else if (closeButtons.Count == 0)
				{
					"SettingsWindow: no CloseButton found on the spawned window; close button listener was not attached.".Log();
				}
				else
				{
					$"SettingsWindow: expected exactly one CloseButton but found {closeButtons.Count}; close button listener was not attached.".Log();
				}
			}

			bool experimental = Helpers.GetProperty(settings, SettingKeys.Experimental) || Helpers.IsDebug;

			List<GameObject> column1 = new List<GameObject>();
			List<GameObject> column2 = new List<GameObject>();
			List<GameObject> column3 = new List<GameObject>();
			List<GameObject> column4 = new List<GameObject>();
			List<GameObject> column5 = new List<GameObject>();
			List<GameObject> column6 = new List<GameObject>();

			#region column1

			column1.Add(UIHelper.CreateButton("AddMoney".LocDef("Add Money"), CompanyFeatures.IncreaseMoney));
			column1.Add(UIHelper.CreateButton("AddAIFunds".LocDef("Add AI Funds"), CompanyFeatures.AddAIFunds));
			column1.Add(UIHelper.CreateButton("MaxFollowers".LocDef("Max Followers"), WorkItemFeatures.MaxFollowers));
			column1.Add(UIHelper.CreateLabel());
			column1.Add(UIHelper.CreateInputBox("ProductName".LocDef("Product Name Here"), boxText => Helpers.ProductPriceName = boxText));
			column1.Add(UIHelper.CreateLabel());
			column1.Add(UIHelper.CreateButton("TakeoverCompany".LocDef("Takeover Company"), CompanyFeatures.TakeoverCompany));
			column1.Add(UIHelper.CreateLabel());

			column1.Add(UIHelper.CreateButton("BankruptAll".LocDef("AI Bankrupt All"), CompanyFeatures.AIBankrupt));
			column1.Add(UIHelper.CreateButton("DaysPerMonth".LocDef("Days per month"), CompanyFeatures.MonthDays));
			column1.Add(UIHelper.CreateButton("ClearAllLoans".LocDef("Clear all loans"), CompanyFeatures.ClearLoans));
			column1.Add(UIHelper.CreateButton("MaxMarketRecognition".LocDef("Max market recognition"), CompanyFeatures.MaxMarketRecognition));
			column1.Add(UIHelper.CreateButton("MaxSkill".LocDef("Max Skill of employees"), EmployeeFeatures.EmployeesToMax));
			column1.Add(UIHelper.CreateButton("ResetAge".LocDef("Reset age of employees"), EmployeeFeatures.ResetAgeOfEmployees));
			column1.Add(UIHelper.CreateButton("SellProductsStock".LocDef("Sell products stock"), ProductFeatures.SellProductStock));
			column1.Add(UIHelper.CreateButton("UnlockAllFurniture".LocDef("Unlock all furniture"), CompanyFeatures.UnlockFurniture));
			column1.Add(UIHelper.CreateButton("UnlockAllSpace".LocDef("Unlock all space"), CompanyFeatures.UnlockAllSpace));
			column1.Add(UIHelper.CreateButton("ExtendDeadline".LocDef("Extend Deadline"), WorkItemFeatures.ExtendDeadline));
			column1.Add(UIHelper.CreateButton("UnlockAndClaimRewards".LocDef("Unlock and Claim Rewards"), CompanyFeatures.UnlockAndClaimAllRewards));

			#endregion

			#region column2

			column2.Add(UIHelper.CreateButton("MaxReputation".LocDef("Max Reputation"), CompanyFeatures.MaxReputation));
			column2.Add(UIHelper.CreateButton("FixBugs".LocDef("Fix Bugs"), WorkItemFeatures.FixBugs));
			column2.Add(UIHelper.CreateButton("InstantResearch".LocDef("Instant Research"), WorkItemFeatures.InstantResearch));
			column2.Add(UIHelper.CreateButton("InstantPorting".LocDef("Instant Porting"), WorkItemFeatures.InstantPorting));
			column2.Add(UIHelper.CreateButton("InstantMarketing".LocDef("Instant Marketing"), MarketFeatures.InstantMarketing));
			column2.Add(UIHelper.CreateButton("TransferOffshoreFunds".LocDef("Transfer Offshore Funds"), CompanyFeatures.TransferOffshoreFunds));
			column2.Add(UIHelper.CreateButton("OptimizeTeamAssignments".LocDef("Optimize Team Assignments"), WorkItemFeatures.OptimizeTeamAssignments));
			column2.Add(UIHelper.CreateLabel());
			column2.Add(UIHelper.CreateButton("SetProductPrice".LocDef("Set Product Price"), ProductFeatures.SetProductPrice));
			column2.Add(UIHelper.CreateLabel());
			column2.Add(UIHelper.CreateButton("SubsidiaryCompany".LocDef("Subsidiary Company"), CompanyFeatures.SubDCompany));
			column2.Add(UIHelper.CreateLabel());

			column2.Add(CreateSettingToggle(settings, "DisableNeeds", "Disable Needs", SettingKeys.NoNeeds));
			column2.Add(CreateSettingToggle(settings, "DisableStress", "Disable Stress", SettingKeys.NoStress));
			column2.Add(CreateSettingToggle(settings, "FreeEmployees", "Free Employees", SettingKeys.FreeEmployees));
			column2.Add(CreateSettingToggle(settings, "FreeStaff", "Free Staff", SettingKeys.FreeStaff));
			column2.Add(CreateSettingToggle(settings, "FullSatisfaction", "Full Satisfaction", SettingKeys.FullSatisfaction));
			column2.Add(CreateSettingToggle(settings, "LockAge", "Lock Age of Employees", SettingKeys.LockAge));
			column2.Add(CreateSettingToggle(settings, "NoVacation", "No Vacation", SettingKeys.NoVacation));
			column2.Add(CreateSettingToggle(settings, "NoSickness", "No Sickness", SettingKeys.NoSickness));
			column2.Add(CreateSettingToggle(settings, "AutoResearchStart", "Auto Research Start", SettingKeys.AutoResearchStart));
			column2.Add(CreateSettingToggle(settings, "DigitalDistributionMonopol", "Digital Distribution Monopol", SettingKeys.DigitalDistributionMonopol));
			column2.Add(CreateSettingToggle(settings, "DisableFireInspection", "Disable Fire Inspection", SettingKeys.DisableFireInspection));
			column2.Add(UIHelper.CreateToggle("UnlimitedSubsidiaries".LocDef("Unlimited Subsidiaries"), settings.Get(SettingKeys.UnlimitedSubsidiaries), a => { settings.Toggle(SettingKeys.UnlimitedSubsidiaries); CompanyFeatures.ApplyUnlimitedSubsidiaries(settings.Get(SettingKeys.UnlimitedSubsidiaries)); }));

			#endregion

			#region column3

			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateButton("SetProductStock".LocDef("Set Product Stock"), ProductFeatures.SetProductStock));
			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateButton("Bankrupt".LocDef("Bankrupt"), CompanyFeatures.ForceBankrupt));
			column3.Add(UIHelper.CreateLabel());

			column3.Add(CreateSettingToggle(settings, "FullEnvironment", "Full Environment", SettingKeys.FullEnvironment));
			column3.Add(CreateSettingToggle(settings, "FullSunLight", "Full Sun Light", SettingKeys.FullRoomBrightness));
			column3.Add(CreateSettingToggle(settings, "LockTemperature", "Lock Temperature To 21", SettingKeys.TemperatureLock));
			column3.Add(CreateSettingToggle(settings, "NoMaintenance", "No Maintenance", SettingKeys.NoMaintenance));
			column3.Add(CreateSettingToggle(settings, "NoiseReduction", "Noise Reduction", SettingKeys.NoiseReduction));
			column3.Add(CreateSettingToggle(settings, "RoomsNeverDirty", "Rooms Never Dirty", SettingKeys.CleanRooms));
			column3.Add(CreateSettingToggle(settings, "NoEducationCost", "No Education Cost", SettingKeys.NoEducationCost));
			column3.Add(CreateSettingToggle(settings, "DisableFires", "Disable Fires", SettingKeys.DisableFires));
			column3.Add(CreateSettingToggle(settings, "AutoDesignEnd", "Auto Design End", SettingKeys.AutoEndDesign));
			column3.Add(CreateSettingToggle(settings, "AutoContractProgression", "Auto Contract Progression", SettingKeys.AutoContractProgression));
			column3.Add(CreateSettingToggle(settings, "AutoResearchEnd", "Auto Research End", SettingKeys.AutoEndResearch));
			column3.Add(CreateSettingToggle(settings, "AutoPatentEnd", "Auto Patent End", SettingKeys.AutoEndPatent));
			column3.Add(CreateSettingToggle(settings, "IncreaseWalkSpeed", "Increase Walk Speed", SettingKeys.IncreaseWalkSpeed));
			column3.Add(CreateSettingToggle(settings, "DisableFurnitureStealing", "Disable Furniture Stealing", SettingKeys.DisableFurnitureStealing));
			column3.Add(UIHelper.CreateToggle("ForceLights".LocDef("Force Lights"), settings.Get(SettingKeys.ForceLights), a => { settings.Toggle(SettingKeys.ForceLights); EnvironmentFeatures.ApplyForceLights(settings.Get(SettingKeys.ForceLights)); }));
			column3.Add(UIHelper.CreateToggle("ShowRoomCeilings".LocDef("Show Room Ceilings"), settings.Get(SettingKeys.ShowRoomCeilings), a => { settings.Toggle(SettingKeys.ShowRoomCeilings); EnvironmentFeatures.ApplyShowRoomCeilings(settings.Get(SettingKeys.ShowRoomCeilings)); }));

			#endregion

			#region column4

			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateButton("SetActiveUsers".LocDef("Set Active Users"), ProductFeatures.AddActiveUsers));
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());

			column4.Add(CreateSettingToggle(settings, "FreePrint", "Free Print", SettingKeys.FreePrint));
			column4.Add(CreateSettingToggle(settings, "FreeWaterElectricity", "Free Water & Electricity", SettingKeys.NoWaterElectricity));
			column4.Add(CreateSettingToggle(settings, "IncreaseBookshelfSkill", "Increase Bookshelf Skill", SettingKeys.IncreaseBookshelfSkill));
			column4.Add(CreateSettingToggle(settings, "IncreaseCourierCapacity", "Increase Courier Capacity", SettingKeys.IncreaseCourierCapacity));
			column4.Add(CreateSettingToggle(settings, "IncreasePrintSpeed", "Increase Print Speed", SettingKeys.IncreasePrintSpeed));
			column4.Add(CreateSettingToggle(settings, "MoreHostingDeals", "More Hosting Deals", SettingKeys.MoreHostingDeals));
			column4.Add(CreateSettingToggle(settings, "ReduceInternetCost", "Reduce Internet Cost", SettingKeys.ReduceISPCost));
			column4.Add(CreateSettingToggle(settings, "NoServerCost", "No Server Cost", SettingKeys.NoServerCost));
			column4.Add(CreateSettingToggle(settings, "ReduceExpansionCost", "Reduce Expansion Cost", SettingKeys.ReduceExpansionCost));
			column4.Add(CreateSettingToggle(settings, "DisableForcePause", "Disable Force Pause", SettingKeys.DisableForcePause));
			column4.Add(CreateSettingToggle(settings, "DisableForceFreeze", "Disable Force Freeze", SettingKeys.DisableForceFreeze));
			column4.Add(CreateSettingToggle(settings, "AutoAcceptHostingDeals", "Auto Accept Hosting Deals", SettingKeys.AutoAcceptHostingDeals));
			column4.Add(CreateSettingToggle(settings, "AutoPorting", "Auto Porting", SettingKeys.AutoPorting));
			column4.Add(CreateSettingToggle(settings, "MoreInspiration", "More Inspiration", SettingKeys.MoreInspiration));
			column4.Add(CreateSettingToggle(settings, "MoreCreativity", "More Creativity", SettingKeys.MoreCreativity));
			column4.Add(CreateSettingToggle(settings, "NoMissedSupportTickets", "No Missed Support Tickets", SettingKeys.NoMissedSupportTickets));
			column4.Add(CreateSettingToggle(settings, "NoInsuranceCost", "No Insurance Cost", SettingKeys.NoInsuranceCost));
			column4.Add(CreateSettingToggle(settings, "NoLoanInterest", "No Loan Interest", SettingKeys.NoLoanInterest));
			column4.Add(CreateSettingToggle(settings, "FreeMarketing", "Free Marketing", SettingKeys.FreeMarketing));
			column4.Add(CreateSettingToggle(settings, "NoOffshoreHeat", "No Offshore Heat", SettingKeys.NoOffshoreHeat));
			column4.Add(CreateSettingToggle(settings, "NoFounderDividends", "No Founder Dividends", SettingKeys.NoFounderDividends));

			#endregion

			#region column5

			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateButton("Discord".LocDef("DISCORD"), () => Helpers.ShowDiscordInvite()));
			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateLabel());

			column5.Add(UIHelper.CreateLabel("Efficiency"));
			var efficiencyComboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, SettingKeys.EfficiencyStore, 2)
			);
			efficiencyComboBox.OnSelectedChanged.AddListener(() => storesSettings.Set(SettingKeys.EfficiencyStore, efficiencySelectItems.GetAt(efficiencyComboBox.Selected).Value));
			column5.Add(efficiencyComboBox.gameObject);

			column5.Add(UIHelper.CreateLabel("Lead Efficiency"));
			var leadEfficiencyComboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, SettingKeys.LeadEfficiencyStore, 2)
			);
			leadEfficiencyComboBox.OnSelectedChanged.AddListener(() => storesSettings.Set(SettingKeys.LeadEfficiencyStore, efficiencySelectItems.GetAt(leadEfficiencyComboBox.Selected).Value));
			column5.Add(leadEfficiencyComboBox.gameObject);

			#endregion

			#region column6
			column6.Add(CreateSettingToggle(settings, "Experimental", "Experimental", SettingKeys.Experimental));
			column6.Add(UIHelper.CreateLabel("After toggle re-open the window"));
			column6.Add(UIHelper.CreateLabel());

			if (experimental)
			{
				bool isOn = false;
				column6.Add(UIHelper.CreateToggle("TestToggle".LocDef("Test Toggle"), isOn, a => isOn = !isOn));
				column6.Add(UIHelper.CreateButton("TestButton".LocDef("Test Button"), WorkItemFeatures.TestButton));

			}

			#endregion

			column1.AddToWindow(Window, Constants.FIRST_COLUMN);
			column2.AddToWindow(Window, Constants.SECOND_COLUMN);
			column3.AddToWindow(Window, Constants.THIRD_COLUMN);
			column4.AddToWindow(Window, Constants.FOURTH_COLUMN);
			column5.AddToWindow(Window, Constants.FIFTH_COLUMN, isComboBox: true);
			column6.AddToWindow(Window, Constants.SIXTH_COLUMN);

			int maxRowsCount = new[] { column1.Count, column2.Count, column3.Count, column4.Count, column5.Count, column6.Count }.Max();
			Window.SetWindowSize(maxRowsCount, Constants.X_SETTINGS_WINDOW);
		}

		private static GameObject CreateSettingToggle(Dictionary<string, bool> settings, string locKey, string defaultText, SettingKey<bool> key)
		{
			return UIHelper.CreateToggle(locKey.LocDef(defaultText), settings.Get(key), a => settings.Toggle(key));
		}
	}
}