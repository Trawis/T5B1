using System;
using System.Collections.Generic;
using System.Linq;
using OrbCreationExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Trainer_v5.Actions; // Add using directive for the new Actions namespace
using Random = System.Random;

namespace Trainer_v5
{
	public class TrainerBehaviour : ModBehaviour
	{
		private static bool _specializationsLoaded;
		private float _defaultEnvironmentISPCostFactor;

		private static GameSettings Settings => GameSettings.Instance;
		private static Dictionary<string, bool> TrainerSettings => Helpers.Settings;
		private static Dictionary<string, object> StoresSettings => Helpers.StoresSettings;

		// Properties to simplify settings access
		private bool LockAgeEnabled => Helpers.GetProperty(TrainerSettings, "LockAge");
		private bool NoiseReductionEnabled => Helpers.GetProperty(TrainerSettings, "NoiseReduction");
		private bool NoWaterElectricityEnabled => Helpers.GetProperty(TrainerSettings, "NoWaterElectricity");
		private bool DisableFiresEnabled => Helpers.GetProperty(TrainerSettings, "DisableFires");
		private bool IncreaseBookshelfSkillEnabled => Helpers.GetProperty(TrainerSettings, "IncreaseBookshelfSkill");
		private bool NoMaintenanceEnabled => Helpers.GetProperty(TrainerSettings, "NoMaintenance");
		private bool DisableFurnitureStealingEnabled => Helpers.GetProperty(TrainerSettings, "DisableFurnitureStealing");
		private bool CleanRoomsEnabled => Helpers.GetProperty(TrainerSettings, "CleanRooms");
		private bool TemperatureLockEnabled => Helpers.GetProperty(TrainerSettings, "TemperatureLock");
		private bool FullEnvironmentEnabled => Helpers.GetProperty(TrainerSettings, "FullEnvironment");
		private bool FullRoomBrightnessEnabled => Helpers.GetProperty(TrainerSettings, "FullRoomBrightness");
		private bool NoSicknessEnabled => Helpers.GetProperty(TrainerSettings, "NoSickness");
		private bool NoStressEnabled => Helpers.GetProperty(TrainerSettings, "NoStress");
		private bool FullSatisfactionEnabled => Helpers.GetProperty(TrainerSettings, "FullSatisfaction");
		private bool NoNeedsEnabled => Helpers.GetProperty(TrainerSettings, "NoNeeds");
		private bool FreeEmployeesEnabled => Helpers.GetProperty(TrainerSettings, "FreeEmployees");
		private bool NoVacationEnabled => Helpers.GetProperty(TrainerSettings, "NoVacation");
		private bool MoreInspirationEnabled => Helpers.GetProperty(TrainerSettings, "MoreInspiration");
		private bool MoreCreativityEnabled => Helpers.GetProperty(TrainerSettings, "MoreCreativity");
		private bool IncreaseWalkSpeedEnabled => Helpers.GetProperty(TrainerSettings, "IncreaseWalkSpeed");
		private bool MoreHostingDealsEnabled => Helpers.GetProperty(TrainerSettings, "MoreHostingDeals");
		private bool DisableBurglarsEnabled => Helpers.GetProperty(TrainerSettings, "DisableBurglars");
		private bool AutoEndDesignEnabled => Helpers.GetProperty(TrainerSettings, "AutoEndDesign");
		private bool AutoEndResearchEnabled => Helpers.GetProperty(TrainerSettings, "AutoEndResearch");
		private bool AutoEndPatentEnabled => Helpers.GetProperty(TrainerSettings, "AutoEndPatent");
		private bool FreePrintEnabled => Helpers.GetProperty(TrainerSettings, "FreePrint");
		private bool IncreasePrintSpeedEnabled => Helpers.GetProperty(TrainerSettings, "IncreasePrintSpeed");
		private bool NoEducationCostEnabled => Helpers.GetProperty(TrainerSettings, "NoEducationCost");
		private bool FreeStaffEnabled => Helpers.GetProperty(TrainerSettings, "FreeStaff");
		private bool NoServerCostEnabled => Helpers.GetProperty(TrainerSettings, "NoServerCost");
		private bool DisableFireInspectionEnabled => Helpers.GetProperty(TrainerSettings, "DisableFireInspection");
		private bool DisableForcePauseEnabled => Helpers.GetProperty(TrainerSettings, "DisableForcePause");
		private bool DisableForceFreezeEnabled => Helpers.GetProperty(TrainerSettings, "DisableForceFreeze");
		private bool AutoResearchStartEnabled => Helpers.GetProperty(TrainerSettings, "AutoResearchStart");
		private bool DigitalDistributionMonopolyEnabled => Helpers.GetProperty(TrainerSettings, "DigitalDistributionMonopol");
		private bool AutoAcceptHostingDealsEnabled => Helpers.GetProperty(TrainerSettings, "AutoAcceptHostingDeals");
		private bool IncreaseCourierCapacityEnabled => Helpers.GetProperty(TrainerSettings, "IncreaseCourierCapacity");
		private bool ReduceBoxPriceEnabled => Helpers.GetProperty(TrainerSettings, "ReduceBoxPrice");
		private bool ReduceISPCostEnabled => Helpers.GetProperty(TrainerSettings, "ReduceISPCost");
		private bool ReduceExpansionCostEnabled => Helpers.GetProperty(TrainerSettings, "ReduceExpansionCost");


