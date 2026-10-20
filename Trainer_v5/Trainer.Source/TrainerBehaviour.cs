using System;
using System.Collections.Generic;
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

		private FeatureDispatcher _dispatcher;

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
			_toggleStates.Reset();
			UnsubscribeFromEvents();
			UnsubscribeFromSceneEvents();
		}

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
				switch (SceneRoutes.For(scene.name))
				{
					case SceneRoute.MainScene:
						// First, so a UI failure below cannot leave the time events unsubscribed.
						SubscribeToEvents();
						ResetPerGameState();
						Main.CreateUIButtons();
						// Immediate attempt; GameSettings.GameReady is the reliable trigger.
						DetailWindowTrainer.Install();
						break;
					case SceneRoute.Customization:
						ActorCustomization.StartYears = new[] { 1970, 1975, 1980, 1985, 1990, 1995, 2000, 2005, 2010, 2015, 2020, 2025, 2030, 2035, 2040, 2045, 2050, 2060, 2070, 2080, 2090, 2100 };
						ActorCustomization.StartLoans = new[] { 0, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000, 5000000, 10000000 };
						break;
					default:
						LeaveGame();
						break;
				}
			}
		}

		private void LeaveGame()
		{
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
			ResetPerGameState();
			_toggleStates.Reset();
			UnsubscribeFromEvents();
		}

		// UI and runtime caches of one game; persisted trainer settings are deliberately not touched.
		private void ResetPerGameState()
		{
			SettingsWindow.Reset();
			EmployeeSkillChangeWindow.Reset();
			EmployeeFeatures.ResetSpecializations();
			MarketFeatures.ResetGameState();
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
			Helpers.TryExecute("LoadSpecializations", EmployeeFeatures.RestoreSpecializationsAfterReset);
		}

		private void OnHourPassed(object obj, EventArgs args)
		{
			GetDispatcher().Run(FeatureTrigger.Hour);
		}

		private void OnMinutePassed(object obj, EventArgs args)
		{
			GetDispatcher().Run(FeatureTrigger.Minute);
		}

		private FeatureDispatcher GetDispatcher()
		{
			if (_dispatcher == null)
			{
				_dispatcher = new FeatureDispatcher(
					FeatureRegistry.Create(),
					key => Helpers.GetProperty(TrainerSettings, key),
					key => Helpers.GetProperty(StoresSettings, key) != null,
					_toggleStates,
					Helpers.TryExecute);
			}
			return _dispatcher;
		}

		private ToggleTransitionEnum GetToggleTransition(ToggleKey settingKey)
		{
			return _toggleStates.Observe(settingKey.Key, Helpers.GetProperty(TrainerSettings, settingKey));
		}

		// Company.Bankrupt is recomputed daily (SimulateMonth runs from UpdateDay), not monthly.
		private void OnDayPassed(object obj, EventArgs args)
		{
			GetDispatcher().Run(FeatureTrigger.Day);
		}

		private void OnMonthPassed(object obj, EventArgs args)
		{
			GetDispatcher().Run(FeatureTrigger.Month);
		}

		private void Update()
		{
			if (!isActiveAndEnabled || !Helpers.IsGameLoaded)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.F1))
			{
				Main.OpenSettingsWindow();
			}

			if (Input.GetKeyDown(KeyCode.F2))
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

			ApplyProcessStaticSettings();

			GetDispatcher().RunTransitions();

			_minuteWatcher.Poll();

			GetDispatcher().Run(FeatureTrigger.Frame);
		}

		private void ApplyProcessStaticSettings()
		{
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
		}
	}
}
