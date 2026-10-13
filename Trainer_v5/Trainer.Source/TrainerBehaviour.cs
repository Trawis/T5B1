using System;
using System.Collections.Generic;
using System.Linq;
using OrbCreationExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace Trainer_v5
{
	public class TrainerBehaviour : ModBehaviour
	{
		private bool _sceneEventsSubscribed;
		private bool _timeEventsSubscribed;

		private readonly ToggleStateTracker _toggleStates = new ToggleStateTracker();

		// TimeOfDay has no minute-passed event; this raises one from its Minute field.
		private readonly GameMinuteWatcher _minuteWatcher = new GameMinuteWatcher();

		private readonly Dictionary<EnvironmentPreset, float> _ispCostBaselines = new Dictionary<EnvironmentPreset, float>();

		private float GetDefaultISPCostFactor()
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

		private void RestoreReduceISPCost()
		{
			Settings.Environment.ISPCostFactor = GetDefaultISPCostFactor();
		}

		private static void RestoreIncreaseCourierCapacity()
		{
			AI.MaxBoxes = Constants.MAX_BOXES_DEFAULT;
			AI.MaxBoxCarry = Constants.MAX_CARRY_DEFAULT;
		}

		private static void RestoreReduceExpansionCost()
		{
			Settings.ExpansionCost = Constants.EXPANSION_COST;
		}

		// Settings the game itself only ever sets once (ctor/new-game/save-load).
		private bool _oneTimeSettingsApplied;
		private bool _forceLightsApplied;
		private bool _showRoomCeilingsApplied;
		private bool _unlimitedSubsidiariesApplied;

		private static GameSettings Settings => GameSettings.Instance;
		private static Dictionary<string, bool> TrainerSettings => Helpers.Settings;
		private static Dictionary<string, object> StoresSettings => Helpers.StoresSettings;

		private void Start()
		{
			Helpers.Random = new Random();

			if (!isActiveAndEnabled)
			{
				return;
			}

			SubscribeToSceneEvents();
		}

		private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
		{
			if (isActiveAndEnabled)
			{
				switch (scene.name)
				{
					case "MainMenu":
						if (Main.TrainerButton != null)
						{
							Destroy(Main.TrainerButton.gameObject);
							Main.TrainerButton = null;
						}

						if (Main.SkillChangeButton != null)
						{
							Destroy(Main.SkillChangeButton.gameObject);
							Main.SkillChangeButton = null;
						}

						DetailWindowTrainer.Reset();
						UnsubscribeFromEvents();
						break;
					case "MainScene":
						Main.CreateUIButtons();
						// Immediate attempt; GameSettings.GameReady below is the reliable trigger.
						DetailWindowTrainer.Install();
						SubscribeToEvents();
						break;
					case "Customization":
						ActorCustomization.StartYears = new[] { 1970, 1975, 1980, 1985, 1990, 1995, 2000, 2005, 2010, 2015, 2020, 2025, 2030, 2035, 2040, 2045, 2050, 2060, 2070, 2080, 2090, 2100 };
						ActorCustomization.StartLoans = new[] { 0, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000, 5000000, 10000000 };
						break;
					default:
						goto case "MainMenu";
				}
			}
		}

		private void SubscribeToSceneEvents()
		{
			if (_sceneEventsSubscribed)
			{
				return;
			}

			SceneManager.sceneLoaded += OnLevelFinishedLoading;
			_sceneEventsSubscribed = true;
		}

		private void UnsubscribeFromSceneEvents()
		{
			if (!_sceneEventsSubscribed)
			{
				return;
			}

			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
			_sceneEventsSubscribed = false;
		}

		private void SubscribeToEvents()
		{
			if (_timeEventsSubscribed)
			{
				return;
			}

			TimeOfDay.OnHourPassed += OnHourPassed;
			TimeOfDay.OnDayPassed += OnDayPassed;
			TimeOfDay.OnMonthPassed += OnMonthPassed;
			_minuteWatcher.OnMinutePassed += OnMinutePassed;
			_minuteWatcher.Reset();

			// Fires once per load, after HUD.DetailWindow already exists.
			GameSettings.GameReady += OnGameReady;

			_timeEventsSubscribed = true;
		}

		private void UnsubscribeFromEvents()
		{
			if (!_timeEventsSubscribed)
			{
				return;
			}

			TimeOfDay.OnHourPassed -= OnHourPassed;
			TimeOfDay.OnDayPassed -= OnDayPassed;
			TimeOfDay.OnMonthPassed -= OnMonthPassed;
			_minuteWatcher.OnMinutePassed -= OnMinutePassed;
			GameSettings.GameReady -= OnGameReady;

			_timeEventsSubscribed = false;
		}

		private void OnGameReady(object sender, EventArgs args)
		{
			DetailWindowTrainer.Install();
		}

		private void OnHourPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.MoreHostingDeals))
			{
				Helpers.TryExecute("MoreHostingDeals", ApplyHostingDealTiming);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreeStaff))
			{
				Helpers.TryExecute("FreeStaff", () => Settings.StaffSalaryDue = 0f);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoServerCost))
			{
				Helpers.TryExecute("NoServerCost", () => Settings.ServerCost = 0f);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoWaterElectricity))
			{
				Helpers.TryExecute("NoWaterElectricity", () =>
				{
					Settings.ElectricityBill = 0f;
					Settings.Waterbill = 0f;
					Settings.Gasbill = 0f;
				});
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoMaxMarketShare))
			{
				// Disabled, see #150
				//Settings.MyCompany.Products.ForEach(product => product.MarketShare = 1f);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoMaintenance))
			{
				Helpers.TryExecute("NoMaintenance", EnvironmentFeatures.ApplyNoMaintenance);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoMissedSupportTickets))
			{
				Helpers.TryExecute("NoMissedSupportTickets", ApplyNoMissedSupportTickets);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreePrint))
			{
				Helpers.TryExecute("FreePrint", ApplyFreePrint);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.IncreasePrintSpeed))
			{
				Helpers.TryExecute("IncreasePrintSpeed", ApplyIncreasePrintSpeed);
			}
		}

		private void OnMinutePassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoEndDesign))
			{
				Helpers.TryExecute("AutoEndDesign", ApplyAutoEndDesign);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoContractProgression))
			{
				Helpers.TryExecute("AutoContractProgression", ApplyAutoContractProgression);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoEndResearch))
			{
				Helpers.TryExecute("AutoEndResearch", ApplyAutoEndResearch);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoEndPatent))
			{
				Helpers.TryExecute("AutoEndPatent", ApplyAutoEndPatent);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoResearchStart))
			{
				Helpers.TryExecute("AutoResearchStart", ApplyAutoResearchStart);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoAcceptHostingDeals))
			{
				Helpers.TryExecute("AutoAcceptHostingDeals", ApplyAutoAcceptHostingDeals);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoPorting))
			{
				Helpers.TryExecute("AutoPorting", ApplyAutoPorting);
			}

			bool fullEnvironment = Helpers.GetProperty(TrainerSettings, SettingKeys.FullEnvironment);
			bool fullRoomBrightness = Helpers.GetProperty(TrainerSettings, SettingKeys.FullRoomBrightness);
			bool increaseBookshelfSkill = Helpers.GetProperty(TrainerSettings, SettingKeys.IncreaseBookshelfSkill);
			bool noWaterElectricity = Helpers.GetProperty(TrainerSettings, SettingKeys.NoWaterElectricity);
			bool disableFires = Helpers.GetProperty(TrainerSettings, SettingKeys.DisableFires);

			if (fullEnvironment)
			{
				Helpers.TryExecute("FullEnvironment", EnvironmentFeatures.ApplyFullEnvironment);
			}

			if (fullRoomBrightness)
			{
				Helpers.TryExecute("FullRoomBrightness", EnvironmentFeatures.ApplyFullRoomBrightness);
			}

			if (increaseBookshelfSkill)
			{
				Helpers.TryExecute("IncreaseBookshelfSkill", EnvironmentFeatures.ApplyIncreaseBookshelfSkill);
			}

			if (noWaterElectricity)
			{
				Helpers.TryExecute("NoWaterElectricity", EnvironmentFeatures.ApplyNoWaterElectricityToAllFurniture);
			}

			if (disableFires)
			{
				Helpers.TryExecute("DisableFires", EnvironmentFeatures.ApplyDisableFiresFireStarter);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.IncreaseWalkSpeed))
			{
				Helpers.TryExecute("IncreaseWalkSpeed", EmployeeFeatures.ApplyIncreaseWalkSpeed);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.IncreaseCourierCapacity))
			{
				Helpers.TryExecute("IncreaseCourierCapacity", () =>
				{
					AI.MaxBoxes = Constants.MAX_BOXES_BOOSTED;
					AI.MaxBoxCarry = Constants.MAX_CARRY_BOOSTED;
				});
			}

			//Not working
			//AI.BoxPrice = Helpers.GetProperty(TrainerSettings, SettingKeys.ReduceBoxPrice) ? 62.5f : 125;

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.ReduceISPCost))
			{
				Helpers.TryExecute("ReduceISPCost", () => Settings.Environment.ISPCostFactor = GetDefaultISPCostFactor() / 2f);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.ReduceExpansionCost))
			{
				Helpers.TryExecute("ReduceExpansionCost", () => Settings.ExpansionCost = Constants.EXPANSION_COST_HALF);
			}
		}

		private bool ToggleJustEnabled(SettingKey<bool> settingKey)
		{
			return GetToggleTransition(settingKey) == ToggleTransitionEnum.JustEnabled;
		}

		private ToggleTransitionEnum GetToggleTransition(SettingKey<bool> settingKey)
		{
			return _toggleStates.Observe(settingKey.Key, Helpers.GetProperty(TrainerSettings, settingKey));
		}

		// Company.Bankrupt is recomputed daily (MarketSimulation.SimulateMonth runs from
		// TimeOfDay.UpdateDay despite the name), not monthly.
		private void OnDayPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DigitalDistributionMonopol))
			{
				Helpers.TryExecute("DigitalDistributionMonopol", ApplyDigitalDistributionMonopol);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoInsuranceCost))
			{
				Helpers.TryExecute("NoInsuranceCost", ApplyNoInsuranceCost);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreeMarketing))
			{
				Helpers.TryExecute("FreeMarketing", ApplyFreeMarketing);
			}
		}

		private void OnMonthPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreeEmployees))
			{
				Helpers.TryExecute("FreeEmployees", EmployeeFeatures.ApplyFreeEmployees);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.MoreCreativity))
			{
				Helpers.TryExecute("MoreCreativity", EmployeeFeatures.ApplyMoreCreativity);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableBurglars))
			{
				Helpers.TryExecute("DisableBurglars", EnvironmentFeatures.ApplyDisableBurglars);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableFireInspection))
			{
				Helpers.TryExecute("DisableFireInspection", EnvironmentFeatures.ApplyDisableFireInspection);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableFurnitureStealing))
			{
				Helpers.TryExecute("DisableFurnitureStealing", EnvironmentFeatures.ApplyDisableFurnitureStealing);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoVacation))
			{
				Helpers.TryExecute("NoVacation", EmployeeFeatures.ApplyNoVacation);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.LockAge))
			{
				Helpers.TryExecute("LockAge", EmployeeFeatures.ApplyLockAge);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoLoanInterest))
			{
				Helpers.TryExecute("NoLoanInterest", ApplyNoLoanInterest);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoFounderDividends))
			{
				Helpers.TryExecute("NoFounderDividends", ApplyNoFounderDividends);
			}
		}

		private void Update()
		{
			if (!isActiveAndEnabled || !Helpers.IsGameLoaded)
			{
				return;
			}

			if (Input.GetKey(KeyCode.F1))
			{
				Main.OpenSettingsWindow();
			}

			if (Input.GetKey(KeyCode.F2))
			{
				Main.CloseSettingsWindow();
			}

			if (!EmployeeFeatures.SpecializationsLoaded && Settings.MyCompany != null)
			{
				Helpers.TryExecute("LoadSpecializations", () =>
				{
					EmployeeFeatures.LoadSpecializations();
					Helpers.ShowDiscordInvite(displayAsPopup: true);
				});
			}

			if (!_oneTimeSettingsApplied)
			{
				GameSettings.MaxFloor = Constants.MAX_FLOOR;
				_oneTimeSettingsApplied = true;
			}

			// Cheats.* are plain static fields with no save-file backing, so they reset to false on every game
			// process start regardless of what the trainer's own (persisted) settings say. Re-push them once per
			// load so a setting that was enabled before a restart takes effect again; each is tracked independently
			// so a transient failure in one only retries that one, without losing or re-running the others.
			if (!_forceLightsApplied)
			{
				_forceLightsApplied = Helpers.TryExecute("ForceLights", () => EnvironmentFeatures.ApplyForceLights(Helpers.GetProperty(TrainerSettings, SettingKeys.ForceLights)));
			}

			if (!_showRoomCeilingsApplied)
			{
				_showRoomCeilingsApplied = Helpers.TryExecute("ShowRoomCeilings", () => EnvironmentFeatures.ApplyShowRoomCeilings(Helpers.GetProperty(TrainerSettings, SettingKeys.ShowRoomCeilings)));
			}

			if (!_unlimitedSubsidiariesApplied)
			{
				_unlimitedSubsidiariesApplied = Helpers.TryExecute("UnlimitedSubsidiaries", () => ApplyUnlimitedSubsidiaries(Helpers.GetProperty(TrainerSettings, SettingKeys.UnlimitedSubsidiaries)));
			}

			if (GetToggleTransition(SettingKeys.ForceLights) != ToggleTransitionEnum.Unchanged)
			{
				Helpers.TryExecute("ForceLights", () => EnvironmentFeatures.ApplyForceLights(Helpers.GetProperty(TrainerSettings, SettingKeys.ForceLights)));
			}

			if (GetToggleTransition(SettingKeys.ShowRoomCeilings) != ToggleTransitionEnum.Unchanged)
			{
				Helpers.TryExecute("ShowRoomCeilings", () => EnvironmentFeatures.ApplyShowRoomCeilings(Helpers.GetProperty(TrainerSettings, SettingKeys.ShowRoomCeilings)));
			}

			if (GetToggleTransition(SettingKeys.UnlimitedSubsidiaries) != ToggleTransitionEnum.Unchanged)
			{
				Helpers.TryExecute("UnlimitedSubsidiaries", () => ApplyUnlimitedSubsidiaries(Helpers.GetProperty(TrainerSettings, SettingKeys.UnlimitedSubsidiaries)));
			}

			if (ToggleJustEnabled(SettingKeys.NoMaintenance))
			{
				Helpers.TryExecute("NoMaintenance", EnvironmentFeatures.ApplyNoMaintenance);
			}

			if (ToggleJustEnabled(SettingKeys.NoVacation))
			{
				Helpers.TryExecute("NoVacation", EmployeeFeatures.ApplyNoVacation);
			}

			if (ToggleJustEnabled(SettingKeys.MoreHostingDeals))
			{
				Helpers.TryExecute("MoreHostingDeals", ApplyHostingDealTiming);
			}

			if (ToggleJustEnabled(SettingKeys.AutoEndDesign))
			{
				Helpers.TryExecute("AutoEndDesign", ApplyAutoEndDesign);
			}

			if (ToggleJustEnabled(SettingKeys.AutoContractProgression))
			{
				Helpers.TryExecute("AutoContractProgression", ApplyAutoContractProgression);
			}

			if (ToggleJustEnabled(SettingKeys.AutoEndResearch))
			{
				Helpers.TryExecute("AutoEndResearch", ApplyAutoEndResearch);
			}

			if (ToggleJustEnabled(SettingKeys.AutoEndPatent))
			{
				Helpers.TryExecute("AutoEndPatent", ApplyAutoEndPatent);
			}

			if (ToggleJustEnabled(SettingKeys.FreeStaff))
			{
				Helpers.TryExecute("FreeStaff", () => Settings.StaffSalaryDue = 0f);
			}

			if (ToggleJustEnabled(SettingKeys.NoServerCost))
			{
				Helpers.TryExecute("NoServerCost", () => Settings.ServerCost = 0f);
			}

			if (ToggleJustEnabled(SettingKeys.NoWaterElectricity))
			{
				Helpers.TryExecute("NoWaterElectricity", () =>
				{
					Settings.ElectricityBill = 0f;
					Settings.Waterbill = 0f;
					Settings.Gasbill = 0f;
					EnvironmentFeatures.ApplyNoWaterElectricityToAllFurniture();
				});
			}

			if (ToggleJustEnabled(SettingKeys.DisableFurnitureStealing))
			{
				Helpers.TryExecute("DisableFurnitureStealing", EnvironmentFeatures.ApplyDisableFurnitureStealing);
			}

			switch (GetToggleTransition(SettingKeys.NoEducationCost))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("NoEducationCost", ApplyNoEducationCost);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("NoEducationCost", RestoreEducationCost);
					break;
				default:
					break;
			}

			if (ToggleJustEnabled(SettingKeys.AutoResearchStart))
			{
				Helpers.TryExecute("AutoResearchStart", ApplyAutoResearchStart);
			}

			if (ToggleJustEnabled(SettingKeys.DigitalDistributionMonopol))
			{
				Helpers.TryExecute("DigitalDistributionMonopol", ApplyDigitalDistributionMonopol);
			}

			if (ToggleJustEnabled(SettingKeys.AutoMaxMarketShare))
			{
				// Disabled, see #150
				//Settings.MyCompany.Products.ForEach(product => product.MarketShare = 1f);
			}

			if (ToggleJustEnabled(SettingKeys.AutoAcceptHostingDeals))
			{
				Helpers.TryExecute("AutoAcceptHostingDeals", ApplyAutoAcceptHostingDeals);
			}

			if (ToggleJustEnabled(SettingKeys.FreeEmployees))
			{
				Helpers.TryExecute("FreeEmployees", EmployeeFeatures.ApplyFreeEmployees);
			}

			if (ToggleJustEnabled(SettingKeys.MoreCreativity))
			{
				Helpers.TryExecute("MoreCreativity", EmployeeFeatures.ApplyMoreCreativity);
			}

			if (ToggleJustEnabled(SettingKeys.DisableBurglars))
			{
				Helpers.TryExecute("DisableBurglars", EnvironmentFeatures.ApplyDisableBurglars);
			}

			if (ToggleJustEnabled(SettingKeys.DisableFireInspection))
			{
				Helpers.TryExecute("DisableFireInspection", EnvironmentFeatures.ApplyDisableFireInspection);
			}

			switch (GetToggleTransition(SettingKeys.FreePrint))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("FreePrint", ApplyFreePrint);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("FreePrint", RestoreFreePrint);
					break;
				default:
					break;
			}

			if (ToggleJustEnabled(SettingKeys.IncreasePrintSpeed))
			{
				Helpers.TryExecute("IncreasePrintSpeed", ApplyIncreasePrintSpeed);
			}

			switch (GetToggleTransition(SettingKeys.FullEnvironment))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("FullEnvironment", EnvironmentFeatures.ApplyFullEnvironment);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("FullEnvironment", EnvironmentFeatures.RestoreFullEnvironment);
					break;
				default:
					break;
			}

			switch (GetToggleTransition(SettingKeys.FullRoomBrightness))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("FullRoomBrightness", EnvironmentFeatures.ApplyFullRoomBrightness);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("FullRoomBrightness", EnvironmentFeatures.RestoreFullRoomBrightness);
					break;
				default:
					break;
			}

			if (ToggleJustEnabled(SettingKeys.IncreaseBookshelfSkill))
			{
				Helpers.TryExecute("IncreaseBookshelfSkill", EnvironmentFeatures.ApplyIncreaseBookshelfSkill);
			}

			if (ToggleJustEnabled(SettingKeys.DisableFires))
			{
				Helpers.TryExecute("DisableFires", EnvironmentFeatures.ApplyDisableFiresFireStarter);
			}

			switch (GetToggleTransition(SettingKeys.IncreaseWalkSpeed))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("IncreaseWalkSpeed", EmployeeFeatures.ApplyIncreaseWalkSpeed);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("IncreaseWalkSpeed", EmployeeFeatures.RestoreIncreaseWalkSpeed);
					break;
				default:
					break;
			}

			switch (GetToggleTransition(SettingKeys.IncreaseCourierCapacity))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("IncreaseCourierCapacity", () =>
					{
						AI.MaxBoxes = Constants.MAX_BOXES_BOOSTED;
						AI.MaxBoxCarry = Constants.MAX_CARRY_BOOSTED;
					});
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("IncreaseCourierCapacity", RestoreIncreaseCourierCapacity);
					break;
				default:
					break;
			}

			switch (GetToggleTransition(SettingKeys.ReduceISPCost))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("ReduceISPCost", () => Settings.Environment.ISPCostFactor = GetDefaultISPCostFactor() / 2f);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("ReduceISPCost", RestoreReduceISPCost);
					break;
				default:
					break;
			}

			switch (GetToggleTransition(SettingKeys.ReduceExpansionCost))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("ReduceExpansionCost", () => Settings.ExpansionCost = Constants.EXPANSION_COST_HALF);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("ReduceExpansionCost", RestoreReduceExpansionCost);
					break;
				default:
					break;
			}

			_minuteWatcher.Poll();

			// Each feature owns one TryExecute boundary around its own traversal. A failing feature can abort
			// its own remaining work for this frame, but unrelated features and the next frame's retry are
			// unaffected. A feature spanning more than one collection (e.g. NoSickness affecting both rooms
			// and actors) still gets a single boundary, since isolating its parts from each other isn't required.
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoiseReduction))
			{
				Helpers.TryExecute("NoiseReduction", EnvironmentFeatures.ApplyNoiseReduction);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableFires))
			{
				Helpers.TryExecute("DisableFires", EnvironmentFeatures.ApplyDisableActiveFires);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.CleanRooms))
			{
				Helpers.TryExecute("CleanRooms", EnvironmentFeatures.ApplyCleanRooms);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoSickness))
			{
				Helpers.TryExecute("NoSickness", EnvironmentFeatures.ApplyNoSickness);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.TemperatureLock))
			{
				Helpers.TryExecute("TemperatureLock", EnvironmentFeatures.ApplyTemperatureLock);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FullSatisfaction))
			{
				Helpers.TryExecute("FullSatisfaction", EmployeeFeatures.ApplyFullSatisfaction);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoStress))
			{
				Helpers.TryExecute("NoStress", EmployeeFeatures.ApplyNoStress);
			}

			if (Helpers.GetProperty(StoresSettings, SettingKeys.LeadEfficiencyStore) != null)
			{
				Helpers.TryExecute("LeadEfficiencyStore", EmployeeFeatures.ApplyLeadEfficiencyStore);
			}

			if (Helpers.GetProperty(StoresSettings, SettingKeys.EfficiencyStore) != null)
			{
				Helpers.TryExecute("EfficiencyStore", EmployeeFeatures.ApplyEfficiencyStore);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoNeeds))
			{
				Helpers.TryExecute("NoNeeds", EmployeeFeatures.ApplyNoNeeds);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.MoreInspiration))
			{
				Helpers.TryExecute("MoreInspiration", EmployeeFeatures.ApplyMoreInspiration);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableForcePause))
			{
				Helpers.TryExecute("DisableForcePause", () => GameSettings.ForcePause = false);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableForceFreeze))
			{
				Helpers.TryExecute("DisableForceFreeze", () => GameSettings.FreezeGame = false);
			}

			// AddHeat checks/triggers an audit synchronously, so clearing every frame is the tightest reactive window available.
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoOffshoreHeat))
			{
				Helpers.TryExecute("NoOffshoreHeat", () => Settings.Heat = 0f);
			}

			/*
			 foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
			{
			  actor.employee.BirthDate += months;
			  actor.employee.Hired += months;
			  actor.employee.LastWage += months;
			  actor.employee.LastInpirationUse += months;
			  actor.LastMeeting += months;
			  actor.MeetingTime += months;
			  actor.DriveTime += months;
			  actor.DespawnTime += months;
			  actor.LeaveTime += months;
			  actor.LastSocial += months;
			  actor.ForgetfulETA += months;
			}
			 * */
		}

		// GameSettings.PaybackLoan charges Monthly (principal + MonthlyInterest) per loan, then removes paid-off loans, before OnMonthPassed fires.
		// Refunding the still-outstanding loans' MonthlyInterest here needs no captured baseline: Loan fields are never modified after creation.
		private static void ApplyNoLoanInterest()
		{
			float totalInterest = Settings.Loans.Sum(loan => loan.MonthlyInterest);
			if (totalInterest > 0f)
			{
				Settings.MyCompany.MakeTransaction(totalInterest, Company.TransactionCategory.Interest);
			}
		}

		// PayDividends (paid monthly, despite being called from something named EndDay) records each payout in NewStock[i].Payout.
		private static void ApplyNoFounderDividends()
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

		private static void ApplyFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = 0f);
		}

		private static void RestoreFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = Constants.PRINTER_PRICE_DEFAULT);
		}

		private static void ApplyIncreasePrintSpeed()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintSpeed = 2f);
		}

		private static void ApplyHostingDealTiming()
		{
			int inGameHour = TimeOfDay.Instance.Hour;

			if ((inGameHour == 9 || inGameHour == 15) && !Helpers.DealIsPushed)
			{
				PushDeal();
			}
			else if (inGameHour != 9 && inGameHour != 15 && Helpers.DealIsPushed)
			{
				Helpers.DealIsPushed = false;
			}

			if (!Helpers.RewardIsGained && inGameHour == 12)
			{
				PushReward();
			}
			else if (inGameHour != 12 && Helpers.RewardIsGained)
			{
				Helpers.RewardIsGained = false;
			}
		}

		private static void ApplyAutoEndDesign()
		{
			// PromoteAction() itself blocks on this precondition (staying HasFinished forever
			// without it), so it must be mirrored here to avoid calling it on ineligible designs.
			var designDocuments = Settings.MyCompany.WorkItems
								.OfType<DesignDocument>()
								.Where(d => d.HasFinished && (!d.NeedsLead() || d.LeadWork != null))
								.ToList();

			designDocuments.ForEach(designDocument =>
			{
				designDocument.PromoteAction();
			});
		}

		// WorkItem.contract is set on the DesignDocument the game creates for accepted contract work
		// (ContractWork.GenerateWorkItem -> DesignDocument.CreateWork), so it identifies contract work
		// without guessing from names. HasFinished already means "required code/art units satisfied for
		// every design iteration" (verified in DesignDocument.DoWork: it only flips true once AllDone()
		// passes for the current iteration and either Parent is set or the iteration cap is reached), so
		// PromoteAction() is called with the exact same precondition as ApplyAutoEndDesign(). PromoteAction
		// itself only replaces the DesignDocument with the next-phase WorkItem (QA/bug-fixing/release stay
		// untouched, and normal/manual control over that new item is unaffected). Running after
		// ApplyAutoEndDesign() in OnMinutePassed means anything it already promoted (and Kill()ed out of
		// WorkItems) is gone from this query, so the two toggles can never double-promote the same item.
		private static void ApplyAutoContractProgression()
		{
			var contractDesigns = Settings.MyCompany.WorkItems
								.OfType<DesignDocument>()
								.Where(d => d.contract != null && d.HasFinished && (!d.NeedsLead() || d.LeadWork != null))
								.ToList();

			contractDesigns.ForEach(designDocument =>
			{
				designDocument.PromoteAction();
			});
		}

		// The RoleBit each type's own GetBoostRole checks; other WorkItem types have no single verified preference and are left alone.
		private static Employee.RoleBit? GetOptimizerRoleBit(WorkItem item)
		{
			if (item is DesignDocument)
			{
				return Employee.RoleBit.Designer;
			}

			if (item is MarketingPlan)
			{
				return Employee.RoleBit.Service;
			}

			if (item is SoftwarePort)
			{
				return Employee.RoleBit.Programmer;
			}

			return null;
		}

		private static int ScoreTeamForRole(Team team, Employee.RoleBit roleBit)
		{
			return team.GetEmployeesDirect().Count(actor => actor.employee.IsRole(roleBit, false));
		}

		public static void OptimizeTeamAssignments()
		{
			int reassigned = 0;

			foreach (WorkItem item in Settings.MyCompany.WorkItems.ToList())
			{
				if (item.AutoDev || item.DevTeams.Count != 1)
				{
					continue;
				}

				Employee.RoleBit? roleBit = GetOptimizerRoleBit(item);
				if (roleBit == null)
				{
					continue;
				}

				DesignDocument designDocument = item as DesignDocument;
				if (designDocument != null && designDocument.NeedsLead() && designDocument.LeadWork != null)
				{
					continue;
				}

				Team currentTeam = GameSettings.GetTeam(item.DevTeams.First());
				if (currentTeam == null)
				{
					continue;
				}

				Team bestTeam = currentTeam;
				int bestScore = ScoreTeamForRole(currentTeam, roleBit.Value);

				foreach (Team team in Settings.sActorManager.Teams.Values)
				{
					int score = ScoreTeamForRole(team, roleBit.Value);
					if (score > bestScore)
					{
						bestScore = score;
						bestTeam = team;
					}
				}

				if (bestTeam != currentTeam)
				{
					item.SetDevTeams(new List<string> { bestTeam.Name });
					reassigned++;
				}
			}

			if (reassigned == 0)
			{
				WindowManager.SpawnDialog("Trainer: All eligible work is already assigned to its best available team!", false, DialogWindow.DialogType.Information);
				return;
			}

			HUD.Instance.AddPopupMessage("Trainer: Team assignments optimized!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		// Verified against ResearchWork.FinishNow() in the vendored assembly: this is the same
		// AddResearch + AddTechLevel + (conditional) LegalWork + Kill(false) sequence the game
		// itself runs to finalize a completed research work item, minus the player-facing patent
		// confirmation dialog. Kill(false) removes the item from Company.WorkItems immediately,
		// so a work item can never reach this method twice.
		private static void CompleteResearchWork(ResearchWork researchWork)
		{
			Settings.MyCompany.AddResearch(researchWork.Spec, researchWork.Year);
			TechLevel tech = Settings.simulation.AddTechLevel(researchWork.Spec, researchWork.Year, SDateTime.Now(), true);
			if (tech != null)
			{
				LegalWork legalWork = new LegalWork(tech);
				Settings.MyCompany.WorkItems.Add(legalWork);
				Settings.ApplyDefaultTeams(legalWork, ((int)legalWork.Type).ToString() + "Team");
			}
			researchWork.Kill(false);
		}

		private static void ApplyAutoEndResearch()
		{
			var researchWorks = Settings.MyCompany.WorkItems
								.OfType<ResearchWork>()
								.Where(rw => rw.Finished)
								.ToList();

			researchWorks.ForEach(CompleteResearchWork);
		}

		private static void ApplyAutoEndPatent()
		{
			var legalWorks = Settings.MyCompany.WorkItems
							   .OfType<LegalWork>()
							   .Where(lw => lw.CurrentStage() == "Finished" &&
									lw.Type == LegalWork.WorkType.Patent)
							   .ToList();

			legalWorks.ForEach(legalWork =>
			{
				legalWork.PatentNow();
			});
		}

		// Simulate() misses a ticket once its stored timestamp ages past ~2 months; refreshing it here keeps that check from ever tripping.
		private static void ApplyNoMissedSupportTickets()
		{
			foreach (SupportWork supportWork in Settings.MyCompany.WorkItems.OfType<SupportWork>())
			{
				for (int i = 0; i < supportWork.Tickets.Count; i++)
				{
					supportWork.Tickets[i] = SDateTime.Now();
				}
			}
		}

		private static float[] _defaultEdCost;

		private static void ApplyNoEducationCost()
		{
			if (_defaultEdCost == null)
			{
				_defaultEdCost = (float[])EducationWindow.EdCost.Clone();
			}
			EducationWindow.EdCost = new[] { 0f, 0f, 0f };
		}

		private static void RestoreEducationCost()
		{
			if (_defaultEdCost != null)
			{
				EducationWindow.EdCost = (float[])_defaultEdCost.Clone();
			}
		}

		private static void ApplyAutoResearchStart()
		{
			var activeTechLevels = MarketSimulation.Active.TechLevels;
			var defaultResearchTeams = GameSettings.Instance.GetDefaultTeams("Research");
			var currentYear = TimeOfDay.Instance.Year;

			if (activeTechLevels.Count > 0 && defaultResearchTeams.Count > 0)
			{
				foreach (var activeTechLevel in activeTechLevels)
				{
					if (!Settings.IsResearching(activeTechLevel.Key))
					{
						int latestResearchYear = Settings.MyCompany.GetLatestResearch(activeTechLevel.Key, -1);
						if (latestResearchYear < currentYear)
						{
							var researchWork = new ResearchWork(activeTechLevel.Key, currentYear);
							researchWork.AddDevTeams(defaultResearchTeams);
							Settings.MyCompany.AddWorkItem(researchWork);
						}
					}
				}
			}
		}

		// Mirrors what SoftwarePort.DoWork itself does once Progress reaches Goal; RefreshCurrent() advances to the next OS or kills the work item once all are ported.
		private static bool AdvanceSoftwarePort(SoftwarePort port)
		{
			if (port.Current == null || port.Product == null || port.Product.DevCompany != Settings.MyCompany)
			{
				return false;
			}

			port.Current.Progress = port.Current.Goal;
			port.Current.Finished = true;
			port.RefreshCurrent();
			return true;
		}

		private static void ApplyAutoPorting()
		{
			foreach (SoftwarePort port in Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList())
			{
				AdvanceSoftwarePort(port);
			}
		}

		public static void InstantPorting()
		{
			var ports = Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList();

			if (ports.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: No active porting work to complete!", false, DialogWindow.DialogType.Information);
				return;
			}

			foreach (SoftwarePort port in ports)
			{
				// Bounded by OSs.Count so a port stuck waiting on something else is left alone rather than looped on.
				for (int i = 0; i < port.OSs.Count; i++)
				{
					if (!AdvanceSoftwarePort(port))
					{
						break;
					}
				}
			}

			HUD.Instance.AddPopupMessage("Trainer: Porting work has been completed!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		private static readonly MarketingPlan.PressOption[] PressReleaseOptions =
		{
			MarketingPlan.PressOption.Text,
			MarketingPlan.PressOption.Image,
			MarketingPlan.PressOption.Video,
		};

		// Only PressRelease has a completable Progress array; PostMarket/Hype campaigns run perpetually with no finish state to force.
		private static void CompletePressRelease(MarketingPlan plan)
		{
			for (int i = 0; i < PressReleaseOptions.Length; i++)
			{
				if ((plan.PressOptions & PressReleaseOptions[i]) != 0)
				{
					plan.Progress[i] = 1f;
				}
			}

			plan.StopMarketing();
		}

		public static void InstantMarketing()
		{
			var plans = Settings.MyCompany.WorkItems.OfType<MarketingPlan>()
				.Where(plan => plan.Type == MarketingPlan.TaskType.PressRelease)
				.ToList();

			if (plans.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: No active press release campaigns to complete!", false, DialogWindow.DialogType.Information);
				return;
			}

			foreach (MarketingPlan plan in plans)
			{
				CompletePressRelease(plan);
			}

			HUD.Instance.AddPopupMessage("Trainer: Press release campaigns have been completed!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		// MarketingPlan.AddEffect bills Spent to the Marketing category daily for PostMarket campaigns then moves it into LastSpent and zeroes Spent; refunding that settled amount needs no captured baseline.
		private static void ApplyFreeMarketing()
		{
			foreach (MarketingPlan plan in Settings.MyCompany.WorkItems.OfType<MarketingPlan>())
			{
				if (plan.Type == MarketingPlan.TaskType.PostMarket && plan.LastSpent > 0f)
				{
					Settings.MyCompany.MakeTransaction(plan.LastSpent, Company.TransactionCategory.Marketing);
				}
			}
		}

		private static void ApplyDigitalDistributionMonopol()
		{
			foreach (var company in Settings.simulation.Companies.Values.ToList())
			{
				if (company.Bankrupt && company.Distribution != null)
				{
					MarketSimulation.Active.DistributionPlatforms.Remove(company.Distribution);
					HUD.Instance.digitalDistributionWindow.PlatformList.Items.Remove(company.Distribution);
				}

				if (company == Settings.MyCompany || company.Distribution == null || !company.Distribution.Open)
					continue;

				company.Distribution.SetCut(1f);
				company.Distribution.SetAutoAcceptClients(false);
				company.Distribution.AvailableBandwidth = 0f;
				company.Distribution.ItemSales = 0f;
				company.Distribution.ActualItemSales = 0f;
				company.Distribution.LastLoad = 0f;
				company.Distribution.MarketShare = 0f;
				MarketSimulation.Active.ClosePlatform(company.Distribution);
			}
		}

		// Refunds the daily insurance bill TimeOfDay.UpdateDay already charged this tick, leaving CurrentRate/coverage untouched.
		private static void ApplyNoInsuranceCost()
		{
			if (!Settings.PassedFireInspection)
			{
				return;
			}

			float dailyBill = Settings.Insurance.GetContentBill(true) / GameSettings.DaysPerMonth;
			Settings.MyCompany.MakeTransaction(dailyBill, Company.TransactionCategory.Bills);
		}

		private static void ApplyAutoAcceptHostingDeals()
		{
			var serverGroups = Settings.GetAllServerGroups().ToList();
			var serverDeals = HUD.Instance.dealWindow.AllDeals.Values.OfType<ServerDeal>().ToList();

			if (serverGroups.Count > 0 && serverDeals.Count > 0)
			{
				ServerGroup mostPowerfulServerGroup = new ServerGroup("TRAINER") { PowerSum = 0f };
				foreach (var serverGroup in serverGroups)
				{
					if (serverGroup.PowerSum > mostPowerfulServerGroup.PowerSum)
						mostPowerfulServerGroup = serverGroup;
				}

				var activeServerDeals = HUD.Instance.dealWindow.GetActiveDeals().OfType<ServerDeal>().ToList();
				foreach (var serverDeal in serverDeals)
				{
					if (!activeServerDeals.Contains(serverDeal))
					{
						HUD.Instance.dealWindow.ActuallyAcceptDeal(serverDeal, true);
						Settings.RegisterWithServer(mostPowerfulServerGroup.Name, serverDeal);
					}
				}
			}
		}

		public static void ClearLoans()
		{
			Settings.Loans.Clear();
			HUD.Instance.AddPopupMessage("Trainer: All loans are cleared!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void PushReward()
		{
			var Deals = HUD.Instance.dealWindow.GetActiveDeals().Where(deal => deal.ToString() == "ServerDeal").ToArray();

			if (!Deals.Any())
			{
				return;
			}

			for (int i = 0; i < Deals.Length; i++)
			{
				Settings.MyCompany.MakeTransaction(Helpers.Random.Next(500, 50000), Company.TransactionCategory.Deals);
			}

			Helpers.RewardIsGained = true;
		}

		public static void PushDeal()
		{
			SoftwareProduct[] Products = Settings.simulation.GetAllProducts(false).Where(pr =>
				  MarketSimulation.Active.SoftwareTypes.ContainsKey(pr.Type.ToString())
				&& pr.Userbase > 0
				&& pr.DevCompany.Name != Settings.MyCompany.Name
				&& pr.ServerReq > 0
				&& !pr.ExternalHostingActive)
					  .ToArray();

			if (Products.Length == 0)
				return;

			int index = Helpers.Random.Next(0, Products.Length);
			var dealExist = HUD.Instance.dealWindow.AllDeals.Values.AnyOf<ServerDeal>(x => x.Product.Name == Products[index].Name);
			if (!dealExist)
			{
				ServerDeal deal = new ServerDeal(Products[index]) { Request = true };
				deal.StillValid(true);
				deal.PerPower *= 2f;

				HUD.Instance.dealWindow.InsertDeal(deal);

				Helpers.DealIsPushed = true;
			}
		}

		public static void Test()
		{
			var designDocuments = Settings.MyCompany.WorkItems.OfType<DesignDocument>().Where(d => !d.HasFinished).ToList();

			foreach (var designDocument in designDocuments)
			{
				for (int i = 0; i < DesignDocument.MaxIteration; i++)
				{
					if (designDocument.Parent == null && designDocument.Iteration < 3)
					{
						for (int index = 0; index < designDocument.Features.Length; index++)
						{
							designDocument.Features[index].ArtDone = designDocument.Features[index].CodeDone = false;
							designDocument.Features[index].Progress = 1f;
							designDocument.Features[index].DevTime = 1f;
							designDocument.Features[index].Qual = 1f;
							designDocument.Features[index].LastIterationProg = 1f;
						}
						DevConsole.Console.Log(designDocument.GetProgress());
						designDocument.Iteration++;
					}
				}
			}
		}

		public static void AIBankrupt()
		{
			SimulatedCompany[] Companies = Settings.simulation.Companies.Values.ToArray();

			for (int i = 0; i < Companies.Length; i++)
			{
				Companies[i].Bankrupt = true;
			}
		}

		public static void SellProductStock()
		{
			WindowManager.SpawnDialog("Stock of products with no active users were sold at half the price.",
				false, DialogWindow.DialogType.Information);

			SoftwareProduct[] Products = Settings.MyCompany.Products
												 .Where(product => product.Userbase == 0)
												 .ToArray();

			if (Products.Length == 0)
			{
				return;
			}

			for (int i = 0; i < Products.Length; i++)
			{
				SoftwareProduct product = Products[i];
				int st = (int)product.PhysicalCopies * (int)(product.Price / 2);

				product.PhysicalCopies = 0;
				Settings.MyCompany.MakeTransaction(st, Company.TransactionCategory.Sales);
			}
		}

		public static void RemoveSoft()
		{
			return;

			//currently broken
			SDateTime time = new SDateTime(1, 70);
			CompanyType type = new CompanyType();
			var dict = new Dictionary<string, string[]>();
			SimulatedCompany simComp = new SimulatedCompany("Trainer Company", time, type, dict, 0f, MarketSimulation.Active);
			simComp.CanMakeTransaction(2139095030f);

			SoftwareProduct[] Products = Settings.simulation.GetAllProducts(true).Where(product =>
				product.DevCompany == Settings.MyCompany &&
				product.Inventor != Settings.MyCompany.Name).ToArray();

			if (Products.Length == 0)
			{
				return;
			}

			for (int i = 0; i < Products.Length; i++)
			{
				SoftwareProduct Product = Products[i];

				Product.Userbase = 0;
				Product.PhysicalCopies = 0;
				Product.Marketing = 0;
				Product.Trade(simComp, time);
			}

			WindowManager.SpawnDialog("Products that you didn't invent are removed.", false, DialogWindow.DialogType.Information);
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

		#region Native Cheat Flags

		public static void ApplyUnlimitedSubsidiaries(bool enabled)
		{
			Cheats.InfiniteSubs = enabled;
		}

		#endregion

		#region MonthDays

		public static void MonthDaysAction(int i)
		{
			GameSettings.DaysPerMonth = i;
			WindowManager.SpawnDialog("You have changed days per month. Please restart the game.", false, DialogWindow.DialogType.Warning);
		}

		public static void MonthDays()
		{
			InputHelper.RequestInt("How many days per month do you want?", "Days per month", "2", MonthDaysAction, 1, 31);
		}

		#endregion

		#region Extend Deadline

		public static void ExtendDeadline()
		{
			foreach (var work in Settings.MyCompany.WorkItems)
			{
				var contract = work.contract;
				if (contract == null) continue;
				var deadline = contract.Deadline;
				contract.Deadline = new SDateTime(deadline.Year + 1, deadline.Month, deadline.Day);
			}
		}

		#endregion

		#region Fix Bugs

		public static void FixBugsAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && item.InBeta);

			if (WorkItem == null)
			{
				return;
			}

			WorkItem.FixedBugs = WorkItem.MaxBugs;
		}

		public static void FixBugs()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Fix Bugs", "", FixBugsAction);
		}

		#endregion

		#region Max Followers

		public static void MaxFollowersAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && !item.Paused);

			if (WorkItem == null)
			{
				return;
			}

			WorkItem.MaxFollowers += 1000000000;
			WorkItem.ReEvaluateMaxFollowers();

			WorkItem.FollowerChange += 1000000000f;
			WorkItem.Followers += 1000000000f;
		}

		public static void MaxFollowers()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Max Followers", "", MaxFollowersAction);
		}

		#endregion

		#region Instant Research

		public static void InstantResearch()
		{
			var eligibleResearch = Settings.MyCompany.WorkItems
								.OfType<ResearchWork>()
								.Where(rw => !rw.Finished)
								.ToList();

			if (eligibleResearch.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: No active research to complete!", false, DialogWindow.DialogType.Information);
				return;
			}

			eligibleResearch.ForEach(researchWork =>
			{
				researchWork.Progress = researchWork.Max;
				researchWork.Finished = true;
				CompleteResearchWork(researchWork);
			});

			HUD.Instance.AddPopupMessage("Trainer: Research completed instantly!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		#endregion

		#region Product Lookup

		// Shared by SetProductPriceAction, SetProductStockAction, and AddActiveUsersAction so
		// player and AI-company products can both be targeted without duplicating the lookup,
		// and without silently picking a product when the entered name isn't unique.
		private static SoftwareProduct ResolveProductByName(string name, out string errorMessage)
		{
			List<SoftwareProduct> matches = Settings.simulation.GetAllProducts(false)
				.Where(product => product.Name == name)
				.ToList();

			if (matches.Count == 0)
			{
				errorMessage = "Trainer: Product " + name + " not found!";
				return null;
			}

			if (matches.Count > 1)
			{
				string owners = string.Join(", ", matches.Select(product => product.DevCompany != null ? product.DevCompany.Name : "Unknown").ToArray());
				errorMessage = "Trainer: Product name " + name + " is ambiguous (owned by: " + owners + "). Rename the product or use a unique name.";
				return null;
			}

			errorMessage = null;
			return matches[0];
		}

		#endregion

		#region Set Product Price

		public static void SetProductPriceAction(float price)
		{
			string error;
			SoftwareProduct Product = ResolveProductByName(Helpers.ProductPriceName, out error);

			if (Product == null)
			{
				WindowManager.SpawnDialog(error, false, DialogWindow.DialogType.Information);
				return;
			}

			Product.Price = price;
			HUD.Instance.AddPopupMessage("Trainer: Price for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void SetProductPrice()
		{
			InputHelper.RequestFloat("Type product price:", "Product price", SetProductPriceAction, 0f, float.MaxValue);
		}

		#endregion

		#region Set Product Stock

		public static void SetProductStockAction(uint stock)
		{
			string error;
			SoftwareProduct Product = ResolveProductByName(Helpers.ProductPriceName, out error);

			if (Product == null)
			{
				WindowManager.SpawnDialog(error, false, DialogWindow.DialogType.Information);
				return;
			}

			Product.PhysicalCopies = stock;
			HUD.Instance.AddPopupMessage("Trainer: Stock for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void SetProductStock()
		{
			InputHelper.RequestUInt("Type product stock:", "Product stock", "100000", SetProductStockAction);
		}

		#endregion

		#region Add Active Users

		public static void AddActiveUsersAction(int users)
		{
			string error;
			SoftwareProduct Product = ResolveProductByName(Helpers.ProductPriceName, out error);

			if (Product == null)
			{
				WindowManager.SpawnDialog(error, false, DialogWindow.DialogType.Information);
				return;
			}

			Product.Userbase = users;
			HUD.Instance.AddPopupMessage("Trainer: Active users for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void AddActiveUsers()
		{
			InputHelper.RequestInt("Type product active users:", "Product active users", "100000", AddActiveUsersAction, 0, int.MaxValue);
		}

		#endregion

		#region Takeover Company

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

		#endregion

		#region Subsidiary Company

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

		#endregion

		#region Force Bankrupt

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

		#endregion

		#region Increase Money

		public static void IncreaseMoneyAction(int amount)
		{
			Settings.MyCompany.MakeTransaction(amount, Company.TransactionCategory.Deals);
			HUD.Instance.AddPopupMessage("Trainer: Money has been added in category Deals!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}
		public static void IncreaseMoney()
		{
			InputHelper.RequestInt("How much money do you want to add?", "Add Money", "100000", IncreaseMoneyAction);
		}

		#endregion

		#region Add AI Company Funds

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

		#endregion

		#region Add Rep

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

		#endregion

		#region Max Market Share

		public static void MaxMarketShare()
		{
			// Disabled, see #150
			//Settings.MyCompany.Products.ForEach(product => product.MarketShare = 1f);
			//HUD.Instance.AddPopupMessage("Trainer: Market share set to 100% for all products!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		#endregion

		#region UnlockAllRewards

		public static void UnlockAndClaimAllRewards()
		{
			GameSettings.Instance.CompletedTasks.AddRange(GameData.Tasks.Select(x => x.Name));
			GameSettings.Instance.ClaimedRewards.AddRange(GameData.Tasks.Select(x => x.Name));
			HUD.Instance.RefreshBuildButtons();

			WindowManager.SpawnDialog("Trainer: All rewards are unlocked and claimed.", false, DialogWindow.DialogType.Information);
		}

		#endregion

		private static void AreYouSureAction(Action action)
		{
			action();
		}

		#region Experimental/Test

		public static void TestAction(string input)
		{
			WorkItem WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && !item.InBeta);

			if (WorkItem == null)
				return;

			var softwareAlpha = ((SoftwareAlpha)WorkItem);
			softwareAlpha.AddQuality(10f, 10f, false);
		}

		public static void TestButton()
		{
			WindowManager.SpawnInputDialog("Type product name in alpha (not beta):", "Add Quality", "", TestAction);
		}

		#endregion

		#region Overrides

		public override void OnActivate()
		{
			SubscribeToSceneEvents();

			// No fresh sceneLoaded event fires if we are reactivated while already
			// in MainScene, so pick up the time-event subscriptions directly here.
			if (SceneManager.GetActiveScene().name == "MainScene")
			{
				SubscribeToEvents();
			}
		}

		public override void OnDeactivate()
		{
			UnsubscribeFromEvents();
			UnsubscribeFromSceneEvents();
		}

		#endregion
	}
}
