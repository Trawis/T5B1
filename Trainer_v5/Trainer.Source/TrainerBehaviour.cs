using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace Trainer_v5
{
	public class TrainerBehaviour : ModBehaviour
	{
		public override void OnActivate()
		{
			SubscribeToSceneEvents();

			// No sceneLoaded event fires on reactivation inside MainScene, so subscribe directly.
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

		private bool _sceneEventsSubscribed;
		private bool _timeEventsSubscribed;

		private readonly ToggleStateTracker _toggleStates = new ToggleStateTracker();

		// TimeOfDay has no minute-passed event; this raises one from its Minute field.
		private readonly GameMinuteWatcher _minuteWatcher = new GameMinuteWatcher();

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
				Helpers.TryExecute("MoreHostingDeals", MarketFeatures.ApplyHostingDealTiming);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreeStaff))
			{
				Helpers.TryExecute("FreeStaff", CompanyFeatures.ApplyFreeStaff);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoServerCost))
			{
				Helpers.TryExecute("NoServerCost", CompanyFeatures.ApplyNoServerCost);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoWaterElectricity))
			{
				Helpers.TryExecute("NoWaterElectricity", CompanyFeatures.ApplyNoWaterElectricityBills);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoMaxMarketShare))
			{
				// Disabled, see #150
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoMaintenance))
			{
				Helpers.TryExecute("NoMaintenance", EnvironmentFeatures.ApplyNoMaintenance);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoMissedSupportTickets))
			{
				Helpers.TryExecute("NoMissedSupportTickets", WorkItemFeatures.ApplyNoMissedSupportTickets);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreePrint))
			{
				Helpers.TryExecute("FreePrint", ProductFeatures.ApplyFreePrint);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.IncreasePrintSpeed))
			{
				Helpers.TryExecute("IncreasePrintSpeed", ProductFeatures.ApplyIncreasePrintSpeed);
			}
		}

		private void OnMinutePassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoEndDesign))
			{
				Helpers.TryExecute("AutoEndDesign", WorkItemFeatures.ApplyAutoEndDesign);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoContractProgression))
			{
				Helpers.TryExecute("AutoContractProgression", WorkItemFeatures.ApplyAutoContractProgression);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoEndResearch))
			{
				Helpers.TryExecute("AutoEndResearch", WorkItemFeatures.ApplyAutoEndResearch);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoEndPatent))
			{
				Helpers.TryExecute("AutoEndPatent", WorkItemFeatures.ApplyAutoEndPatent);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoResearchStart))
			{
				Helpers.TryExecute("AutoResearchStart", WorkItemFeatures.ApplyAutoResearchStart);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoAcceptHostingDeals))
			{
				Helpers.TryExecute("AutoAcceptHostingDeals", MarketFeatures.ApplyAutoAcceptHostingDeals);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.AutoPorting))
			{
				Helpers.TryExecute("AutoPorting", WorkItemFeatures.ApplyAutoPorting);
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
				Helpers.TryExecute("IncreaseCourierCapacity", CompanyFeatures.ApplyIncreaseCourierCapacity);
			}

			// Reduce Box Price is disabled: it does not work.

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.ReduceISPCost))
			{
				Helpers.TryExecute("ReduceISPCost", CompanyFeatures.ApplyReduceISPCost);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.ReduceExpansionCost))
			{
				Helpers.TryExecute("ReduceExpansionCost", CompanyFeatures.ApplyReduceExpansionCost);
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

		// Company.Bankrupt is recomputed daily (SimulateMonth runs from UpdateDay), not monthly.
		private void OnDayPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DigitalDistributionMonopol))
			{
				Helpers.TryExecute("DigitalDistributionMonopol", MarketFeatures.ApplyDigitalDistributionMonopol);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoInsuranceCost))
			{
				Helpers.TryExecute("NoInsuranceCost", CompanyFeatures.ApplyNoInsuranceCost);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.FreeMarketing))
			{
				Helpers.TryExecute("FreeMarketing", MarketFeatures.ApplyFreeMarketing);
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
				Helpers.TryExecute("NoLoanInterest", CompanyFeatures.ApplyNoLoanInterest);
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoFounderDividends))
			{
				Helpers.TryExecute("NoFounderDividends", CompanyFeatures.ApplyNoFounderDividends);
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

			// Cheats.* flags have no save backing and reset on every process start, so they are re-pushed once per load.
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
				_unlimitedSubsidiariesApplied = Helpers.TryExecute("UnlimitedSubsidiaries", () => CompanyFeatures.ApplyUnlimitedSubsidiaries(Helpers.GetProperty(TrainerSettings, SettingKeys.UnlimitedSubsidiaries)));
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
				Helpers.TryExecute("UnlimitedSubsidiaries", () => CompanyFeatures.ApplyUnlimitedSubsidiaries(Helpers.GetProperty(TrainerSettings, SettingKeys.UnlimitedSubsidiaries)));
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
				Helpers.TryExecute("MoreHostingDeals", MarketFeatures.ApplyHostingDealTiming);
			}

			if (ToggleJustEnabled(SettingKeys.AutoEndDesign))
			{
				Helpers.TryExecute("AutoEndDesign", WorkItemFeatures.ApplyAutoEndDesign);
			}

			if (ToggleJustEnabled(SettingKeys.AutoContractProgression))
			{
				Helpers.TryExecute("AutoContractProgression", WorkItemFeatures.ApplyAutoContractProgression);
			}

			if (ToggleJustEnabled(SettingKeys.AutoEndResearch))
			{
				Helpers.TryExecute("AutoEndResearch", WorkItemFeatures.ApplyAutoEndResearch);
			}

			if (ToggleJustEnabled(SettingKeys.AutoEndPatent))
			{
				Helpers.TryExecute("AutoEndPatent", WorkItemFeatures.ApplyAutoEndPatent);
			}

			if (ToggleJustEnabled(SettingKeys.FreeStaff))
			{
				Helpers.TryExecute("FreeStaff", CompanyFeatures.ApplyFreeStaff);
			}

			if (ToggleJustEnabled(SettingKeys.NoServerCost))
			{
				Helpers.TryExecute("NoServerCost", CompanyFeatures.ApplyNoServerCost);
			}

			if (ToggleJustEnabled(SettingKeys.NoWaterElectricity))
			{
				Helpers.TryExecute("NoWaterElectricity", () =>
				{
					CompanyFeatures.ApplyNoWaterElectricityBills();
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
					Helpers.TryExecute("NoEducationCost", CompanyFeatures.ApplyNoEducationCost);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("NoEducationCost", CompanyFeatures.RestoreEducationCost);
					break;
				default:
					break;
			}

			if (ToggleJustEnabled(SettingKeys.AutoResearchStart))
			{
				Helpers.TryExecute("AutoResearchStart", WorkItemFeatures.ApplyAutoResearchStart);
			}

			if (ToggleJustEnabled(SettingKeys.DigitalDistributionMonopol))
			{
				Helpers.TryExecute("DigitalDistributionMonopol", MarketFeatures.ApplyDigitalDistributionMonopol);
			}

			if (ToggleJustEnabled(SettingKeys.AutoMaxMarketShare))
			{
				// Disabled, see #150
			}

			if (ToggleJustEnabled(SettingKeys.AutoAcceptHostingDeals))
			{
				Helpers.TryExecute("AutoAcceptHostingDeals", MarketFeatures.ApplyAutoAcceptHostingDeals);
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
					Helpers.TryExecute("FreePrint", ProductFeatures.ApplyFreePrint);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("FreePrint", ProductFeatures.RestoreFreePrint);
					break;
				default:
					break;
			}

			if (ToggleJustEnabled(SettingKeys.IncreasePrintSpeed))
			{
				Helpers.TryExecute("IncreasePrintSpeed", ProductFeatures.ApplyIncreasePrintSpeed);
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
					Helpers.TryExecute("IncreaseCourierCapacity", CompanyFeatures.ApplyIncreaseCourierCapacity);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("IncreaseCourierCapacity", CompanyFeatures.RestoreIncreaseCourierCapacity);
					break;
				default:
					break;
			}

			switch (GetToggleTransition(SettingKeys.ReduceISPCost))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("ReduceISPCost", CompanyFeatures.ApplyReduceISPCost);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("ReduceISPCost", CompanyFeatures.RestoreReduceISPCost);
					break;
				default:
					break;
			}

			switch (GetToggleTransition(SettingKeys.ReduceExpansionCost))
			{
				case ToggleTransitionEnum.JustEnabled:
					Helpers.TryExecute("ReduceExpansionCost", CompanyFeatures.ApplyReduceExpansionCost);
					break;
				case ToggleTransitionEnum.JustDisabled:
					Helpers.TryExecute("ReduceExpansionCost", CompanyFeatures.RestoreReduceExpansionCost);
					break;
				default:
					break;
			}

			_minuteWatcher.Poll();

			// One TryExecute boundary per feature; a failing feature does not stop unrelated ones.
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

			// Non-capturing lambdas are cached by the compiler; a method group would allocate a delegate every frame.
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableForcePause))
			{
				Helpers.TryExecute("DisableForcePause", () => CompanyFeatures.ApplyDisableForcePause());
			}

			if (Helpers.GetProperty(TrainerSettings, SettingKeys.DisableForceFreeze))
			{
				Helpers.TryExecute("DisableForceFreeze", () => CompanyFeatures.ApplyDisableForceFreeze());
			}

			// AddHeat triggers audits synchronously, so clear it every frame.
			if (Helpers.GetProperty(TrainerSettings, SettingKeys.NoOffshoreHeat))
			{
				Helpers.TryExecute("NoOffshoreHeat", () => CompanyFeatures.ApplyNoOffshoreHeat());
			}

		}
	}
}
