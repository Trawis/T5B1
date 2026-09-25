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
		private static bool _specializationsLoaded;
		private bool _sceneEventsSubscribed;
		private bool _timeEventsSubscribed;

		// Used by ToggleJustEnabled to apply reduced-cadence toggles immediately on enable.
		private readonly Dictionary<string, bool> _reducedCadenceToggleState = new Dictionary<string, bool>();

		// TimeOfDay has no minute-passed event; this raises one from its Minute field.
		private readonly GameMinuteWatcher _minuteWatcher = new GameMinuteWatcher();

		private static bool IsGameReady(bool requireSelector = false) =>
			Helpers.IsGameLoaded && (!requireSelector || SelectorController.Instance != null);
		private float _defaultEnvironmentISPCostFactor;

		// Settings the game itself only ever sets once (ctor/new-game/save-load).
		private bool _oneTimeSettingsApplied;

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

			_timeEventsSubscribed = false;
		}

		private void OnHourPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, "MoreHostingDeals"))
			{
				Helpers.TryExecute("MoreHostingDeals", ApplyHostingDealTiming);
			}

			if (Helpers.GetProperty(TrainerSettings, "FreeStaff"))
			{
				Helpers.TryExecute("FreeStaff", () => Settings.StaffSalaryDue = 0f);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoServerCost"))
			{
				Helpers.TryExecute("NoServerCost", () => Settings.ServerCost = 0f);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoWaterElectricity"))
			{
				Helpers.TryExecute("NoWaterElectricity", () =>
				{
					Settings.ElectricityBill = 0f;
					Settings.Waterbill = 0f;
					Settings.Gasbill = 0f;
				});
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoMaxMarketShare"))
			{
				// Disabled, see #150
				//Settings.MyCompany.Products.ForEach(product => product.MarketShare = 1f);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoMaintenance"))
			{
				Helpers.TryExecute("NoMaintenance", ApplyNoMaintenance);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoMissedSupportTickets"))
			{
				Helpers.TryExecute("NoMissedSupportTickets", ApplyNoMissedSupportTickets);
			}

			if (Helpers.GetProperty(TrainerSettings, "FreePrint"))
			{
				Helpers.TryExecute("FreePrint", ApplyFreePrint);
			}

			if (Helpers.GetProperty(TrainerSettings, "IncreasePrintSpeed"))
			{
				Helpers.TryExecute("IncreasePrintSpeed", ApplyIncreasePrintSpeed);
			}
		}

		private void OnMinutePassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, "AutoEndDesign"))
			{
				Helpers.TryExecute("AutoEndDesign", ApplyAutoEndDesign);
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoContractProgression"))
			{
				Helpers.TryExecute("AutoContractProgression", ApplyAutoContractProgression);
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoEndResearch"))
			{
				Helpers.TryExecute("AutoEndResearch", ApplyAutoEndResearch);
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoEndPatent"))
			{
				Helpers.TryExecute("AutoEndPatent", ApplyAutoEndPatent);
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoResearchStart"))
			{
				Helpers.TryExecute("AutoResearchStart", ApplyAutoResearchStart);
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoAcceptHostingDeals"))
			{
				Helpers.TryExecute("AutoAcceptHostingDeals", ApplyAutoAcceptHostingDeals);
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoPorting"))
			{
				Helpers.TryExecute("AutoPorting", ApplyAutoPorting);
			}

			bool fullEnvironment = Helpers.GetProperty(TrainerSettings, "FullEnvironment");
			bool fullRoomBrightness = Helpers.GetProperty(TrainerSettings, "FullRoomBrightness");
			bool increaseBookshelfSkill = Helpers.GetProperty(TrainerSettings, "IncreaseBookshelfSkill");
			bool noWaterElectricity = Helpers.GetProperty(TrainerSettings, "NoWaterElectricity");
			bool disableFires = Helpers.GetProperty(TrainerSettings, "DisableFires");

			if (fullEnvironment || fullRoomBrightness)
			{
				for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
				{
					Room room = Settings.sRoomManager.Rooms[i];

					if (fullEnvironment)
					{
						Helpers.TryExecute("FullEnvironment", () => ApplyFullEnvironmentToRoom(room));
					}

					if (fullRoomBrightness)
					{
						Helpers.TryExecute("FullRoomBrightness", () => ApplyFullRoomBrightnessToRoom(room));
					}
				}
			}

			if (increaseBookshelfSkill || noWaterElectricity || disableFires)
			{
				foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
				{
					if (increaseBookshelfSkill)
					{
						Helpers.TryExecute("IncreaseBookshelfSkill", () => ApplyIncreaseBookshelfSkillToFurniture(furniture));
					}

					if (noWaterElectricity)
					{
						Helpers.TryExecute("NoWaterElectricity", () => ApplyNoWaterElectricityToFurniture(furniture));
					}

					if (disableFires)
					{
						Helpers.TryExecute("DisableFires", () => ApplyDisableFiresFireStarterToFurniture(furniture));
					}
				}
			}

			bool increaseWalkSpeed = Helpers.GetProperty(TrainerSettings, "IncreaseWalkSpeed");
			Helpers.TryExecute("IncreaseWalkSpeed", () =>
			{
				for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
				{
					Settings.sActorManager.Actors[i].WalkSpeed = increaseWalkSpeed ? Constants.WALK_SPEED_BOOSTED : Constants.WALK_SPEED_DEFAULT;
				}
			});

			Helpers.TryExecute("IncreaseCourierCapacity", () =>
			{
				AI.MaxBoxes = Helpers.GetProperty(TrainerSettings, "IncreaseCourierCapacity") ? Constants.MAX_BOXES_BOOSTED : Constants.MAX_BOXES_DEFAULT;
				AI.MaxBoxCarry = Helpers.GetProperty(TrainerSettings, "IncreaseCourierCapacity") ? Constants.MAX_CARRY_BOOSTED : Constants.MAX_CARRY_DEFAULT;
			});
			//Not working
			//AI.BoxPrice = Helpers.GetProperty(TrainerSettings, "ReduceBoxPrice") ? 62.5f : 125;
			Helpers.TryExecute("ReduceISPCost", () => Settings.Environment.ISPCostFactor = Helpers.GetProperty(TrainerSettings, "ReduceISPCost") ? _defaultEnvironmentISPCostFactor / 2f : _defaultEnvironmentISPCostFactor);
			Helpers.TryExecute("ReduceExpansionCost", () => Settings.ExpansionCost = Helpers.GetProperty(TrainerSettings, "ReduceExpansionCost") ? Constants.EXPANSION_COST_HALF : Constants.EXPANSION_COST);
		}

		private bool ToggleJustEnabled(string settingKey)
		{
			bool isEnabled = Helpers.GetProperty(TrainerSettings, settingKey);
			bool wasEnabled;
			_reducedCadenceToggleState.TryGetValue(settingKey, out wasEnabled);
			_reducedCadenceToggleState[settingKey] = isEnabled;

			return isEnabled && !wasEnabled;
		}

		// Company.Bankrupt is recomputed daily (MarketSimulation.SimulateMonth runs from
		// TimeOfDay.UpdateDay despite the name), not monthly.
		private void OnDayPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, "DigitalDistributionMonopol"))
			{
				Helpers.TryExecute("DigitalDistributionMonopol", ApplyDigitalDistributionMonopol);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoInsuranceCost"))
			{
				Helpers.TryExecute("NoInsuranceCost", ApplyNoInsuranceCost);
			}

			if (Helpers.GetProperty(TrainerSettings, "FreeMarketing"))
			{
				Helpers.TryExecute("FreeMarketing", ApplyFreeMarketing);
			}
		}

		private void OnMonthPassed(object obj, EventArgs args)
		{
			if (Helpers.GetProperty(TrainerSettings, "FreeEmployees"))
			{
				Helpers.TryExecute("FreeEmployees", ApplyFreeEmployees);
			}

			if (Helpers.GetProperty(TrainerSettings, "MoreCreativity"))
			{
				Helpers.TryExecute("MoreCreativity", ApplyMoreCreativity);
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableBurglars"))
			{
				Helpers.TryExecute("DisableBurglars", ApplyDisableBurglars);
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableFireInspection"))
			{
				Helpers.TryExecute("DisableFireInspection", ApplyDisableFireInspection);
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableFurnitureStealing"))
			{
				Helpers.TryExecute("DisableFurnitureStealing", ApplyDisableFurnitureStealing);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoVacation"))
			{
				Helpers.TryExecute("NoVacation", ApplyNoVacation);
			}

			if (Helpers.GetProperty(TrainerSettings, "LockAge"))
			{
				Helpers.TryExecute("LockAge", () => Settings.sActorManager.Actors.ForEach(x => x.employee.BirthDate += 1));
			}

			if (Helpers.GetProperty(TrainerSettings, "NoLoanInterest"))
			{
				Helpers.TryExecute("NoLoanInterest", ApplyNoLoanInterest);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoFounderDividends"))
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

			if (!_specializationsLoaded && Settings.MyCompany != null)
			{
				Helpers.TryExecute("LoadSpecializations", () =>
				{
					LoadSpecializations();
					ShowDiscordInvite(displayAsPopup: true);
				});
			}

			if (!_oneTimeSettingsApplied)
			{
				_defaultEnvironmentISPCostFactor = Settings.Environment.ISPCostFactor;
				GameSettings.MaxFloor = Constants.MAX_FLOOR;

				// Cheats.* are plain static fields with no save-file backing, so they reset to
				// false on every game process start regardless of what the trainer's own
				// (persisted) settings say. Re-push them once per load so a setting that was
				// enabled before a restart takes effect again.
				Helpers.TryExecute("ForceLights", () => ApplyForceLights(Helpers.GetProperty(TrainerSettings, "ForceLights")));
				Helpers.TryExecute("ShowRoomCeilings", () => ApplyShowRoomCeilings(Helpers.GetProperty(TrainerSettings, "ShowRoomCeilings")));
				Helpers.TryExecute("UnlimitedSubsidiaries", () => ApplyUnlimitedSubsidiaries(Helpers.GetProperty(TrainerSettings, "UnlimitedSubsidiaries")));

				_oneTimeSettingsApplied = true;
			}

			if (ToggleJustEnabled("NoMaintenance"))
			{
				Helpers.TryExecute("NoMaintenance", ApplyNoMaintenance);
			}

			if (ToggleJustEnabled("NoVacation"))
			{
				Helpers.TryExecute("NoVacation", ApplyNoVacation);
			}

			if (ToggleJustEnabled("MoreHostingDeals"))
			{
				Helpers.TryExecute("MoreHostingDeals", ApplyHostingDealTiming);
			}

			if (ToggleJustEnabled("AutoEndDesign"))
			{
				Helpers.TryExecute("AutoEndDesign", ApplyAutoEndDesign);
			}

			if (ToggleJustEnabled("AutoContractProgression"))
			{
				Helpers.TryExecute("AutoContractProgression", ApplyAutoContractProgression);
			}

			if (ToggleJustEnabled("AutoEndResearch"))
			{
				Helpers.TryExecute("AutoEndResearch", ApplyAutoEndResearch);
			}

			if (ToggleJustEnabled("AutoEndPatent"))
			{
				Helpers.TryExecute("AutoEndPatent", ApplyAutoEndPatent);
			}

			if (ToggleJustEnabled("FreeStaff"))
			{
				Helpers.TryExecute("FreeStaff", () => Settings.StaffSalaryDue = 0f);
			}

			if (ToggleJustEnabled("NoServerCost"))
			{
				Helpers.TryExecute("NoServerCost", () => Settings.ServerCost = 0f);
			}

			if (ToggleJustEnabled("NoWaterElectricity"))
			{
				Helpers.TryExecute("NoWaterElectricity", () =>
				{
					Settings.ElectricityBill = 0f;
					Settings.Waterbill = 0f;
					Settings.Gasbill = 0f;
					Settings.sRoomManager.AllFurniture.ForEach(ApplyNoWaterElectricityToFurniture);
				});
			}

			if (ToggleJustEnabled("DisableFurnitureStealing"))
			{
				Helpers.TryExecute("DisableFurnitureStealing", ApplyDisableFurnitureStealing);
			}

			if (ToggleJustEnabled("NoEducationCost"))
			{
				Helpers.TryExecute("NoEducationCost", () => EducationWindow.EdCost = new[] { 0f, 0f, 0f });
			}

			if (ToggleJustEnabled("AutoResearchStart"))
			{
				Helpers.TryExecute("AutoResearchStart", ApplyAutoResearchStart);
			}

			if (ToggleJustEnabled("DigitalDistributionMonopol"))
			{
				Helpers.TryExecute("DigitalDistributionMonopol", ApplyDigitalDistributionMonopol);
			}

			if (ToggleJustEnabled("AutoMaxMarketShare"))
			{
				// Disabled, see #150
				//Settings.MyCompany.Products.ForEach(product => product.MarketShare = 1f);
			}

			if (ToggleJustEnabled("AutoAcceptHostingDeals"))
			{
				Helpers.TryExecute("AutoAcceptHostingDeals", ApplyAutoAcceptHostingDeals);
			}

			if (ToggleJustEnabled("FreeEmployees"))
			{
				Helpers.TryExecute("FreeEmployees", ApplyFreeEmployees);
			}

			if (ToggleJustEnabled("MoreCreativity"))
			{
				Helpers.TryExecute("MoreCreativity", ApplyMoreCreativity);
			}

			if (ToggleJustEnabled("DisableBurglars"))
			{
				Helpers.TryExecute("DisableBurglars", ApplyDisableBurglars);
			}

			if (ToggleJustEnabled("DisableFireInspection"))
			{
				Helpers.TryExecute("DisableFireInspection", ApplyDisableFireInspection);
			}

			if (ToggleJustEnabled("FreePrint"))
			{
				Helpers.TryExecute("FreePrint", ApplyFreePrint);
			}

			if (ToggleJustEnabled("IncreasePrintSpeed"))
			{
				Helpers.TryExecute("IncreasePrintSpeed", ApplyIncreasePrintSpeed);
			}

			if (ToggleJustEnabled("FullEnvironment"))
			{
				Helpers.TryExecute("FullEnvironment", () =>
				{
					for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
					{
						ApplyFullEnvironmentToRoom(Settings.sRoomManager.Rooms[i]);
					}
				});
			}

			if (ToggleJustEnabled("FullRoomBrightness"))
			{
				Helpers.TryExecute("FullRoomBrightness", () =>
				{
					for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
					{
						ApplyFullRoomBrightnessToRoom(Settings.sRoomManager.Rooms[i]);
					}
				});
			}

			if (ToggleJustEnabled("IncreaseBookshelfSkill"))
			{
				Helpers.TryExecute("IncreaseBookshelfSkill", () => Settings.sRoomManager.AllFurniture.ForEach(ApplyIncreaseBookshelfSkillToFurniture));
			}

			if (ToggleJustEnabled("DisableFires"))
			{
				Helpers.TryExecute("DisableFires", () => Settings.sRoomManager.AllFurniture.ForEach(ApplyDisableFiresFireStarterToFurniture));
			}

			if (ToggleJustEnabled("IncreaseWalkSpeed"))
			{
				Helpers.TryExecute("IncreaseWalkSpeed", () =>
				{
					for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
					{
						Settings.sActorManager.Actors[i].WalkSpeed = Constants.WALK_SPEED_BOOSTED;
					}
				});
			}

			if (ToggleJustEnabled("IncreaseCourierCapacity"))
			{
				Helpers.TryExecute("IncreaseCourierCapacity", () =>
				{
					AI.MaxBoxes = Constants.MAX_BOXES_BOOSTED;
					AI.MaxBoxCarry = Constants.MAX_CARRY_BOOSTED;
				});
			}

			if (ToggleJustEnabled("ReduceISPCost"))
			{
				Helpers.TryExecute("ReduceISPCost", () => Settings.Environment.ISPCostFactor = _defaultEnvironmentISPCostFactor / 2f);
			}

			if (ToggleJustEnabled("ReduceExpansionCost"))
			{
				Helpers.TryExecute("ReduceExpansionCost", () => Settings.ExpansionCost = Constants.EXPANSION_COST_HALF);
			}

			_minuteWatcher.Poll();

			bool fullSatisfaction = Helpers.GetProperty(TrainerSettings, "FullSatisfaction");
			bool noSickness = Helpers.GetProperty(TrainerSettings, "NoSickness");
			bool cleanRooms = Helpers.GetProperty(TrainerSettings, "CleanRooms");

			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
				if (Helpers.GetProperty(TrainerSettings, "NoiseReduction"))
				{
					Helpers.TryExecute("NoiseReduction", () =>
					{
						furniture.ActorNoise = 0f;
						furniture.EnvironmentNoise = 0f;
						furniture.FinalNoise = 0f;
						furniture.Noisiness = 0;
					});
				}

				if (Helpers.GetProperty(TrainerSettings, "DisableFires") && furniture.Parent.IsOnFire)
				{
					Helpers.TryExecute("DisableFires", () =>
					{
						if (furniture.Parent.Temperature > 40f)
						{
							furniture.Parent.Temperature = 21f;
						}
						furniture.Parent.StopFire();
					});
				}
			}

			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				Room room = Settings.sRoomManager.Rooms[i];

				if (cleanRooms)
				{
					Helpers.TryExecute("CleanRooms", () => ApplyCleanRoomsToRoom(room));
				}

				if (noSickness)
				{
					Helpers.TryExecute("NoSickness", () => ApplyNoSicknessToRoom(room));
				}

				if (Helpers.GetProperty(TrainerSettings, "TemperatureLock"))
				{
					Helpers.TryExecute("TemperatureLock", () => room.Temperature = 21f);
				}
			}

			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				Employee employee = actor.employee;

				if (fullSatisfaction)
				{
					Helpers.TryExecute("FullSatisfaction", () => ApplyFullSatisfactionToActor(actor));
				}

				if (noSickness)
				{
					Helpers.TryExecute("NoSickness", () => ApplyNoSicknessToActor(actor));
				}

				if (Helpers.GetProperty(TrainerSettings, "NoStress"))
				{
					Helpers.TryExecute("NoStress", () => employee.Stress = 1f);
				}

				if (employee.RoleString.Contains("Lead") && Helpers.GetProperty(StoresSettings, "LeadEfficiencyStore") != null)
				{
					Helpers.TryExecute("LeadEfficiencyStore", () => actor.Effectiveness = Helpers.GetProperty(StoresSettings, "LeadEfficiencyStore").MakeFloat());
				}

				if (!employee.RoleString.Contains("Lead") && Helpers.GetProperty(StoresSettings, "EfficiencyStore") != null)
				{
					Helpers.TryExecute("EfficiencyStore", () => actor.Effectiveness = Helpers.GetProperty(StoresSettings, "EfficiencyStore").MakeFloat());
				}

				if (Helpers.GetProperty(TrainerSettings, "NoNeeds"))
				{
					Helpers.TryExecute("NoNeeds", () =>
					{
						actor.NextSmell = 0f;
						employee.Bladder = 1f;
						employee.Hunger = 1f;
						employee.Energy = 1f;
						employee.Social = 1f;
						employee.Posture = 1f;
						employee.ActiveComplaint = false;
						employee.HadProperFood = true;
					});
				}

				if (Helpers.GetProperty(TrainerSettings, "NoiseReduction"))
				{
					Helpers.TryExecute("NoiseReduction", () => actor.Noisiness = 0);
				}

				if (Helpers.GetProperty(TrainerSettings, "MoreInspiration"))
				{
					Helpers.TryExecute("MoreInspiration", () => employee.LastInpirationUse = new SDateTime(0));
				}
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableForcePause"))
			{
				Helpers.TryExecute("DisableForcePause", () => GameSettings.ForcePause = false);
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableForceFreeze"))
			{
				Helpers.TryExecute("DisableForceFreeze", () => GameSettings.FreezeGame = false);
			}

			// AddHeat checks/triggers an audit synchronously, so clearing every frame is the tightest reactive window available.
			if (Helpers.GetProperty(TrainerSettings, "NoOffshoreHeat"))
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

		private static void ApplyCleanRoomsToRoom(Room room)
		{
			room.ClearDirt();
			room.Smell = 0f;
		}

		private static void ApplyNoSicknessToRoom(Room room)
		{
			room.GermCount = 0f;
		}

		private static void ApplyNoSicknessToActor(Actor actor)
		{
			TimeOfDay.Instance.Sick.Clear();

			if (actor.SpecialState == Actor.HomeState.Sick)
			{
				actor.SpecialState = Actor.HomeState.Default;
				actor.WasSick = true;
			}

			actor.GermAdd = 0f;
			actor.GermCount = 0f;
			actor.SickDays = 0;
		}

		private static void ApplyNoMaintenance()
		{
			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
				switch (furniture.Type)
				{
					case "Chair":
						if (furniture.Comfort < Constants.CHAIR_COMFORT_LOW)
						{
							furniture.Comfort = Constants.CHAIR_COMFORT_HIGH;
						}
						goto case "Ventilation";
					case "CCTV":
					case "Computer":
					case "Lamp":
					case "Server":
					case "Product Printer":
					case "Radiator":
					case "Sink":
					case "Toilet":
					case "Ventilation":
						break;
					default:
						break;
				}

				if (furniture.HasUpg && (furniture.upg.Quality < 0.8f || furniture.upg.Broken))
				{
					furniture.upg.RepairMe();
				}
			}
		}

		private static void ApplyFullSatisfactionToActor(Actor actor)
		{
			Employee employee = actor.employee;

			employee.JobSatisfaction = 2f;
			employee.ActiveComplaint = false;
			foreach (var thought in employee.Thoughts.Values.ToList())
			{
				if (thought.Mood.Negative || thought.Mood.Sue || !string.IsNullOrEmpty(thought.Mood.QuitReason))
				{
					employee.Thoughts.Remove(thought.Thought);
				}
			}

			employee.SetMood("LoveWork", actor, 1f);
		}

		private static void ApplyNoVacation()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];

				actor.VacationMonth = SDateTime.NextMonth(24);
				if (actor.SpecialState == Actor.HomeState.Vacation)
					actor.SpecialState = Actor.HomeState.Default;
			}
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

		private static void ApplyFullEnvironmentToRoom(Room room)
		{
			room.FurnEnvironment = Constants.ENV_FULL;
		}

		private static void ApplyFullRoomBrightnessToRoom(Room room)
		{
			room.IndirectLighting = Constants.ROOM_BRIGHTNESS_FULL;
		}

		private static void ApplyIncreaseBookshelfSkillToFurniture(Furniture furniture)
		{
			if (furniture.Type == "Bookshelf")
			{
				furniture.AuraValues[1] = Constants.BOOKSHELF_AURA_BOOSTED;
			}
		}

		private static void ApplyNoWaterElectricityToFurniture(Furniture furniture)
		{
			furniture.Water = 0;
			furniture.Wattage = 0;
		}

		private static void ApplyDisableFiresFireStarterToFurniture(Furniture furniture)
		{
			if (furniture.HasUpg && furniture.upg.FireStarter > 0.0f)
			{
				furniture.upg.FireStarter = 0.0f;
			}
		}

		private static void ApplyFreeEmployees()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				Employee employee = actor.employee;

				actor.NegotiateSalary = false;
				employee.Salary = 0f;
				employee.AskedFor = 0f;
				employee.Demanded = 0f;
				employee.UpfrontDemand = 0f;
				employee.ChangeSalary(0f, 0f, actor, false);
			}
		}

		private static void ApplyMoreCreativity()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Settings.sActorManager.Actors[i].employee.RevealCreativity(1f);
			}
		}

		private static void ApplyDisableBurglars()
		{
			foreach (var burglar in Settings.sActorManager.Others["Burglars"])
			{
				burglar.Despawned = true;
				Settings.sActorManager.RemoveFromAwaiting(burglar);
			}
		}

		private static void ApplyDisableFireInspection()
		{
			foreach (var fireInspector in Settings.sActorManager.Others["FireInspector"])
			{
				fireInspector.Despawned = true;
				Settings.sActorManager.RemoveFromAwaiting(fireInspector);
			}

			Settings.ActiveFireReport.Reset();
			Settings.PassedFireInspection = true;
		}

		//TODO: add printspeed and printprice when it's disabled (else)
		private static void ApplyFreePrint()
		{
			Settings.ProductPrinters.ForEach(p => p.PrintPrice = 0f);
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

		private static void ApplyDisableFurnitureStealing()
		{
			Settings.sRoomManager.AllFurniture.ForEach(x => x.CanSteal = false);
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

		private static void LoadSpecializations()
		{
			if (Helpers.SpecializationsList != null && Helpers.SpecializationsList.Count() > 0)
			{
				return;
			}

			var specializations = new Dictionary<string, bool>();

			foreach (var role in Helpers.RolesList)
			{
				foreach (var specialization in Settings.GetAllSpecializations(role.Key.ToEmployeeRole()))
				{
					if (!specializations.ContainsKey(specialization))
					{
						specializations.Add(specialization, false);
					}
				}
			}

			Helpers.SpecializationsList = specializations;

			_specializationsLoaded = true;
		}

		public static void ShowDiscordInvite(bool displayAsPopup = false)
		{
			string message = $"Join us on our discord server\n{Helpers.DiscordUrl}";
			if (displayAsPopup)
			{
				HUD.Instance.AddPopupMessage(message, "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			}
			else
			{
				WindowManager.SpawnDialog(message, false, DialogWindow.DialogType.Information);
			}
		}

		public static void SetSkillPerEmployeeAction(string input)
		{
			var selectedActors = SelectorController.Instance.Selected.OfType<Actor>().ToList();
			var selectedRoles = Helpers.RolesList.Where(r => r.Value).ToList();
			var selectedSpecializations = Helpers.SpecializationsList.Where(s => s.Value).ToList();

			int amount;
			if (selectedActors.Count == 0)
			{
				WindowManager.SpawnDialog("Select one or more employees.", false, DialogWindow.DialogType.Error);
				return;
			}
			else if (selectedRoles.Count == 0)
			{
				WindowManager.SpawnDialog("Select one or more roles.", false, DialogWindow.DialogType.Error);
				return;
			}
			else if (selectedSpecializations.Count == 0)
			{
				WindowManager.SpawnDialog("Select one or more specializations.", false, DialogWindow.DialogType.Error);
				return;
			}
			else if (!int.TryParse(input, out amount) || amount == 0 || amount < -3 || amount > 3)
			{
				WindowManager.SpawnDialog("Invalid input!\nAllowed inputs are: -3, -2, -1, 1, 2, 3", false, DialogWindow.DialogType.Error);
				return;
			}
			else
			{
				selectedActors.ForEach(actor =>
				{
					foreach (var role in selectedRoles)
					{
						//actor.employee.ChangeSkillDirect(role.Key.ToEmployeeRole(), 1f);

						foreach (var specialization in selectedSpecializations)
						{
							actor.employee.AddSpecialization(role.Key.ToEmployeeRole(), specialization.Key, false, true, amount);
						}
					}
				});

				HUD.Instance.AddPopupMessage("Trainer: Employee skills/specializations are set!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			}
		}

		public static void SetSkillPerEmployee()
		{
			WindowManager.SpawnInputDialog("How many specialization stars do you want?\nMin = -3, Max = 3", "Stars amount", "3", SetSkillPerEmployeeAction);
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

		public static void HREmployees()
		{
			if (!IsGameReady(requireSelector: true)) return;

			Actor[] Actors = Settings.sActorManager.Actors
									 .Where(actor => actor.employee.RoleString.Contains("Lead"))
									 .ToArray();

			if (Actors.Length == 0)
			{
				return;
			}

			for (var i = 0; i < Actors.Length; i++)
			{
				Actors[i].employee.SetSpecialization(Actors[i].employee.GetRoleOrNatural(), "HR", 5);
			}

			HUD.Instance.AddPopupMessage("Trainer: All leaders are now HRed!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0, 1);
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

		public static void ResetAgeOfEmployees()
		{
			var currentTime = SDateTime.Now();
			foreach (var actor in Settings.sActorManager.Actors)
			{
				actor.employee.BirthDate = currentTime - Employee.Youngest * 12;
				actor.UpdateAgeLook();
			}

			HUD.Instance.AddPopupMessage("Trainer: Employees age has been reset!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void EmployeesToMax()
		{
			SoftwareType[] softwareTypes = MarketSimulation.Active.SoftwareTypes.Values.ToArray();
			Employee.EmployeeRole[] employeeRoles = (Employee.EmployeeRole[])Enum.GetValues(typeof(Employee.EmployeeRole));

			if (!IsGameReady(requireSelector: true)) return;

			foreach (Actor actor in Settings.sActorManager.Actors.ToArray())
			{
				actor.employee.CreativityKnown = 1f;

				foreach (SoftwareType t in softwareTypes)
				{
					actor.employee.LeadSpecializationFix[t.ToString()] = 1f;
				}

				foreach (Employee.EmployeeRole employeeRole in employeeRoles)
				{
					actor.employee.ChangeSkillDirect(employeeRole, 1f);

					string[] specializations = Settings.GetAllSpecializations(employeeRole);
					foreach (string specialization in specializations)
					{
						actor.employee.AddSpecialization(employeeRole, specialization, false, true, 3);
					}
				}
			}

			HUD.Instance.AddPopupMessage("Trainer: All employees are now max skilled!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0, 1);
		}

		public static void UnlockAllSpace()
		{
			if (!IsGameReady()) return;

			Example.TakeAllLand();
			HUD.Instance.AddPopupMessage("Trainer: All plots has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void UnlockFurniture()
		{
			if (!IsGameReady()) return;

			Example.UnlockFurniture();
			Cheats.UnlockFurn = true;
			HUD.Instance.UpdateFurnitureButtons();
			HUD.Instance.AddPopupMessage("Trainer: All furniture has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		#region Native Cheat Flags

		// Cheats.ForceLights (read live in LampScript.UpdateNow / RoadLightScript.ToggleNow),
		// Cheats.CeilingMeshes (read live in Room.GenerateOuterWalls / GenerateInnerPolygon),
		// and Cheats.InfiniteSubs (read in CompanyDetailWindow.TakeOverSub's subsidiary-count
		// gate) are all consulted directly by the game whenever relevant, so the trainer binds
		// the native flag the moment the setting changes instead of polling for it every frame.
		// Cheats.DisableDarkness has no readers anywhere in the vendored assembly and is
		// intentionally left unexposed - setting it would have no observable effect.

		public static void ApplyForceLights(bool enabled)
		{
			Cheats.ForceLights = enabled;
		}

		public static void ApplyShowRoomCeilings(bool enabled)
		{
			Cheats.CeilingMeshes = enabled;

			// Mirrors CameraScript.RefreshFlyMode, the only other place the game itself flips
			// this flag: marking every room dirty forces outer/inner meshes (the ceiling mesh
			// included) to regenerate, so already-built rooms reflect the change immediately
			// instead of only rooms built or modified afterward.
			if (Helpers.IsGameLoaded)
			{
				foreach (Room room in Settings.sRoomManager.Rooms)
				{
					room.DirtyOuterMesh = true;
					room.DirtyInnerMesh = true;
				}
			}
		}

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
