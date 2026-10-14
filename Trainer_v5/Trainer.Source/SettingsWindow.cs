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

			Window = WindowManager.SpawnWindow();
			Window.InitialTitle = Window.TitleText.text = Window.NonLocTitle = _title;
			Window.name = "TrainerSettings";
			Window.MainPanel.name = "TrainerSettingsPanel";

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

			column2.Add(UIHelper.CreateToggle("DisableNeeds".LocDef("Disable Needs"), settings.Get(SettingKeys.NoNeeds), a => settings.Toggle(SettingKeys.NoNeeds)));
			column2.Add(UIHelper.CreateToggle("DisableStress".LocDef("Disable Stress"), settings.Get(SettingKeys.NoStress), a => settings.Toggle(SettingKeys.NoStress)));
			column2.Add(UIHelper.CreateToggle("FreeEmployees".LocDef("Free Employees"), settings.Get(SettingKeys.FreeEmployees), a => settings.Toggle(SettingKeys.FreeEmployees)));
			column2.Add(UIHelper.CreateToggle("FreeStaff".LocDef("Free Staff"), settings.Get(SettingKeys.FreeStaff), a => settings.Toggle(SettingKeys.FreeStaff)));
			column2.Add(UIHelper.CreateToggle("FullSatisfaction".LocDef("Full Satisfaction"), settings.Get(SettingKeys.FullSatisfaction), a => settings.Toggle(SettingKeys.FullSatisfaction)));
			column2.Add(UIHelper.CreateToggle("LockAge".LocDef("Lock Age of Employees"), settings.Get(SettingKeys.LockAge), a => settings.Toggle(SettingKeys.LockAge)));
			column2.Add(UIHelper.CreateToggle("NoVacation".LocDef("No Vacation"), settings.Get(SettingKeys.NoVacation), a => settings.Toggle(SettingKeys.NoVacation)));
			column2.Add(UIHelper.CreateToggle("NoSickness".LocDef("No Sickness"), settings.Get(SettingKeys.NoSickness), a => settings.Toggle(SettingKeys.NoSickness)));
			column2.Add(UIHelper.CreateToggle("AutoResearchStart".LocDef("Auto Research Start"), settings.Get(SettingKeys.AutoResearchStart), a => settings.Toggle(SettingKeys.AutoResearchStart)));
			column2.Add(UIHelper.CreateToggle("DigitalDistributionMonopol".LocDef("Digital Distribution Monopol"), settings.Get(SettingKeys.DigitalDistributionMonopol), a => settings.Toggle(SettingKeys.DigitalDistributionMonopol)));
			column2.Add(UIHelper.CreateToggle("DisableFireInspection".LocDef("Disable Fire Inspection"), settings.Get(SettingKeys.DisableFireInspection), a => settings.Toggle(SettingKeys.DisableFireInspection)));
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

			column3.Add(UIHelper.CreateToggle("FullEnvironment".LocDef("Full Environment"), settings.Get(SettingKeys.FullEnvironment), a => settings.Toggle(SettingKeys.FullEnvironment)));
			column3.Add(UIHelper.CreateToggle("FullSunLight".LocDef("Full Sun Light"), settings.Get(SettingKeys.FullRoomBrightness), a => settings.Toggle(SettingKeys.FullRoomBrightness)));
			column3.Add(UIHelper.CreateToggle("LockTemperature".LocDef("Lock Temperature To 21"), settings.Get(SettingKeys.TemperatureLock), a => settings.Toggle(SettingKeys.TemperatureLock)));
			column3.Add(UIHelper.CreateToggle("NoMaintenance".LocDef("No Maintenance"), settings.Get(SettingKeys.NoMaintenance), a => settings.Toggle(SettingKeys.NoMaintenance)));
			column3.Add(UIHelper.CreateToggle("NoiseReduction".LocDef("Noise Reduction"), settings.Get(SettingKeys.NoiseReduction), a => settings.Toggle(SettingKeys.NoiseReduction)));
			column3.Add(UIHelper.CreateToggle("RoomsNeverDirty".LocDef("Rooms Never Dirty"), settings.Get(SettingKeys.CleanRooms), a => settings.Toggle(SettingKeys.CleanRooms)));
			column3.Add(UIHelper.CreateToggle("NoEducationCost".LocDef("No Education Cost"), settings.Get(SettingKeys.NoEducationCost), a => settings.Toggle(SettingKeys.NoEducationCost)));
			column3.Add(UIHelper.CreateToggle("DisableFires".LocDef("Disable Fires"), settings.Get(SettingKeys.DisableFires), a => settings.Toggle(SettingKeys.DisableFires)));
			column3.Add(UIHelper.CreateToggle("AutoDesignEnd".LocDef("Auto Design End"), settings.Get(SettingKeys.AutoEndDesign), a => settings.Toggle(SettingKeys.AutoEndDesign)));
			column3.Add(UIHelper.CreateToggle("AutoContractProgression".LocDef("Auto Contract Progression"), settings.Get(SettingKeys.AutoContractProgression), a => settings.Toggle(SettingKeys.AutoContractProgression)));
			column3.Add(UIHelper.CreateToggle("AutoResearchEnd".LocDef("Auto Research End"), settings.Get(SettingKeys.AutoEndResearch), a => settings.Toggle(SettingKeys.AutoEndResearch)));
			column3.Add(UIHelper.CreateToggle("AutoPatentEnd".LocDef("Auto Patent End"), settings.Get(SettingKeys.AutoEndPatent), a => settings.Toggle(SettingKeys.AutoEndPatent)));
			column3.Add(UIHelper.CreateToggle("IncreaseWalkSpeed".LocDef("Increase Walk Speed"), settings.Get(SettingKeys.IncreaseWalkSpeed), a => settings.Toggle(SettingKeys.IncreaseWalkSpeed)));
			column3.Add(UIHelper.CreateToggle("DisableFurnitureStealing".LocDef("Disable Furniture Stealing"), settings.Get(SettingKeys.DisableFurnitureStealing), a => settings.Toggle(SettingKeys.DisableFurnitureStealing)));
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

			column4.Add(UIHelper.CreateToggle("FreePrint".LocDef("Free Print"), settings.Get(SettingKeys.FreePrint), a => settings.Toggle(SettingKeys.FreePrint)));
			column4.Add(UIHelper.CreateToggle("FreeWaterElectricity".LocDef("Free Water & Electricity"), settings.Get(SettingKeys.NoWaterElectricity), a => settings.Toggle(SettingKeys.NoWaterElectricity)));
			column4.Add(UIHelper.CreateToggle("IncreaseBookshelfSkill".LocDef("Increase Bookshelf Skill"), settings.Get(SettingKeys.IncreaseBookshelfSkill), a => settings.Toggle(SettingKeys.IncreaseBookshelfSkill)));
			column4.Add(UIHelper.CreateToggle("IncreaseCourierCapacity".LocDef("Increase Courier Capacity"), settings.Get(SettingKeys.IncreaseCourierCapacity), a => settings.Toggle(SettingKeys.IncreaseCourierCapacity)));
			column4.Add(UIHelper.CreateToggle("IncreasePrintSpeed".LocDef("Increase Print Speed"), settings.Get(SettingKeys.IncreasePrintSpeed), a => settings.Toggle(SettingKeys.IncreasePrintSpeed)));
			column4.Add(UIHelper.CreateToggle("MoreHostingDeals".LocDef("More Hosting Deals"), settings.Get(SettingKeys.MoreHostingDeals), a => settings.Toggle(SettingKeys.MoreHostingDeals)));
			column4.Add(UIHelper.CreateToggle("ReduceInternetCost".LocDef("Reduce Internet Cost"), settings.Get(SettingKeys.ReduceISPCost), a => settings.Toggle(SettingKeys.ReduceISPCost)));
			column4.Add(UIHelper.CreateToggle("NoServerCost".LocDef("No Server Cost"), settings.Get(SettingKeys.NoServerCost), a => settings.Toggle(SettingKeys.NoServerCost)));
			column4.Add(UIHelper.CreateToggle("ReduceExpansionCost".LocDef("Reduce Expansion Cost"), settings.Get(SettingKeys.ReduceExpansionCost), a => settings.Toggle(SettingKeys.ReduceExpansionCost)));
			column4.Add(UIHelper.CreateToggle("DisableForcePause".LocDef("Disable Force Pause"), settings.Get(SettingKeys.DisableForcePause), a => settings.Toggle(SettingKeys.DisableForcePause)));
			column4.Add(UIHelper.CreateToggle("DisableForceFreeze".LocDef("Disable Force Freeze"), settings.Get(SettingKeys.DisableForceFreeze), a => settings.Toggle(SettingKeys.DisableForceFreeze)));
			column4.Add(UIHelper.CreateToggle("AutoAcceptHostingDeals".LocDef("Auto Accept Hosting Deals"), settings.Get(SettingKeys.AutoAcceptHostingDeals), a => settings.Toggle(SettingKeys.AutoAcceptHostingDeals)));
			column4.Add(UIHelper.CreateToggle("AutoPorting".LocDef("Auto Porting"), settings.Get(SettingKeys.AutoPorting), a => settings.Toggle(SettingKeys.AutoPorting)));
			column4.Add(UIHelper.CreateToggle("MoreInspiration".LocDef("More Inspiration"), settings.Get(SettingKeys.MoreInspiration), a => settings.Toggle(SettingKeys.MoreInspiration)));
			column4.Add(UIHelper.CreateToggle("MoreCreativity".LocDef("More Creativity"), settings.Get(SettingKeys.MoreCreativity), a => settings.Toggle(SettingKeys.MoreCreativity)));
			column4.Add(UIHelper.CreateToggle("NoMissedSupportTickets".LocDef("No Missed Support Tickets"), settings.Get(SettingKeys.NoMissedSupportTickets), a => settings.Toggle(SettingKeys.NoMissedSupportTickets)));
			column4.Add(UIHelper.CreateToggle("NoInsuranceCost".LocDef("No Insurance Cost"), settings.Get(SettingKeys.NoInsuranceCost), a => settings.Toggle(SettingKeys.NoInsuranceCost)));
			column4.Add(UIHelper.CreateToggle("NoLoanInterest".LocDef("No Loan Interest"), settings.Get(SettingKeys.NoLoanInterest), a => settings.Toggle(SettingKeys.NoLoanInterest)));
			column4.Add(UIHelper.CreateToggle("FreeMarketing".LocDef("Free Marketing"), settings.Get(SettingKeys.FreeMarketing), a => settings.Toggle(SettingKeys.FreeMarketing)));
			column4.Add(UIHelper.CreateToggle("NoOffshoreHeat".LocDef("No Offshore Heat"), settings.Get(SettingKeys.NoOffshoreHeat), a => settings.Toggle(SettingKeys.NoOffshoreHeat)));
			column4.Add(UIHelper.CreateToggle("NoFounderDividends".LocDef("No Founder Dividends"), settings.Get(SettingKeys.NoFounderDividends), a => settings.Toggle(SettingKeys.NoFounderDividends)));

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
			column6.Add(UIHelper.CreateToggle("Experimental".LocDef("Experimental"), settings.Get(SettingKeys.Experimental), a => settings.Toggle(SettingKeys.Experimental)));
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
	}
}