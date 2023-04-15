using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils = Trainer_v5.Utilities;

namespace Trainer_v5
{
	public class SettingsWindow : MonoBehaviour
	{
		private static readonly string _title = $"Trainer Settings, by Trawis (v{Helpers.Version})";

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
			var stores = Helpers.Stores;
			var efficiencyValues = Helpers.EfficiencyValues;

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

      List<GameObject> column1 = new List<GameObject>();
			List<GameObject> column2 = new List<GameObject>();
			List<GameObject> column3 = new List<GameObject>();
			List<GameObject> column4 = new List<GameObject>();
			List<GameObject> column5 = new List<GameObject>();

			#region column1

			Utils.AddButton("AddMoney".LocDef("Add Money"), TrainerBehaviour.IncreaseMoney, column1);
			Utils.AddButton("MaxFollowers".LocDef("Max Followers"), TrainerBehaviour.MaxFollowers, column1);
			Utils.AddEmptyBox(column1);
			Utils.AddInputBox("ProductName".LocDef("Product Name Here"), boxText => Helpers.ProductPriceName = boxText, column1);
			Utils.AddEmptyBox(column1);
			Utils.AddButton("TakeoverCompany".LocDef("Takeover Company"),TrainerBehaviour.TakeoverCompany, column1);
			Utils.AddEmptyBox(column1);
			Utils.AddButton("BankruptAll".LocDef("AI Bankrupt All"), TrainerBehaviour.AIBankrupt, column1);
			Utils.AddButton("DaysPerMonth".LocDef("Days per month"), TrainerBehaviour.MonthDays, column1);
			Utils.AddButton("ClearAllLoans".LocDef("Clear all loans"), TrainerBehaviour.ClearLoans, column1);
			Utils.AddButton("MaxMarketRecognition".LocDef("Max market recognition"), TrainerBehaviour.MaxMarketRecognition, column1);
			Utils.AddButton("MaxSkill".LocDef("Max Skill of employees"), TrainerBehaviour.EmployeesToMax, column1);
			Utils.AddButton("RemoveProducts".LocDef("Remove Products"), TrainerBehaviour.RemoveSoft, column1);
			Utils.AddButton("ResetAge".LocDef("Reset age of employees"), TrainerBehaviour.ResetAgeOfEmployees, column1);
			Utils.AddButton("SellProductsStock".LocDef("Sell products stock"), TrainerBehaviour.SellProductStock, column1);
			Utils.AddButton("UnlockAllFurniture".LocDef("Unlock all furniture"), TrainerBehaviour.UnlockFurniture, column1);
			Utils.AddButton("UnlockAllSpace".LocDef("Unlock all space"), TrainerBehaviour.UnlockAllSpace, column1);
			Utils.AddButton("ExtendDeadline".LocDef("Extend Deadline"), TrainerBehaviour.ExtendDeadline, column1);

			if (Helpers.IsDebug)
			{
				Utils.AddButton("Test".LocDef("Test"), TrainerBehaviour.AddQuality, column1);
			}

			#endregion