		private void Start()
		{
			Helpers.Random = new Random();

			if (!isActiveAndEnabled)
			{
				return;
			}

			SceneManager.sceneLoaded += OnLevelFinishedLoading;
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
							Destroy(Main.SkillChangeButton.gameObject);
						}
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

		private void SubscribeToEvents()
		{
			TimeOfDay.OnHourPassed += (obj, args) => OnHourPassed(obj, args);
			TimeOfDay.OnDayPassed += (obj, args) => OnDayPassed(obj, args);
			TimeOfDay.OnMonthPassed += (obj, args) => OnMonthPassed(obj, args);
		}

		private void UnsubscribeFromEvents()
		{
			TimeOfDay.OnHourPassed -= (obj, args) => OnHourPassed(obj, args);
			TimeOfDay.OnDayPassed -= (obj, args) => OnDayPassed(obj, args);
			TimeOfDay.OnMonthPassed -= (obj, args) => OnMonthPassed(obj, args);
		}

		private void OnHourPassed(object obj, EventArgs args)
		{

		}

		private void OnDayPassed(object obj, EventArgs args)
		{

		}

		private void OnMonthPassed(object obj, EventArgs args)
		{
			if (LockAgeEnabled)
			{
				Settings.sActorManager.Actors.ForEach(x => x.employee.BirthDate += 1);
			}
		}

		private void Update()
		{
			if (!isActiveAndEnabled || !Helpers.IsGameLoaded)
			{
				return;
			}

			HandleInput();
			InitializeTrainerState();

			ApplyFurnitureUpdates();
			ApplyRoomUpdates();
			ApplyActorUpdates();
			ApplyWorkItemUpdates();
			ApplyCompanyUpdates();
			ApplyWorldSettingsUpdates();
			HandleTimedEvents();
		}

		private void HandleInput()
		{
			if (Input.GetKey(KeyCode.F1))
			{
				Main.OpenSettingsWindow();
			}

			if (Input.GetKey(KeyCode.F2))
			{
				Main.CloseSettingsWindow();
			}
		}

		private void InitializeTrainerState()
		{
			if (!_specializationsLoaded && Settings.MyCompany != null)
			{
				LoadSpecializations();
				ShowDiscordInvite(displayAsPopup: true);
			}

			if (_defaultEnvironmentISPCostFactor.IsZero())
			{
				_defaultEnvironmentISPCostFactor = Settings.Environment.ISPCostFactor;
			}
		}

