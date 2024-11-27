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
				Window.GetComponentsInChildren<Button>()
				  .SingleOrDefault(x => x.name == "CloseButton")
				  .onClick.AddListener(() => Shown = false);
			}

			bool experimental = Helpers.GetProperty(settings, "Experimental") || Helpers.IsDebug;

			List<GameObject> column1 = new List<GameObject>();
			List<GameObject> column2 = new List<GameObject>();
			List<GameObject> column3 = new List<GameObject>();
			List<GameObject> column4 = new List<GameObject>();
			List<GameObject> column5 = new List<GameObject>();
			List<GameObject> column6 = new List<GameObject>();

			#region column1

			column1.Add(UIHelper.CreateButton("AddMoney".LocDef("Add Money"), TrainerBehaviour.IncreaseMoney));
			column1.Add(UIHelper.CreateButton("MaxFollowers".LocDef("Max Followers"), TrainerBehaviour.MaxFollowers));
			column1.Add(UIHelper.CreateLabel());
			column1.Add(UIHelper.CreateInputBox("ProductName".LocDef("Product Name Here"), boxText => Helpers.ProductPriceName = boxText));
			column1.Add(UIHelper.CreateLabel());
			column1.Add(UIHelper.CreateButton("TakeoverCompany".LocDef("Takeover Company"), TrainerBehaviour.TakeoverCompany));
			column1.Add(UIHelper.CreateLabel());

			column1.Add(UIHelper.CreateButton("BankruptAll".LocDef("AI Bankrupt All"), TrainerBehaviour.AIBankrupt));
			column1.Add(UIHelper.CreateButton("DaysPerMonth".LocDef("Days per month"), TrainerBehaviour.MonthDays));
			column1.Add(UIHelper.CreateButton("ClearAllLoans".LocDef("Clear all loans"), TrainerBehaviour.ClearLoans));
			column1.Add(UIHelper.CreateButton("MaxMarketRecognition".LocDef("Max market recognition"), TrainerBehaviour.MaxMarketRecognition));
			column1.Add(UIHelper.CreateButton("MaxSkill".LocDef("Max Skill of employees"), TrainerBehaviour.EmployeesToMax));
			//column1.Add(UIHelper.CreateButton("RemoveProducts".LocDef("Remove Products"), TrainerBehaviour.RemoveSoft));
			column1.Add(UIHelper.CreateButton("ResetAge".LocDef("Reset age of employees"), TrainerBehaviour.ResetAgeOfEmployees));
			column1.Add(UIHelper.CreateButton("SellProductsStock".LocDef("Sell products stock"), TrainerBehaviour.SellProductStock));
			column1.Add(UIHelper.CreateButton("UnlockAllFurniture".LocDef("Unlock all furniture"), TrainerBehaviour.UnlockFurniture));
			column1.Add(UIHelper.CreateButton("UnlockAllSpace".LocDef("Unlock all space"), TrainerBehaviour.UnlockAllSpace));
			column1.Add(UIHelper.CreateButton("ExtendDeadline".LocDef("Extend Deadline"), TrainerBehaviour.ExtendDeadline));
			column1.Add(UIHelper.CreateButton("UnlockAndClaimRewards".LocDef("Unlock and Claim Rewards"), TrainerBehaviour.UnlockAndClaimAllRewards));

			#endregion

			#region column2

			column2.Add(UIHelper.CreateButton("MaxReputation".LocDef("Max Reputation"), TrainerBehaviour.MaxReputation));
			column2.Add(UIHelper.CreateButton("FixBugs".LocDef("Fix Bugs"), TrainerBehaviour.FixBugs));
			column2.Add(UIHelper.CreateLabel());
			column2.Add(UIHelper.CreateButton("SetProductPrice".LocDef("Set Product Price"), TrainerBehaviour.SetProductPrice));
			column2.Add(UIHelper.CreateLabel());
			column2.Add(UIHelper.CreateButton("SubsidiaryCompany".LocDef("Subsidiary Company"), TrainerBehaviour.SubDCompany));
			column2.Add(UIHelper.CreateLabel());

			column2.Add(UIHelper.CreateToggle("DisableNeeds".LocDef("Disable Needs"), settings.Get("NoNeeds"), a => settings.Toggle("NoNeeds")));
			column2.Add(UIHelper.CreateToggle("DisableStress".LocDef("Disable Stress"), settings.Get("NoStress"), a => settings.Toggle("NoStress")));
			column2.Add(UIHelper.CreateToggle("FreeEmployees".LocDef("Free Employees"), settings.Get("FreeEmployees"), a => settings.Toggle("FreeEmployees")));
			column2.Add(UIHelper.CreateToggle("FreeStaff".LocDef("Free Staff"), settings.Get("FreeStaff"), a => settings.Toggle("FreeStaff")));
			column2.Add(UIHelper.CreateToggle("FullSatisfaction".LocDef("Full Satisfaction"), settings.Get("FullSatisfaction"), a => settings.Toggle("FullSatisfaction")));
			column2.Add(UIHelper.CreateToggle("LockAge".LocDef("Lock Age of Employees"), settings.Get("LockAge"), a => settings.Toggle("LockAge")));
			column2.Add(UIHelper.CreateToggle("NoVacation".LocDef("No Vacation"), settings.Get("NoVacation"), a => settings.Toggle("NoVacation")));
			column2.Add(UIHelper.CreateToggle("NoSickness".LocDef("No Sickness"), settings.Get("NoSickness"), a => settings.Toggle("NoSickness")));
			column2.Add(UIHelper.CreateToggle("AutoResearchStart".LocDef("Auto Research Start"), settings.Get("AutoResearchStart"), a => settings.Toggle("AutoResearchStart")));
			column2.Add(UIHelper.CreateToggle("DigitalDistributionMonopol".LocDef("Digital Distribution Monopol"), settings.Get("DigitalDistributionMonopol"), a => settings.Toggle("DigitalDistributionMonopol")));
			column2.Add(UIHelper.CreateToggle("DisableFireInspection".LocDef("Disable Fire Inspection"), settings.Get("DisableFireInspection"), a => settings.Toggle("DisableFireInspection")));

			#endregion

			#region column3

			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateButton("SetProductStock".LocDef("Set Product Stock"), TrainerBehaviour.SetProductStock));
			column3.Add(UIHelper.CreateLabel());
			column3.Add(UIHelper.CreateButton("Bankrupt".LocDef("Bankrupt"), TrainerBehaviour.ForceBankrupt));
			column3.Add(UIHelper.CreateLabel());

			column3.Add(UIHelper.CreateToggle("FullEnvironment".LocDef("Full Environment"), settings.Get("FullEnvironment"), a => settings.Toggle("FullEnvironment")));
			column3.Add(UIHelper.CreateToggle("FullSunLight".LocDef("Full Sun Light"), settings.Get("FullRoomBrightness"), a => settings.Toggle("FullRoomBrightness")));
			column3.Add(UIHelper.CreateToggle("LockTemperature".LocDef("Lock Temperature To 21"), settings.Get("TemperatureLock"), a => settings.Toggle("TemperatureLock")));
			column3.Add(UIHelper.CreateToggle("NoMaintenance".LocDef("No Maintenance"), settings.Get("NoMaintenance"), a => settings.Toggle("NoMaintenance")));
			column3.Add(UIHelper.CreateToggle("NoiseReduction".LocDef("Noise Reduction"), settings.Get("NoiseReduction"), a => settings.Toggle("NoiseReduction")));
			column3.Add(UIHelper.CreateToggle("RoomsNeverDirty".LocDef("Rooms Never Dirty"), settings.Get("CleanRooms"), a => settings.Toggle("CleanRooms")));
			column3.Add(UIHelper.CreateToggle("NoEducationCost".LocDef("No Education Cost"), settings.Get("NoEducationCost"), a => settings.Toggle("NoEducationCost")));
			column3.Add(UIHelper.CreateToggle("DisableFires".LocDef("Disable Fires"), settings.Get("DisableFires"), a => settings.Toggle("DisableFires")));
			column3.Add(UIHelper.CreateToggle("AutoDesignEnd".LocDef("Auto Design End"), settings.Get("AutoEndDesign"), a => settings.Toggle("AutoEndDesign")));
			column3.Add(UIHelper.CreateToggle("AutoResearchEnd".LocDef("Auto Research End"), settings.Get("AutoEndResearch"), a => settings.Toggle("AutoEndResearch")));
			column3.Add(UIHelper.CreateToggle("AutoPatentEnd".LocDef("Auto Patent End"), settings.Get("AutoEndPatent"), a => settings.Toggle("AutoEndPatent")));
			column3.Add(UIHelper.CreateToggle("IncreaseWalkSpeed".LocDef("Increase Walk Speed"), settings.Get("IncreaseWalkSpeed"), a => settings.Toggle("IncreaseWalkSpeed")));
			column3.Add(UIHelper.CreateToggle("DisableFurnitureStealing".LocDef("Disable Furniture Stealing"), settings.Get("DisableFurnitureStealing"), a => settings.Toggle("DisableFurnitureStealing")));

			#endregion

			#region column4

			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateButton("SetActiveUsers".LocDef("Set Active Users"), TrainerBehaviour.AddActiveUsers));
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());
			column4.Add(UIHelper.CreateLabel());

			column4.Add(UIHelper.CreateToggle("FreePrint".LocDef("Free Print"), settings.Get("FreePrint"), a => settings.Toggle("FreePrint")));
			column4.Add(UIHelper.CreateToggle("FreeWaterElectricity".LocDef("Free Water & Electricity"), settings.Get("NoWaterElectricity"), a => settings.Toggle("NoWaterElectricity")));
			column4.Add(UIHelper.CreateToggle("IncreaseBookshelfSkill".LocDef("Increase Bookshelf Skill"), settings.Get("IncreaseBookshelfSkill"), a => settings.Toggle("IncreaseBookshelfSkill")));
			column4.Add(UIHelper.CreateToggle("IncreaseCourierCapacity".LocDef("Increase Courier Capacity"), settings.Get("IncreaseCourierCapacity"), a => settings.Toggle("IncreaseCourierCapacity")));
			column4.Add(UIHelper.CreateToggle("IncreasePrintSpeed".LocDef("Increase Print Speed"), settings.Get("IncreasePrintSpeed"), a => settings.Toggle("IncreasePrintSpeed")));
			column4.Add(UIHelper.CreateToggle("MoreHostingDeals".LocDef("More Hosting Deals"), settings.Get("MoreHostingDeals"), a => settings.Toggle("MoreHostingDeals")));
			column4.Add(UIHelper.CreateToggle("ReduceInternetCost".LocDef("Reduce Internet Cost"), settings.Get("ReduceISPCost"), a => settings.Toggle("ReduceISPCost")));
			column4.Add(UIHelper.CreateToggle("NoServerCost".LocDef("No Server Cost"), settings.Get("NoServerCost"), a => settings.Toggle("NoServerCost")));
			column4.Add(UIHelper.CreateToggle("ReduceExpansionCost".LocDef("Reduce Expansion Cost"), settings.Get("ReduceExpansionCost"), a => settings.Toggle("ReduceExpansionCost")));
			column4.Add(UIHelper.CreateToggle("ReduceBoxPrice".LocDef("Reduce Box Price"), settings.Get("ReduceBoxPrice"), a => settings.Toggle("ReduceBoxPrice")));
			column4.Add(UIHelper.CreateToggle("DisableForcePause".LocDef("Disable Force Pause"), settings.Get("DisableForcePause"), a => settings.Toggle("DisableForcePause")));
			column4.Add(UIHelper.CreateToggle("DisableForceFreeze".LocDef("Disable Force Freeze"), settings.Get("DisableForceFreeze"), a => settings.Toggle("DisableForceFreeze")));
			column4.Add(UIHelper.CreateToggle("AutoAcceptHostingDeals".LocDef("Auto Accept Hosting Deals"), settings.Get("AutoAcceptHostingDeals"), a => settings.Toggle("AutoAcceptHostingDeals")));

			#endregion

			#region column5

			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateButton("Discord".LocDef("DISCORD"), () => TrainerBehaviour.ShowDiscordInvite()));
			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateLabel());
			column5.Add(UIHelper.CreateLabel());

			column5.Add(UIHelper.CreateLabel("Efficiency"));
			var efficiencyComboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, "EfficiencyStore", 2)
			);
			efficiencyComboBox.OnSelectedChanged.AddListener(() => storesSettings.Set("EfficiencyStore", efficiencySelectItems.GetAt(efficiencyComboBox.Selected).Value));
			column5.Add(efficiencyComboBox.gameObject);

			column5.Add(UIHelper.CreateLabel("Lead Efficiency"));
			var leadEfficiencyComboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, "LeadEfficiencyStore", 2)
			);
			leadEfficiencyComboBox.OnSelectedChanged.AddListener(() => storesSettings.Set("LeadEfficiencyStore", efficiencySelectItems.GetAt(leadEfficiencyComboBox.Selected).Value));
			column5.Add(leadEfficiencyComboBox.gameObject);

			#endregion

			#region column6
			column6.Add(UIHelper.CreateToggle("Experimental".LocDef("Experimental"), settings.Get("Experimental"), a => settings.Toggle("Experimental")));
			column6.Add(UIHelper.CreateLabel("After toggle re-open the window"));
			column6.Add(UIHelper.CreateLabel());

			if (experimental)
			{
				bool isOn = false;
				column6.Add(UIHelper.CreateToggle("TestToggle".LocDef("Test Toggle"), isOn, a => isOn = !isOn));
				column6.Add(UIHelper.CreateButton("TestButton".LocDef("Test Button"), TrainerBehaviour.TestButton));
				column6.Add(UIHelper.CreateToggle("MoreInspiration".LocDef("More Inspiration [TEST]"), settings.Get("MoreInspiration"), a => settings.Toggle("MoreInspiration")));
				column6.Add(UIHelper.CreateToggle("MoreCreativity".LocDef("More Creativity [TEST]"), settings.Get("MoreCreativity"), a => settings.Toggle("MoreCreativity")));
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