			#region column2
			Utils.AddButton("MaxReputation".LocDef("Max Reputation"), TrainerBehaviour.MaxReputation, column2);
			Utils.AddButton("FixBugs".LocDef("Fix Bugs"), TrainerBehaviour.FixBugs, column2);
			Utils.AddEmptyBox(column2);
			Utils.AddButton("SetProductPrice".LocDef("Set Product Price"), TrainerBehaviour.SetProductPrice, column2);
			Utils.AddEmptyBox(column2);
			Utils.AddButton("SubsidiaryCompany".LocDef("Subsidiary Company"), TrainerBehaviour.SubDCompany, column2);
			Utils.AddEmptyBox(column2);
			Utils.AddToggle("DisableNeeds".LocDef("Disable Needs"), Helpers.GetProperty(settings, "NoNeeds"),
					a => Helpers.SetProperty(settings, "NoNeeds", !Helpers.GetProperty(settings, "NoNeeds")), column2);
			Utils.AddToggle("DisableStress".LocDef("Disable Stress"), Helpers.GetProperty(settings, "NoStress"),
					a => Helpers.SetProperty(settings, "NoStress", !Helpers.GetProperty(settings, "NoStress")), column2);
			Utils.AddToggle("FreeEmployees".LocDef("Free Employees"), Helpers.GetProperty(settings, "FreeEmployees"),
					a => Helpers.SetProperty(settings, "FreeEmployees", !Helpers.GetProperty(settings, "FreeEmployees")), column2);
			Utils.AddToggle("FreeStaff".LocDef("Free Staff"), Helpers.GetProperty(settings, "FreeStaff"),
					a => Helpers.SetProperty(settings, "FreeStaff", !Helpers.GetProperty(settings, "FreeStaff")), column2);
			Utils.AddToggle("FullSatisfaction".LocDef("Full Satisfaction"), Helpers.GetProperty(settings, "FullSatisfaction"),
					a => Helpers.SetProperty(settings, "FullSatisfaction", !Helpers.GetProperty(settings, "FullSatisfaction")), column2);
      Utils.AddToggle("LockAge".LocDef("Lock Age of Employees"), Helpers.GetProperty(settings, "LockAge"),
          a => Helpers.SetProperty(settings, "LockAge", !Helpers.GetProperty(settings, "LockAge")), column2);
      Utils.AddToggle("NoVacation".LocDef("No Vacation"), Helpers.GetProperty(settings, "NoVacation"),
					a => Helpers.SetProperty(settings, "NoVacation", !Helpers.GetProperty(settings, "NoVacation")), column2);
			Utils.AddToggle("NoSickness".LocDef("No Sickness"), Helpers.GetProperty(settings, "NoSickness"),
					a => Helpers.SetProperty(settings, "NoSickness", !Helpers.GetProperty(settings, "NoSickness")), column2);
			Utils.AddToggle("MoreInspiration".LocDef("More Inspiration [TEST]"), Helpers.GetProperty(settings, "MoreInspiration"),
					a => Helpers.SetProperty(settings, "MoreInspiration", !Helpers.GetProperty(settings, "MoreInspiration")), column2);
			Utils.AddToggle("MoreCreativity".LocDef("More Creativity [TEST]"), Helpers.GetProperty(settings, "MoreCreativity"),
					a => Helpers.SetProperty(settings, "MoreCreativity", !Helpers.GetProperty(settings, "MoreCreativity")), column2);

			if (Helpers.IsDebug)
			{
				bool isOn = false;
				Utils.AddToggle("Test".LocDef("Test"), isOn, a => isOn = !isOn, column2);
			}

			#endregion

			#region column3
			Utils.AddEmptyBox(column3);
			Utils.AddEmptyBox(column3);
			Utils.AddEmptyBox(column3);
			Utils.AddButton("SetProductStock".LocDef("Set Product Stock"), TrainerBehaviour.SetProductStock, column3);
			Utils.AddEmptyBox(column3);
			Utils.AddButton("Bankrupt".LocDef("Bankrupt"), TrainerBehaviour.ForceBankrupt, column3);
			Utils.AddEmptyBox(column3);