		private void ApplyFurnitureUpdates()
		{
			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
				if (NoiseReductionEnabled)
				{
					furniture.ActorNoise = 0f;
					furniture.EnvironmentNoise = 0f;
					furniture.FinalNoise = 0f;
					furniture.Noisiness = 0;
				}

				if (NoWaterElectricityEnabled)
				{
					furniture.Water = 0;
					furniture.Wattage = 0;
				}

				if (DisableFiresEnabled)
				{
					if (furniture.HasUpg && furniture.upg.FireStarter > 0.0f)
					{
						furniture.upg.FireStarter = 0.0f;
					}
					if (furniture.Parent.IsOnFire)
					{
						if (furniture.Parent.Temperature > 40f)
						{
							furniture.Parent.Temperature = 21f;
						}
						furniture.Parent.StopFire();
					}
				}

				if (IncreaseBookshelfSkillEnabled && furniture.Type == "Bookshelf")
				{
					furniture.AuraValues[1] = 0.75f; // TODO: Consider making this configurable or resetting when disabled
				}

				if (NoMaintenanceEnabled)
				{
					// Simplified maintenance logic - consider specific furniture types if needed
					if (furniture.HasUpg && (furniture.upg.Quality < 0.8f || furniture.upg.Broken))
					{
						furniture.upg.RepairMe();
					}
					// Apply comfort boost specifically to chairs if needed
					if (furniture.Type == "Chair" && furniture.Comfort < 1.2f)
					{
						furniture.Comfort = 1.5f;
					}
				}

				if (DisableFurnitureStealingEnabled)
				{
					furniture.CanSteal = false; // Apply this setting here as it relates to furniture
				}
				else
				{
					// Optionally reset CanSteal if the setting is disabled, depending on desired behavior
					// furniture.CanSteal = true; // Or reset based on original furniture properties
				}
			}
		}

		private void ApplyRoomUpdates()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				Room room = Settings.sRoomManager.Rooms[i];

				if (CleanRoomsEnabled)
				{
					room.ClearDirt();
					room.Smell = 0f;
				}

				if (TemperatureLockEnabled)
				{
					room.Temperature = 21f;
				}

				if (FullEnvironmentEnabled)
				{
					room.FurnEnvironment = 8;
				}

				if (FullRoomBrightnessEnabled)
				{
					room.IndirectLighting = 16;
				}

