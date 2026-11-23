using System;
using System.Collections.Generic;
using Trainer_v5.Experimental;
using Trainer_v5.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Trainer_v5
{
	public class TrainerBehaviour : ModBehaviour
	{
		private bool _sceneEventsSubscribed;
		private bool _timeEventsSubscribed;

		private readonly ToggleStateTracker _toggleStates = new ToggleStateTracker();

		private FeatureDispatcher _dispatcher;

		private readonly GameMinuteWatcher _minuteWatcher = new GameMinuteWatcher();

		private static GameSettings Settings => GameSettings.Instance;

		public override void OnActivate()
		{
			SubscribeToSceneEvents();

			// No sceneLoaded event fires on reactivation inside MainScene, so subscribe directly.
			if (SceneManager.GetActiveScene().name == "MainScene")
			{
				SubscribeToEvents();
				// A game loaded while the mod was inactive never reached Main.Deserialize.
				Helpers.TryExecute("EstablishSettings", EstablishSettingsForGame);
			}
		}

		public override void OnDeactivate()
		{
			SettingsSession.Deactivated();
			_toggleStates.Reset();
			UnsubscribeFromEvents();
			UnsubscribeFromSceneEvents();
		}

		private void Start()
		{
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
						SettingsSession.BeginGame();
						ResetPerGameState();
						Main.CreateUIButtons();
						// Immediate attempt; GameSettings.GameReady is the reliable trigger.
						EmployeesWindowTrainer.Install();
						ProductsWindowTrainer.Install();
						CompaniesWindowTrainer.Install();
						break;
					case SceneRoute.Customization:
						ActorCustomization.StartYears = Constants.CUSTOMIZATION_START_YEARS;
						ActorCustomization.StartLoans = Constants.CUSTOMIZATION_START_LOANS;
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

			EmployeesWindowTrainer.Reset();
			ProductsWindowTrainer.Reset();
			CompaniesWindowTrainer.Reset();
			ResetPerGameState();
			_toggleStates.Reset();
			UnsubscribeFromEvents();
		}

		// The first time this game is seen without a full load having given it its settings, they come from its own
		// ModData or the declared defaults, never from the previous game.
		private void EstablishSettingsForGame()
		{
			GameSettings game = Settings;
			if (game == null || !SettingsSession.NeedsDefaults(game.GetInstanceID()))
			{
				return;
			}

			Main.ApplyForGame(game.ModData);
			if (LoggingOptions.DebugEnabled)
			{
				"settings established for a game without a trainer load".LogDebug();
			}
		}

		private void ResetPerGameState()
		{
			SettingsWindow.Reset();
			EmployeeSkillChangeWindow.Reset();
			EmployeeFeatures.ResetSpecializations();
			MarketFeatures.ResetGameState();
			ProductFeatures.ResetGameState();

			if (LoggingOptions.DebugEnabled)
			{
				"per-game state reset".LogDebug();
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
			if (LoggingOptions.DebugEnabled)
			{
				"game ready".LogDebug();
			}

			Helpers.TryExecute("EstablishSettings", EstablishSettingsForGame);

			if (!ExperimentalOptions.Enabled)
			{
				ExperimentalToggles.TurnOffAll(Helpers.Settings);
			}

			EmployeesWindowTrainer.Install();
			ProductsWindowTrainer.Install();
			CompaniesWindowTrainer.Install();
			Helpers.TryExecute("LoadSpecializations", EmployeeFeatures.RestoreSpecializationsAfterReset);
			Helpers.TryExecute("RepairLegacyHostingRequests", () =>
			{
				if (MultiplayerRule.Active)
				{
					return;
				}

				var repaired = MarketFeatures.RepairLegacyHostingRequests();
				if (LoggingOptions.DebugEnabled)
				{
					("legacy hosting deals repaired: " + repaired).LogDebug();
				}
			});
		}

		private void OnHourPassed(object obj, EventArgs args)
		{
			if (!Helpers.IsSimulationReady())
			{
				return;
			}

			GetDispatcher().Run(FeatureTrigger.Hour);
		}

		private void OnMinutePassed(object obj, EventArgs args)
		{
			if (!Helpers.IsSimulationReady())
			{
				return;
			}

			GetDispatcher().Run(FeatureTrigger.Minute);
		}

		private FeatureDispatcher GetDispatcher()
		{
			if (_dispatcher == null)
			{
				List<FeatureEntry> entries = FeatureRegistry.Create();
				_dispatcher = new FeatureDispatcher(
					entries,
					MultiplayerRule.IsToggleOn,
					MultiplayerRule.IsStoreSet,
					_toggleStates,
					Helpers.TryExecute);

				if (LoggingOptions.DebugEnabled)
				{
					("dispatcher created: " + entries.Count + " entries").LogDebug();
				}
			}
			return _dispatcher;
		}

		// Company.Bankrupt is recomputed daily (SimulateMonth runs from UpdateDay), not monthly.
		private void OnDayPassed(object obj, EventArgs args)
		{
			if (!Helpers.IsSimulationReady())
			{
				return;
			}

			GetDispatcher().Run(FeatureTrigger.Day);
		}

		private void OnMonthPassed(object obj, EventArgs args)
		{
			if (!Helpers.IsSimulationReady())
			{
				return;
			}

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

			// The game reads MaxFloor while it loads the rooms, before the simulation is ready.
			Helpers.TryExecute("MaxFloor", EnvironmentFeatures.ApplyMaxFloor);

			// Loading and pre-simulation still run: nothing below may touch the game or consume a toggle transition yet.
			if (!Helpers.IsSimulationReady())
			{
				return;
			}

			if (!EmployeeFeatures.SpecializationsLoaded && Settings.MyCompany != null)
			{
				Helpers.TryExecute("LoadSpecializations", () =>
				{
					EmployeeFeatures.LoadSpecializations();
					Helpers.ShowDiscordInvite(displayAsPopup: true);
				});
			}

			// Each shared step has its own boundary so a failure cannot skip the Frame run below.
			Helpers.TryExecute("ToggleTransitions", () => GetDispatcher().RunTransitions());

			Helpers.TryExecute("MinutePoll", () => _minuteWatcher.Poll());

			GetDispatcher().Run(FeatureTrigger.Frame);
		}
	}
}