			Utils.AddToggle("FullEnvironment".LocDef("Full Environment"), Helpers.GetProperty(settings, "FullEnvironment"),
					a => Helpers.SetProperty(settings, "FullEnvironment", !Helpers.GetProperty(settings, "FullEnvironment")), column3);
			Utils.AddToggle("FullSunLight".LocDef("Full Sun Light"), Helpers.GetProperty(settings, "FullRoomBrightness"),
					a => Helpers.SetProperty(settings, "FullRoomBrightness", !Helpers.GetProperty(settings, "FullRoomBrightness")), column3);
			Utils.AddToggle("LockTemperature".LocDef("Lock Temperature To 21"), Helpers.GetProperty(settings, "TemperatureLock"),
					a => Helpers.SetProperty(settings, "TemperatureLock", !Helpers.GetProperty(settings, "TemperatureLock")), column3);
			Utils.AddToggle("NoMaintenance".LocDef("No Maintenance"), Helpers.GetProperty(settings, "NoMaintenance"),
					a => Helpers.SetProperty(settings, "NoMaintenance", !Helpers.GetProperty(settings, "NoMaintenance")), column3);
			Utils.AddToggle("NoiseReduction".LocDef("Noise Reduction"), Helpers.GetProperty(settings, "NoiseReduction"),
					a => Helpers.SetProperty(settings, "NoiseReduction", !Helpers.GetProperty(settings, "NoiseReduction")), column3);
			Utils.AddToggle("RoomsNeverDirty".LocDef("Rooms Never Dirty"), Helpers.GetProperty(settings, "CleanRooms"),
					a => Helpers.SetProperty(settings, "CleanRooms", !Helpers.GetProperty(settings, "CleanRooms")), column3);
			Utils.AddToggle("NoEducationCost".LocDef("No Education Cost"), Helpers.GetProperty(settings, "NoEducationCost"),
					a => Helpers.SetProperty(settings, "NoEducationCost", !Helpers.GetProperty(settings, "NoEducationCost")), column3);
			Utils.AddToggle("DisableFires".LocDef("Disable Fires"), Helpers.GetProperty(settings, "DisableFires"),
					a => Helpers.SetProperty(settings, "DisableFires", !Helpers.GetProperty(settings, "DisableFires")), column3);
			Utils.AddToggle("AutoDesignEnd".LocDef("Auto Design End"), Helpers.GetProperty(settings, "AutoEndDesign"),
					a => Helpers.SetProperty(settings, "AutoEndDesign", !Helpers.GetProperty(settings, "AutoEndDesign")), column3);
			Utils.AddToggle("AutoResearchEnd".LocDef("Auto Research End"), Helpers.GetProperty(settings, "AutoEndResearch"),
					a => Helpers.SetProperty(settings, "AutoEndResearch", !Helpers.GetProperty(settings, "AutoEndResearch")), column3);
			Utils.AddToggle("AutoPatentEnd".LocDef("Auto Patent End"), Helpers.GetProperty(settings, "AutoEndPatent"),
					a => Helpers.SetProperty(settings, "AutoEndPatent", !Helpers.GetProperty(settings, "AutoEndPatent")), column3);
			Utils.AddToggle("IncreaseWalkSpeed".LocDef("Increase Walk Speed"), Helpers.GetProperty(settings, "IncreaseWalkSpeed"),
					a => Helpers.SetProperty(settings, "IncreaseWalkSpeed", !Helpers.GetProperty(settings, "IncreaseWalkSpeed")), column3);
			Utils.AddToggle("DisableFurnitureStealing".LocDef("Disable Furniture Stealing"), Helpers.GetProperty(settings, "DisableFurnitureStealing"),
					a => Helpers.SetProperty(settings, "DisableFurnitureStealing", !Helpers.GetProperty(settings, "DisableFurnitureStealing")), column3);

			#endregion

			#region column4

