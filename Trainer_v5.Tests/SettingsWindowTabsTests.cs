using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// The game's UI types cannot be built in the test host, so the tab navigation of the Trainer Settings window (#449) is
	// checked by the shape of its source: what is built once, what a tab switch may do, what a toggle handler guards.
	public class SettingsWindowTabsTests
	{
		private static readonly string[] UiFiles = { "SettingsWindow.cs", "SettingActionRows.cs", "SettingToggleRows.cs", "SettingsWindowState.cs", "TrainerDomain.cs" };

		private static string Window()
		{
			return SourceCode.Read("SettingsWindow.cs");
		}

		[Fact]
		public void SwitchingTabsOnlyShowsAndHidesWhatInitBuilt()
		{
			var source = Window();
			var switchTab = SourceCode.Body(source, "private static void SetActiveTab(");
			var mark = SourceCode.Body(source, "private static void MarkTab(");

			Assert.Contains(".SetActive(", switchTab);
			foreach (var body in new[] { switchTab, mark })
			{
				Assert.DoesNotContain("AddListener", body);
				Assert.DoesNotContain("Create", body);
				Assert.DoesNotContain("Spawn", body);
				Assert.DoesNotContain("Features.", body);
				Assert.DoesNotContain("Helpers.", body);
			}
		}

		[Fact]
		public void TheWindowIsBuiltOnceAndReopeningOnlyTogglesAndRefreshesIt()
		{
			var source = Window();
			var toggle = SourceCode.Body(source, "public static void Toggle()");

			Assert.Equal(1, Count(source, "Init();"));
			SourceCode.AssertInOrder(toggle, "if (Window == null) { Init(); }", "Window.Toggle();", "built only when there is no window");
			Assert.Equal(1, Count(source, "BuildTabStrip();"));
			Assert.Equal(1, Count(source, "UIHelper.SpawnWindow("));
			foreach (var body in new[] { toggle, SourceCode.Body(source, "private static void RefreshFromSettings()") })
			{
				Assert.DoesNotContain("AddListener", body);
				Assert.DoesNotContain("Spawn", body);
				Assert.DoesNotContain("Create", body);
			}
		}

		[Fact]
		public void EveryListenerIsAddedWhileTheWindowIsBuilt()
		{
			var source = Window();
			var allowed = new[] { "private static void Init()", "private static void BuildEfficiency(" };
			var inAllowed = 0;
			foreach (var marker in allowed)
			{
				inAllowed += Count(SourceCode.Body(source, marker), "AddListener");
			}

			Assert.Equal(Count(source, "AddListener"), inAllowed);
			Assert.True(Count(source, "AddListener") >= 1);
		}

		[Fact]
		public void ATabButtonOnlySwitchesTheTab()
		{
			var source = Window();
			var create = SourceCode.Body(source, "private static Button CreateTabButton(");
			var strip = SourceCode.Body(source, "private static void BuildTabStrip()");

			Assert.Contains("() => SetActiveTab(tab)", create);
			Assert.DoesNotContain("Features.", create);
			Assert.DoesNotContain("Settings.", create);
			Assert.Contains("\"TabActions\".LocDef(\"Actions\")", strip);
			Assert.Contains("\"TabToggles\".LocDef(\"Toggles\")", strip);
			Assert.Contains("\"TabSettings\".LocDef(\"Settings\")", strip);
		}

		[Fact]
		public void AToggleChangeRaisedByARefreshDoesNotFlipTheSetting()
		{
			var source = Window();
			var handler = SourceCode.Body(source, "private static void OnToggleChanged(");
			var refresh = SourceCode.Body(source, "private static void RefreshFromSettings()");

			SourceCode.AssertInOrder(handler, "if (_refreshing) { return; }", "settings.Toggle(key);", "the guard comes before the flip");
			SourceCode.AssertInOrder(refresh, "_refreshing = true;", ".isOn =", "set while the controls change");
			SourceCode.AssertInOrder(refresh, ".isOn =", "finally { _refreshing = false; }", "cleared even when a control throws");
			Assert.Contains("UpdateSelectionSupressEvents(", source);
			Assert.DoesNotContain(".UpdateSelection(", SourceCode.Body(source, "private static void RefreshEfficiency("));
			Assert.DoesNotContain("AddListener", refresh);
		}

		[Fact]
		public void EveryToggleIsCreatedThroughOneHandlerThatKeepsItsKey()
		{
			var source = Window();
			var create = SourceCode.Body(source, "private static GameObject CreateSettingToggle(");

			Assert.Equal(1, Count(source, "UIHelper.CreateToggle("));
			Assert.Contains("a => OnToggleChanged(key)", create);
			Assert.Contains("_toggleBindings.Add(new ToggleBinding(", create);
			Assert.Contains("rows.AddRange(SettingToggleRows.CreateImmediate());", SourceCode.Body(source, "private static void BuildToggles("));
		}

		[Fact]
		public void TheImmediateTogglesStillApplyTheirEffectAtOnce()
		{
			var handler = SourceCode.Body(Window(), "private static void OnToggleChanged(");

			Assert.Contains("key == SettingKeys.UnlimitedSubsidiaries", handler);
			Assert.Contains("CompanyFeatures.ApplyUnlimitedSubsidiaries(enabled);", handler);
			Assert.Contains("key == SettingKeys.ForceLights", handler);
			Assert.Contains("EnvironmentFeatures.ApplyForceLights(enabled);", handler);
			Assert.Contains("key == SettingKeys.ShowRoomCeilings", handler);
			Assert.Contains("EnvironmentFeatures.ApplyShowRoomCeilings(enabled);", handler);

			foreach (var row in SettingToggleRows.CreateImmediate())
			{
				Assert.Contains("key == SettingKeys." + row.Key.Key, handler);
			}
		}

		[Fact]
		public void TheWindowCallsNoFeatureExceptTheThreeImmediateToggleEffects()
		{
			var calls = new SortedSet<string>();
			foreach (Match match in Regex.Matches(Window(), @"\b(?<feature>\w+Features)\.(?<method>\w+)"))
			{
				calls.Add(match.Groups["feature"].Value + "." + match.Groups["method"].Value);
			}

			Assert.Equal(new[] { "CompanyFeatures.ApplyUnlimitedSubsidiaries", "EnvironmentFeatures.ApplyForceLights", "EnvironmentFeatures.ApplyShowRoomCeilings" }, new List<string>(calls));
		}

		[Fact]
		public void EveryButtonComesFromTheActionTableOrIsATabAndEveryToggleFromTheToggleTable()
		{
			var source = Window();

			Assert.Equal(1, Count(source, "UIHelper.CreateButton("));
			Assert.Equal(1, Count(source, "UIFactory.Button("));
			Assert.Contains("SettingActionRows.Create()", SourceCode.Body(source, "private static void BuildActions("));
			Assert.Contains("SettingToggleRows.Create()", SourceCode.Body(source, "private static void BuildToggles("));
			Assert.Equal(0, Count(source, "SpawnButton"));
			Assert.Equal(0, Count(source, "SpawnCheckbox"));
		}

		[Fact]
		public void TheSettingsTabHoldsTheEfficiencyDropDownsAndTheTableButtonsOfThatTab()
		{
			var source = Window();
			var actions = SourceCode.Body(source, "private static void BuildActions(");
			var efficiency = SourceCode.Body(source, "private static void BuildEfficiency(");

			Assert.Contains("row.Tab == SettingsTab.Settings", actions);
			Assert.Contains("SettingKeys.EfficiencyStore", efficiency);
			Assert.Contains("SettingKeys.LeadEfficiencyStore", efficiency);
			Assert.Equal(2, Count(source, "UIHelper.CreateComboBox("));
			Assert.Contains("PlaceSettingsPage(settingsPage.Content, settingsCells);", SourceCode.Body(source, "private static void Init()"));
		}

		[Fact]
		public void TheProductNameFieldIsTheFirstControlOfTheProductSection()
		{
			var actions = SourceCode.Body(Window(), "private static void BuildActions(");

			SourceCode.AssertInOrder(actions, "UIHelper.CreateInputBox(", "SettingActionRows.Create()", "the field leads the section");
			Assert.Contains("actionCells[(int)TrainerDomain.Product].Add(UIHelper.CreateInputBox(", actions);
			Assert.Contains("Helpers.ProductPriceName = boxText", actions);
		}

		[Fact]
		public void ResetDropsEveryControlReferenceButKeepsTheRememberedTab()
		{
			var reset = SourceCode.Body(Window(), "public static void Reset()");

			foreach (var statement in new[] { "Window = null;", "Shown = false;", "_tabButtons = null;", "_tabPages = null;", "_toggleBindings = null;", "_efficiencyComboBox = null;", "_leadEfficiencyComboBox = null;" })
			{
				Assert.Contains(statement, reset);
			}

			Assert.DoesNotContain("_activeTab", reset);
			SourceCode.AssertInOrder(Window(), "public static void Reset()", "private static void Init()", "PerGameStateResetTests reads between them");
		}

		[Fact]
		public void TheRememberedTabLivesInTheRunningGameOnly()
		{
			var source = Window();

			Assert.Contains("private static SettingsTab _activeTab;", source);
			Assert.DoesNotContain("SaveSetting", source);
			Assert.DoesNotContain("SettingKeys.Experimental", source);
			Assert.Equal(0, Count(SourceCode.Read("Main.cs"), "_activeTab"));
		}

		[Fact]
		public void TheWindowHeightReservesTheTabStripAndIsTheSameOnEveryTab()
		{
			var init = SourceCode.Body(Window(), "private static void Init()");

			Assert.Contains("UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, 1)", init);
			Assert.Contains("Window.SetWindowSize(visibleRows + 1, SettingsWindowState.WindowWidth);", init);
			Assert.Equal(3, Count(init, "CreateTabPage("));
		}

		[Fact]
		public void ControlsThatBelongElsewhereAreNotInTheTabs()
		{
			foreach (var file in UiFiles)
			{
				var source = SourceCode.Read(file);

				Assert.DoesNotContain("ExperimentalOptions", source);
				Assert.DoesNotContain("\"Experimental\"", source);
				Assert.DoesNotContain("SettingKeys.Experimental", source);
				Assert.DoesNotContain("IsDebug", source);
				Assert.DoesNotContain("testButton", source);
				Assert.DoesNotContain("EmployeeTraitChangeWindow", source);
				Assert.DoesNotContain("EmployeeDemandChangeWindow", source);
				Assert.DoesNotContain("EmployeeLeadSpecChangeWindow", source);
				Assert.DoesNotContain("EmployeeSkillChangeWindow", source);
				Assert.DoesNotContain("DetailWindowTrainer", source);
				Assert.DoesNotContain("FounderToggle", source);
				foreach (var retired in new[] { "DisableSkillDecay", "ReduceBoxPrice", "AutoMaxMarketShare", "MoreCreativity", "ReduceExpansionCost" })
				{
					Assert.DoesNotContain(retired, source);
				}
			}
		}

		private static int Count(string source, string text)
		{
			var count = 0;
			var index = 0;
			while ((index = source.IndexOf(text, index, System.StringComparison.Ordinal)) >= 0)
			{
				count++;
				index += text.Length;
			}

			return count;
		}
	}
}