				if (NoSicknessEnabled) // Germs are room-related
				{
					room.GermCount = 0f;
				}
			}
		}

		private void ApplyActorUpdates()
		{
			bool noSickness = NoSicknessEnabled; // Use property
			if (noSickness)
			{
				TimeOfDay.Instance.Sick.Clear(); // Clear global sick list once if setting is enabled
			}

			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				Employee employee = actor.employee;

				if (noSickness)
				{
					if (actor.SpecialState == Actor.HomeState.Sick)
						actor.SpecialState = Actor.HomeState.Default;

					actor.GermAdd = 0f;
					actor.GermCount = 0f;
					actor.SickDays = 0;
				}

				if (NoStressEnabled)
				{
					employee.Stress = 1f;
				}

				// Apply efficiency based on role and settings
				float? efficiency = null;
				if (employee.RoleString.Contains("Lead") && Helpers.GetProperty(StoresSettings, "LeadEfficiencyStore") != null)
				{
					efficiency = Helpers.GetProperty(StoresSettings, "LeadEfficiencyStore").MakeFloat();
				}
				else if (!employee.RoleString.Contains("Lead") && Helpers.GetProperty(StoresSettings, "EfficiencyStore") != null)
				{
					efficiency = Helpers.GetProperty(StoresSettings, "EfficiencyStore").MakeFloat();
				}
				if (efficiency.HasValue)
				{
					actor.Effectiveness = efficiency.Value;
				}
				// Consider resetting effectiveness if settings are off?

				if (FullSatisfactionEnabled)
				{
					employee.JobSatisfaction = 2f;
					employee.ActiveComplaint = false;

					// Collect keys of negative thoughts to remove
					List<string> keysToRemove = new List<string>();
					// Iterate over keys assuming Thoughts has a Keys property and allows index access
					if (employee.Thoughts != null) // Add null check for safety
					{
						// Create a temporary list of keys to avoid modifying the collection while iterating
						List<string> currentKeys = new List<string>(employee.Thoughts.Keys); 
						foreach (string key in currentKeys)
						{
							Employee.ThoughtEffect thought; // Declare variable outside for C# 6 compatibility
							// Check if the key still exists before accessing (optional, defensive)
							if (employee.Thoughts.TryGetValue(key, out thought))
                            {
								if (thought.Mood.Negative || thought.Mood.Sue || !string.IsNullOrEmpty(thought.Mood.QuitReason))
								{
									keysToRemove.Add(key);
								}
                            }
						}
					}

					// Remove the collected thoughts
					foreach (string keyToRemove in keysToRemove)
					{
						if (employee.Thoughts != null) // Add null check for safety
						{
							employee.Thoughts.Remove(keyToRemove);
						}
					}

					employee.SetMood("LoveWork", actor, 1f);
				}

				if (NoNeedsEnabled)
				{
					actor.NextSmell = 0f;
					employee.Bladder = 1f;
					employee.Hunger = 1f;
					employee.Energy = 1f;
					employee.Social = 1f;
					employee.Posture = 1f;
					employee.ActiveComplaint = false;
					employee.HadProperFood = true;
				}

				if (FreeEmployeesEnabled)
				{
					actor.NegotiateSalary = false;
					if (employee.Salary > 0f) // Only change if salary is not already zero
					{
						employee.ChangeSalary(0f, 0f, actor, false);
					}
					employee.AskedFor = 0f;
					employee.Demanded = 0f;
					employee.UpfrontDemand = 0f;
				}

				if (NoiseReductionEnabled) // Actor noisiness
				{
					actor.Noisiness = 0;
				}

				if (NoVacationEnabled)
				{
					actor.VacationMonth = SDateTime.NextMonth(24);
				}

				if (MoreInspirationEnabled)
				{
					employee.LastInpirationUse = new SDateTime(0);
				}

				if (MoreCreativityEnabled)
				{
					employee.RevealCreativity(1f);
				}

				actor.WalkSpeed = IncreaseWalkSpeedEnabled ? 4f : 2f;
			}
		}

		private void ApplyWorkItemUpdates()
		{
			if (AutoEndDesignEnabled)
			{
				var designDocuments = Settings.MyCompany.WorkItems
									.OfType<DesignDocument>()
									.Where(d => d.HasFinished)
									.ToList(); // ToList to avoid modification issues during iteration

				designDocuments.ForEach(designDocument => designDocument.PromoteAction());
			}

			if (AutoEndResearchEnabled)
			{
				var researchWorks = Settings.MyCompany.WorkItems
									.OfType<ResearchWork>()
									.Where(rw => rw.Finished)
									.ToList();

				researchWorks.ForEach(researchWork =>
				{
					// Simplified logic from original code
					Settings.MyCompany.AddResearch(researchWork.Spec, researchWork.Year);
					TechLevel tech = Settings.simulation.AddTechLevel(researchWork.Spec, researchWork.Year, SDateTime.Now(), true);
					if (tech != null)
					{
						LegalWork legalWork = new LegalWork(tech);
						Settings.MyCompany.WorkItems.Add(legalWork);
						Settings.ApplyDefaultTeams(legalWork, ((int)legalWork.Type).ToString() + "Team");
					}
					researchWork.Kill(false);
				});
			}

			if (AutoEndPatentEnabled)
			{
				var legalWorks = Settings.MyCompany.WorkItems
								   .OfType<LegalWork>()
								   .Where(lw => lw.CurrentStage() == "Finished" && lw.Type == LegalWork.WorkType.Patent)
								   .ToList();

				legalWorks.ForEach(legalWork => legalWork.PatentNow());
			}

			if (AutoResearchStartEnabled)
			{
				StartAutoResearch();
			}
		}

		private void StartAutoResearch()
		{
			var activeTechLevels = MarketSimulation.Active.TechLevels;
			var defaultResearchTeams = Settings.GetDefaultTeams("Research");
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


		private void ApplyCompanyUpdates()
		{
			//TODO: add printspeed and printprice when it's disabled (else)
			if (FreePrintEnabled)
			{
				Settings.ProductPrinters.ForEach(p => p.PrintPrice = 0f);
			}

			if (IncreasePrintSpeedEnabled)
			{
				Settings.ProductPrinters.ForEach(p => p.PrintSpeed = 2f);
			}

			if (NoEducationCostEnabled)
			{
				EducationWindow.EdCost = new[] { 0f, 0f, 0f };
			}
			// Consider resetting EdCost if setting is disabled

			if (FreeStaffEnabled)
			{
				Settings.StaffSalaryDue = 0f;
			}

			if (NoServerCostEnabled)
			{
				Settings.ServerCost = 0f;
			}

			if (NoWaterElectricityEnabled) // Bills are company-wide
			{
				Settings.ElectricityBill = 0f;
				Settings.Waterbill = 0f;
				Settings.Gasbill = 0f;
			}

			if (DigitalDistributionMonopolyEnabled)
			{
				ApplyDigitalDistributionMonopoly();
			}

			if (AutoAcceptHostingDealsEnabled)
			{
				AcceptHostingDealsAutomatically();
			}
		}

		private void ApplyDigitalDistributionMonopoly()
		{
#if DEBUG || SWINCBETA1_7 || SWINCBETA1_8 || SWINCBETA1_9 || SWINCBETA1_10
			foreach (var company in Settings.simulation.Companies.Values.ToList())
			{
				if (company.Bankrupt && company.Distribution != null)
				{
					MarketSimulation.Active.DistributionPlatforms.Remove(company.Distribution);
					HUD.Instance.digitalDistributionWindow.PlatformList.Items.Remove(company.Distribution);
				}

				if (company == Settings.MyCompany || company.Distribution == null || !company.Distribution.Open)
					continue;

				// Apply monopoly effects
				company.Distribution.SetCut(1f);
				company.Distribution.SetAutoAcceptClients(false);
				company.Distribution.AvailableBandwidth = 0f;
				company.Distribution.ItemSales = 0f;
				company.Distribution.ActualItemSales = 0f;
				company.Distribution.LastLoad = 0f;
				company.Distribution.MarketShare = 0f;
				MarketSimulation.Active.ClosePlatform(company.Distribution);
			}
#endif
		}

		private void AcceptHostingDealsAutomatically()
		{
#if DEBUG || SWINCBETA1_7 || SWINCBETA1_8 || SWINCBETA1_9 || SWINCBETA1_10
			var serverGroups = Settings.GetAllServerGroups().ToList();
			if (serverGroups.Count == 0) return;

			// Find the most powerful server group (consider caching this if it's expensive)
			ServerGroup mostPowerfulServerGroup = serverGroups.OrderByDescending(sg => sg.PowerSum).FirstOrDefault();
			if (mostPowerfulServerGroup == null) return; // Should not happen if serverGroups.Count > 0

			var availableServerDeals = HUD.Instance.dealWindow.AllDeals.Values.OfType<ServerDeal>().ToList();
			if (availableServerDeals.Count == 0) return;

			var activeServerDealProducts = HUD.Instance.dealWindow.GetActiveDeals()
											 .OfType<ServerDeal>()
											 .Select(d => d.Product)
											 .ToHashSet(); // Use HashSet for efficient lookup

			foreach (var serverDeal in availableServerDeals)
			{
				if (!activeServerDealProducts.Contains(serverDeal.Product))
				{
					HUD.Instance.dealWindow.ActuallyAcceptDeal(serverDeal, true);
					Settings.RegisterWithServer(mostPowerfulServerGroup.Name, serverDeal);
				}
			}
#endif
		}


		private void ApplyWorldSettingsUpdates()
		{
			if (DisableBurglarsEnabled)
			{
				foreach (var burglar in Settings.sActorManager.Others["Burglars"].ToList()) // ToList for safe removal
				{
					burglar.Despawned = true;
					Settings.sActorManager.RemoveFromAwaiting(burglar);
				}
			}

			if (DisableFireInspectionEnabled)
			{
				foreach (var fireInspector in Settings.sActorManager.Others["FireInspector"].ToList())
				{
					fireInspector.Despawned = true;
					Settings.sActorManager.RemoveFromAwaiting(fireInspector);
				}
				Settings.ActiveFireReport.Reset();
				Settings.PassedFireInspection = true;
			}

			if (DisableForcePauseEnabled)
			{
				GameSettings.ForcePause = false;
			}

			if (DisableForceFreezeEnabled)
			{
				GameSettings.FreezeGame = false;
			}

			// Apply settings that modify global game parameters
			GameSettings.MaxFloor = 100; // Consider if this should only be set once or configurable
			AI.MaxBoxes = IncreaseCourierCapacityEnabled ? 108 : 54;
			AI.MaxBoxCarry = IncreaseCourierCapacityEnabled ? 18 : 9;
			AI.BoxPrice = ReduceBoxPriceEnabled ? 62.5f : 125;
			Settings.Environment.ISPCostFactor = ReduceISPCostEnabled ? _defaultEnvironmentISPCostFactor / 2f : _defaultEnvironmentISPCostFactor;
			Settings.ExpansionCost = ReduceExpansionCostEnabled ? 175f : 350f;
		}

		private void HandleTimedEvents()
		{
			if (MoreHostingDealsEnabled)
			{
				int inGameHour = TimeOfDay.Instance.Hour;

				// Push Deal logic
				if ((inGameHour == 9 || inGameHour == 15) && !Helpers.DealIsPushed)
				{
					MiscActions.PushDeal(); // Call method from MiscActions class
				}
				else if (inGameHour != 9 && inGameHour != 15 && Helpers.DealIsPushed)
				{
					Helpers.DealIsPushed = false;
				}

				// Push Reward logic
				if (!Helpers.RewardIsGained && inGameHour == 12)
				{
					MiscActions.PushReward(); // Call method from MiscActions class
				}
				else if (inGameHour != 12 && Helpers.RewardIsGained)
				{
					Helpers.RewardIsGained = false;
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
			string message = "Join us on our discord server\nhttps://discord.gg/NQpm5kn";
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

		// ClearLoans moved to Actions/MiscActions.cs
		// PushReward moved to Actions/MiscActions.cs
		// PushDeal moved to Actions/MiscActions.cs
		// Test moved to Actions/MiscActions.cs
		// AIBankrupt moved to Actions/CompanyActions.cs

		public static void HREmployees()
		{
			if (!Helpers.IsGameLoaded || SelectorController.Instance == null)
			{
				return;
			}

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

		// SellProductStock moved to Actions/ProductActions.cs
		// RemoveSoft moved to Actions/ProductActions.cs
		// ResetAgeOfEmployees moved to Actions/EmployeeActions.cs
		// EmployeesToMax moved to Actions/EmployeeActions.cs
		// UnlockAllSpace moved to Actions/MiscActions.cs
		// UnlockFurniture moved to Actions/MiscActions.cs
		// HREmployees moved to Actions/EmployeeActions.cs
		// SetSkillPerEmployeeAction moved to Actions/EmployeeActions.cs
		// SetSkillPerEmployee moved to Actions/EmployeeActions.cs
		// MonthDaysAction moved to Actions/MiscActions.cs
		// MonthDays moved to Actions/MiscActions.cs
		// ExtendDeadline moved to Actions/MiscActions.cs
		// FixBugsAction moved to Actions/ProductActions.cs
		// FixBugs moved to Actions/ProductActions.cs
		// MaxFollowersAction moved to Actions/ProductActions.cs
		// MaxFollowers moved to Actions/ProductActions.cs
		// SetProductPriceAction moved to Actions/ProductActions.cs
		// SetProductPrice moved to Actions/ProductActions.cs
		// SetProductStockAction moved to Actions/ProductActions.cs
		// SetProductStock moved to Actions/ProductActions.cs
		// AddActiveUsersAction moved to Actions/ProductActions.cs
		// AddActiveUsers moved to Actions/ProductActions.cs
		// TakeoverCompanyAction moved to Actions/CompanyActions.cs
		// TakeoverCompany moved to Actions/CompanyActions.cs
		// SubDCompanyAction moved to Actions/CompanyActions.cs
		// SubDCompany moved to Actions/CompanyActions.cs
		// ForceBankruptAction moved to Actions/CompanyActions.cs
		// ForceBankrupt moved to Actions/CompanyActions.cs
		// IncreaseMoneyAction moved to Actions/MiscActions.cs
		// IncreaseMoney moved to Actions/MiscActions.cs
		// MaxReputation moved to Actions/MiscActions.cs
		// MaxMarketRecognition moved to Actions/MiscActions.cs
		// UnlockAndClaimAllRewards moved to Actions/MiscActions.cs
		// AreYouSureAction moved to Actions/MiscActions.cs (as private helper)
		// TestAction moved to Actions/MiscActions.cs
		// TestButton moved to Actions/MiscActions.cs

		#region Overrides

		public override void OnActivate() { /* Mandatory but not needed */ }

		public override void OnDeactivate() { /* Mandatory but not needed */ }

		#endregion
	}
}