			Utils.AddEmptyBox(column4);
			Utils.AddEmptyBox(column4);
			Utils.AddEmptyBox(column4);
			Utils.AddButton("SetActiveUsers".LocDef("Set Active Users"), TrainerBehaviour.AddActiveUsers, column4);
			Utils.AddEmptyBox(column4);
			Utils.AddEmptyBox(column4);
			Utils.AddEmptyBox(column4);
			//Utils.AddToggle("Auto Distribution Deals", Helpers.GetProperty(settings, "AutoDistributionDeals"),
			//	a => Helpers.SetProperty(settings, "AutoDistributionDeals", !Helpers.GetProperty(settings, "AutoDistributionDeals")), column4);
			Utils.AddToggle("FreePrint".LocDef("Free Print"), Helpers.GetProperty(settings, "FreePrint"),
					a => Helpers.SetProperty(settings, "FreePrint", !Helpers.GetProperty(settings, "FreePrint")), column4);
			Utils.AddToggle("FreeWaterElectricity".LocDef("Free Water & Electricity"), Helpers.GetProperty(settings, "NoWaterElectricity"),
					a => Helpers.SetProperty(settings, "NoWaterElectricity", !Helpers.GetProperty(settings, "NoWaterElectricity")), column4);
			Utils.AddToggle("IncreaseBookshelfSkill".LocDef("Increase Bookshelf Skill"), Helpers.GetProperty(settings, "IncreaseBookshelfSkill"),
					a => Helpers.SetProperty(settings, "IncreaseBookshelfSkill", !Helpers.GetProperty(settings, "IncreaseBookshelfSkill")), column4);
			Utils.AddToggle("IncreaseCourierCapacity".LocDef("Increase Courier Capacity"), Helpers.GetProperty(settings, "IncreaseCourierCapacity"),
					a => Helpers.SetProperty(settings, "IncreaseCourierCapacity", !Helpers.GetProperty(settings, "IncreaseCourierCapacity")), column4);
			Utils.AddToggle("IncreasePrintSpeed".LocDef("Increase Print Speed"), Helpers.GetProperty(settings, "IncreasePrintSpeed"),
					a => Helpers.SetProperty(settings, "IncreasePrintSpeed", !Helpers.GetProperty(settings, "IncreasePrintSpeed")), column4);
			Utils.AddToggle("MoreHostingDeals".LocDef("More Hosting Deals"), Helpers.GetProperty(settings, "MoreHostingDeals"),
					a => Helpers.SetProperty(settings, "MoreHostingDeals", !Helpers.GetProperty(settings, "MoreHostingDeals")), column4);
			Utils.AddToggle("ReduceInternetCost".LocDef("Reduce Internet Cost"), Helpers.GetProperty(settings, "ReduceISPCost"),
					a => Helpers.SetProperty(settings, "ReduceISPCost", !Helpers.GetProperty(settings, "ReduceISPCost")), column4);
			Utils.AddToggle("NoServerCost".LocDef("No Server Cost"), Helpers.GetProperty(settings, "NoServerCost"),
					a => Helpers.SetProperty(settings, "NoServerCost", !Helpers.GetProperty(settings, "NoServerCost")), column4);
			Utils.AddToggle("ReduceExpansionCost".LocDef("Reduce Expansion Cost"), Helpers.GetProperty(settings, "ReduceExpansionCost"),
					a => Helpers.SetProperty(settings, "ReduceExpansionCost", !Helpers.GetProperty(settings, "ReduceExpansionCost")), column4);
			Utils.AddToggle("ReduceBoxPrice".LocDef("Reduce Box Price"), Helpers.GetProperty(settings, "ReduceBoxPrice"),
								a => Helpers.SetProperty(settings, "ReduceBoxPrice", !Helpers.GetProperty(settings, "ReduceBoxPrice")), column4);

			#endregion

			#region column5
			Utils.AddEmptyBox(column5);
			Utils.AddButton("Discord".LocDef("DISCORD"),  () => TrainerBehaviour.ShowDiscordInvite(), column5);
			Utils.AddEmptyBox(column5);
			Utils.AddEmptyBox(column5);
			Utils.AddEmptyBox(column5);
			Utils.AddEmptyBox(column5);

			GUICombobox efficiencyComboBox = Utils.AddComboBox("Efficiency", efficiencyValues, Helpers.GetIndex(efficiencyValues, stores, "EfficiencyStore", 2), column5);
			efficiencyComboBox.OnSelectedChanged.AddListener(() => Helpers.SetProperty(stores, "EfficiencyStore", efficiencyValues[efficiencyComboBox.Selected].Value));

			GUICombobox leadEfficiencyComboBox = Utils.AddComboBox("Lead Efficiency", Helpers.EfficiencyValues, Helpers.GetIndex(efficiencyValues, stores, "LeadEfficiencyStore", 2), column5);
			leadEfficiencyComboBox.OnSelectedChanged.AddListener(() => Helpers.SetProperty(stores, "LeadEfficiencyStore", efficiencyValues[leadEfficiencyComboBox.Selected].Value));

			#endregion

			Utils.CreateGameObjects(Constants.FIRST_COLUMN, column1.ToArray(), Window);
			Utils.CreateGameObjects(Constants.SECOND_COLUMN, column2.ToArray(), Window);
			Utils.CreateGameObjects(Constants.THIRD_COLUMN, column3.ToArray(), Window);
			Utils.CreateGameObjects(Constants.FOURTH_COLUMN, column4.ToArray(), Window);
			Utils.CreateGameObjects(Constants.FIFTH_COLUMN, column5.ToArray(), Window, isComboBox: true);

			int[] columnsCount = new int[]
			{
				column1.Count(), column2.Count(), column3.Count(), column4.Count(), column5.Count()
			};

			Utils.SetWindowSize(columnsCount.Max(), Constants.X_SETTINGS_WINDOW, Window);
		}
	}
}