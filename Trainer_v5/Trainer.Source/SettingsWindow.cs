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

			var standardToggles = SettingToggleRows.Create();

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
			column1.Add(UIHelper.CreateInputBox(SettingsWindowState.ProductNameFieldText(Helpers.ProductPriceName, "ProductName".LocDef("Product Name Here")), boxText => Helpers.ProductPriceName = boxText));
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

			AddStandardToggles(column2, standardToggles, 2, settings);
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

			AddStandardToggles(column3, standardToggles, 3, settings);
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

			AddStandardToggles(column4, standardToggles, 4, settings);

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
			bool testIsOn = false;
			var testToggle = UIHelper.CreateToggle("TestToggle".LocDef("Test Toggle"), testIsOn, a => testIsOn = !testIsOn);
			var testButton = UIHelper.CreateButton("TestButton".LocDef("Test Button"), WorkItemFeatures.TestButton);
			var experimentalToggle = UIHelper.CreateToggle("Experimental".LocDef("Experimental"), settings.Get(SettingKeys.Experimental), a =>
			{
				settings.Toggle(SettingKeys.Experimental);
				SetTestControlsActive(settings, testToggle, testButton);
			});

			column6.Add(experimentalToggle);
			column6.Add(UIHelper.CreateLabel("Shows the Test controls below"));
			column6.Add(UIHelper.CreateLabel());
			column6.Add(testToggle);
			column6.Add(testButton);

			#endregion

			column1.AddToWindow(Window, Constants.FIRST_COLUMN);
			column2.AddToWindow(Window, Constants.SECOND_COLUMN);
			column3.AddToWindow(Window, Constants.THIRD_COLUMN);
			column4.AddToWindow(Window, Constants.FOURTH_COLUMN);
			column5.AddToWindow(Window, Constants.FIFTH_COLUMN, isComboBox: true);
			column6.AddToWindow(Window, Constants.SIXTH_COLUMN);

			int maxRowsCount = new[] { column1.Count, column2.Count, column3.Count, column4.Count, column5.Count, column6.Count }.Max();
			Window.SetWindowSize(maxRowsCount, Constants.X_SETTINGS_WINDOW);
			SetTestControlsActive(settings, testToggle, testButton);
		}

		private static void SetTestControlsActive(Dictionary<string, bool> settings, GameObject testToggle, GameObject testButton)
		{
			bool experimental = Helpers.GetProperty(settings, SettingKeys.Experimental) || Helpers.IsDebug;
			testToggle.SetActive(experimental);
			testButton.SetActive(experimental);
		}

		private static void AddStandardToggles(List<GameObject> column, List<SettingToggleRow> rows, int columnNumber, Dictionary<string, bool> settings)
		{
			foreach (var row in rows)
			{
				if (row.Column == columnNumber)
				{
					column.Add(CreateSettingToggle(settings, row.LocKey, row.DefaultText, row.Key));
				}
			}
		}

		private static GameObject CreateSettingToggle(Dictionary<string, bool> settings, string locKey, string defaultText, ToggleKey key)
		{
			return UIHelper.CreateToggle(locKey.LocDef(defaultText), settings.Get(key), a => settings.Toggle(key));
		}
	}
}
