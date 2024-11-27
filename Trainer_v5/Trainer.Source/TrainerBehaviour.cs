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
		private float _defaultEnvironmentISPCostFactor;

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
			if (Helpers.GetProperty(TrainerSettings, "LockAge"))
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
				LoadSpecializations();
				ShowDiscordInvite(displayAsPopup: true);
			}

			if (_defaultEnvironmentISPCostFactor.IsZero())
			{
				_defaultEnvironmentISPCostFactor = Settings.Environment.ISPCostFactor;
			}

			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
				if (Helpers.GetProperty(TrainerSettings, "NoiseReduction"))
				{
					furniture.ActorNoise = 0f;
					furniture.EnvironmentNoise = 0f;
					furniture.FinalNoise = 0f;
					furniture.Noisiness = 0;
				}

				if (Helpers.GetProperty(TrainerSettings, "NoWaterElectricity"))
				{
					furniture.Water = 0;
					furniture.Wattage = 0;
				}

				if (Helpers.GetProperty(TrainerSettings, "DisableFires"))
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

				if (Helpers.GetProperty(TrainerSettings, "IncreaseBookshelfSkill") && furniture.Type == "Bookshelf")
				{
					furniture.AuraValues[1] = 0.75f;
				}

				//TODO: else 0.25
				if (Helpers.GetProperty(TrainerSettings, "NoMaintenance"))
				{
					switch (furniture.Type)
					{
						case "Chair":
							if (furniture.Comfort < 1.2f)
							{
								furniture.Comfort = 1.5f;
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

			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				Room room = Settings.sRoomManager.Rooms[i];

				if (Helpers.GetProperty(TrainerSettings, "CleanRooms"))
				{
					room.ClearDirt();
					room.Smell = 0f;
				}

				if (Helpers.GetProperty(TrainerSettings, "TemperatureLock"))
				{
					room.Temperature = 21f;
				}

				if (Helpers.GetProperty(TrainerSettings, "FullEnvironment"))
				{
					room.FurnEnvironment = 8;
				}

				if (Helpers.GetProperty(TrainerSettings, "FullRoomBrightness"))
				{
					room.IndirectLighting = 16;
				}

				if (Helpers.GetProperty(TrainerSettings, "NoSickness"))
				{
					room.GermCount = 0f;
				}
			}

			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				Employee employee = actor.employee;

				if (Helpers.GetProperty(TrainerSettings, "NoSickness"))
				{
					TimeOfDay.Instance.Sick.Clear();

					if (actor.SpecialState == Actor.HomeState.Sick)
						actor.SpecialState = Actor.HomeState.Default;

					actor.GermAdd = 0f;
					actor.GermCount = 0f;
					actor.SickDays = 0;
					//actor.WasSick = true;
				}

				if (Helpers.GetProperty(TrainerSettings, "NoStress"))
				{
					employee.Stress = 1f;
				}

				if (employee.RoleString.Contains("Lead") && Helpers.GetProperty(StoresSettings, "LeadEfficiencyStore") != null)
				{
					actor.Effectiveness = Helpers.GetProperty(StoresSettings, "LeadEfficiencyStore").MakeFloat();
				}

				if (!employee.RoleString.Contains("Lead") && Helpers.GetProperty(StoresSettings, "EfficiencyStore") != null)
				{
					actor.Effectiveness = Helpers.GetProperty(StoresSettings, "EfficiencyStore").MakeFloat();
				}

				if (Helpers.GetProperty(TrainerSettings, "FullSatisfaction"))
				{
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

				if (Helpers.GetProperty(TrainerSettings, "NoNeeds"))
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

				if (Helpers.GetProperty(TrainerSettings, "FreeEmployees"))
				{
					actor.NegotiateSalary = false;
					employee.Salary = 0f;
					employee.AskedFor = 0f;
					employee.Demanded = 0f;
					employee.UpfrontDemand = 0f;
					employee.ChangeSalary(0f, 0f, actor, false);
				}

				if (Helpers.GetProperty(TrainerSettings, "NoiseReduction"))
				{
					actor.Noisiness = 0;
				}

				if (Helpers.GetProperty(TrainerSettings, "NoVacation"))
				{
					actor.VacationMonth = SDateTime.NextMonth(24);
				}

				if (Helpers.GetProperty(TrainerSettings, "MoreInspiration"))
				{
					employee.LastInpirationUse = new SDateTime(0);
				}

				if (Helpers.GetProperty(TrainerSettings, "MoreCreativity"))
				{
					employee.RevealCreativity(1f);
				}

				actor.WalkSpeed = Helpers.GetProperty(TrainerSettings, "IncreaseWalkSpeed") ? 4f : 2f;
			}

			if (Helpers.GetProperty(TrainerSettings, "MoreHostingDeals"))
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

			if (Helpers.GetProperty(TrainerSettings, "DisableBurglars"))
			{
				foreach (var burglar in Settings.sActorManager.Others["Burglars"])
				{
					burglar.Despawned = true;
					Settings.sActorManager.RemoveFromAwaiting(burglar);
				}
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoEndDesign"))
			{
				var designDocuments = Settings.MyCompany.WorkItems
									.OfType<DesignDocument>()
									.Where(d => d.HasFinished)
									.ToList();

				designDocuments.ForEach(designDocument =>
				{
					designDocument.PromoteAction();
				});
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoEndResearch"))
			{
				var researchWorks = Settings.MyCompany.WorkItems
									.OfType<ResearchWork>()
									.Where(rw => rw.Finished)
									.ToList();

				researchWorks.ForEach(researchWork =>
				{
					GameSettings.Instance.MyCompany.AddResearch(researchWork.Spec, researchWork.Year);
					TechLevel tech = GameSettings.Instance.simulation.AddTechLevel(researchWork.Spec, researchWork.Year, SDateTime.Now(), true);
					if (tech != null)
					{
						LegalWork legalWork = new LegalWork(tech);
						GameSettings.Instance.MyCompany.WorkItems.Add(legalWork);
						GameSettings.Instance.ApplyDefaultTeams(legalWork, ((int)legalWork.Type).ToString() + "Team");
					}
					researchWork.Kill(false);
				});
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoEndPatent"))
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

			//TODO: add printspeed and printprice when it's disabled (else)
			if (Helpers.GetProperty(TrainerSettings, "FreePrint"))
			{
				Settings.ProductPrinters.ForEach(p => p.PrintPrice = 0f);
			}

			if (Helpers.GetProperty(TrainerSettings, "IncreasePrintSpeed"))
			{
				Settings.ProductPrinters.ForEach(p => p.PrintSpeed = 2f);
			}

			if (Helpers.GetProperty(TrainerSettings, "NoEducationCost"))
			{
				EducationWindow.EdCost = new[] { 0f, 0f, 0f };
			}

			if (Helpers.GetProperty(TrainerSettings, "FreeStaff"))
			{
				Settings.StaffSalaryDue = 0f;
			}

			if (Helpers.GetProperty(TrainerSettings, "NoServerCost"))
			{
				Settings.ServerCost = 0f;
			}

			if (Helpers.GetProperty(TrainerSettings, "NoWaterElectricity"))
			{
				Settings.ElectricityBill = 0f;
				Settings.Waterbill = 0f;
				Settings.Gasbill = 0f;
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableFurnitureStealing"))
			{
				Settings.sRoomManager.AllFurniture.ForEach(x => x.CanSteal = false);
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableFireInspection"))
			{
				foreach (var fireInspector in Settings.sActorManager.Others["FireInspector"])
				{
					fireInspector.Despawned = true;
					Settings.sActorManager.RemoveFromAwaiting(fireInspector);
				}

				Settings.ActiveFireReport.Reset();
				Settings.PassedFireInspection = true;
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableForcePause"))
			{
				GameSettings.ForcePause = false;
			}

			if (Helpers.GetProperty(TrainerSettings, "DisableForceFreeze"))
			{
				GameSettings.FreezeGame = false;
			}

			if (Helpers.GetProperty(TrainerSettings, "AutoResearchStart"))
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

			if (Helpers.GetProperty(TrainerSettings, "DigitalDistributionMonopol"))
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

			if (Helpers.GetProperty(TrainerSettings, "AutoAcceptHostingDeals"))
			{
#if DEBUG || SWINCBETA1_7 || SWINCBETA1_8 || SWINCBETA1_9 || SWINCBETA1_10
				var serverGroups = Settings.GetAllServerGroups().ToList();
				if (serverGroups.Count == 0)
					return;

				ServerGroup mostPowerfulServerGroup = new ServerGroup("TRAINER") { PowerSum = 0f };
				foreach (var serverGroup in serverGroups)
				{
					if (serverGroup.PowerSum > mostPowerfulServerGroup.PowerSum)
						mostPowerfulServerGroup = serverGroup;
				}

				var serverDeals = HUD.Instance.dealWindow.AllDeals.Values.OfType<ServerDeal>().ToList();
				if (serverDeals.Count == 0)
					return;

				var activeServerDeals = HUD.Instance.dealWindow.GetActiveDeals().OfType<ServerDeal>().ToList();
				foreach (var serverDeal in serverDeals)
				{
					if (!activeServerDeals.Contains(serverDeal))
					{
						HUD.Instance.dealWindow.ActuallyAcceptDeal(serverDeal, true);
						Settings.RegisterWithServer(mostPowerfulServerGroup.Name, serverDeal);
					}
				}
#endif
			}

			GameSettings.MaxFloor = 100; //10 default
			AI.MaxBoxes = Helpers.GetProperty(TrainerSettings, "IncreaseCourierCapacity") ? 108 : 54;
			AI.MaxBoxCarry = Helpers.GetProperty(TrainerSettings, "IncreaseCourierCapacity") ? 18 : 9;
			AI.BoxPrice = Helpers.GetProperty(TrainerSettings, "ReduceBoxPrice") ? 62.5f : 125;
			Settings.Environment.ISPCostFactor = Helpers.GetProperty(TrainerSettings, "ReduceISPCost") ? _defaultEnvironmentISPCostFactor / 2f : _defaultEnvironmentISPCostFactor;
			Settings.ExpansionCost = Helpers.GetProperty(TrainerSettings, "ReduceExpansionCost") ? 175f : 350f;
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

			if (!Helpers.IsGameLoaded || SelectorController.Instance == null)
			{
				return;
			}

			foreach (Actor actor in Settings.sActorManager.Actors.ToArray())
			{
				actor.employee.CreativityKnown = 1f;

				foreach (SoftwareType t in softwareTypes)
				{
#if DEBUG || SWINCBETA1_7 || SWINCBETA1_8 || SWINCBETA1_9 || SWINCBETA1_10
					actor.employee.LeadSpecializationFix[t.ToString()] = 1f;
#else
					actor.employee.LeadSpecialization[t] = 1f;
#endif
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
			if (!Helpers.IsGameLoaded)
			{
				return;
			}

			Example.TakeAllLand();
			HUD.Instance.AddPopupMessage("Trainer: All plots has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void UnlockFurniture()
		{
			if (!Helpers.IsGameLoaded)
			{
				return;
			}

			Example.UnlockFurniture();
			Cheats.UnlockFurn = true;
			HUD.Instance.UpdateFurnitureButtons();
			HUD.Instance.AddPopupMessage("Trainer: All furniture has been unlocked!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		#region MonthDays

		public static void MonthDaysAction(string input)
		{
			int i;
			if (!int.TryParse(input, out i))
			{
				return;
			}

			GameSettings.DaysPerMonth = i;
			WindowManager.SpawnDialog("You have changed days per month. Please restart the game.", false, DialogWindow.DialogType.Warning);
		}

		public static void MonthDays()
		{
			WindowManager.SpawnInputDialog("How many days per month do you want?", "Days per month", "2", MonthDaysAction);
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
			WorkItem WorkItem = Settings.MyCompany.WorkItems
				.Where(item => item.GetType() == typeof(SoftwareAlpha)).FirstOrDefault(item =>
					(item as SoftwareAlpha).Name == input && (item as SoftwareAlpha).InBeta);

			if (WorkItem == null)
			{
				return;
			}

		  ((SoftwareAlpha)WorkItem).FixedBugs = ((SoftwareAlpha)WorkItem).MaxBugs;
		}

		public static void FixBugs()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Fix Bugs", "", FixBugsAction);
		}

		#endregion

		#region Max Followers

		public static void MaxFollowersAction(string input)
		{
			WorkItem WorkItem = Settings.MyCompany.WorkItems
				.Where(item => item.GetType() == typeof(SoftwareAlpha)).FirstOrDefault(item =>
					(item as SoftwareAlpha).Name == input && !(item as SoftwareAlpha).Paused);

			if (WorkItem == null)
			{
				return;
			}

			SoftwareAlpha alpha = (SoftwareAlpha)WorkItem;

			alpha.MaxFollowers += 1000000000;
			alpha.ReEvaluateMaxFollowers();

			alpha.FollowerChange += 1000000000f;
			alpha.Followers += 1000000000f;
		}

		public static void MaxFollowers()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Max Followers", "", MaxFollowersAction);
		}

		#endregion

		#region Set Product Price

		public static void SetProductPriceAction(string input)
		{
			SoftwareProduct Product =
				Settings.MyCompany.Products.FirstOrDefault(product => product.Name == Helpers.ProductPriceName);

			if (Product == null)
			{
				return;
			}

			Product.Price = input.ConvertToFloatDef(50f);
			HUD.Instance.AddPopupMessage("Trainer: Price for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void SetProductPrice()
		{
			WindowManager.SpawnInputDialog("Type product price:", "Product price", "50", SetProductPriceAction);
		}

		#endregion

		#region Set Product Stock

		public static void SetProductStockAction(string input)
		{
			SoftwareProduct Product =
				Settings.MyCompany.Products.FirstOrDefault(product => product.Name == Helpers.ProductPriceName);

			if (Product == null)
			{
				return;
			}

			Product.PhysicalCopies = (uint)input.ConvertToIntDef(100000);
			HUD.Instance.AddPopupMessage("Trainer: Stock for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void SetProductStock()
		{
			WindowManager.SpawnInputDialog("Type product stock:", "Product stock", "100000", SetProductStockAction);
		}

		#endregion

		#region Add Active Users

		public static void AddActiveUsersAction(string input)
		{
			SoftwareProduct Product =
				Settings.MyCompany.Products.FirstOrDefault(product => product.Name == Helpers.ProductPriceName);

			if (Product == null)
			{
				return;
			}

			Product.Userbase = input.ConvertToIntDef(100000);
			HUD.Instance.AddPopupMessage("Trainer: Active users for " + Product.Name + " has been setted up!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void AddActiveUsers()
		{
			WindowManager.SpawnInputDialog("Type product active users:", "Product active users", "100000", AddActiveUsersAction);
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

		public static void IncreaseMoneyAction(string input)
		{
			Settings.MyCompany.MakeTransaction(input.ConvertToIntDef(100000), Company.TransactionCategory.Deals);
			HUD.Instance.AddPopupMessage("Trainer: Money has been added in category Deals!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}
		public static void IncreaseMoney()
		{
			WindowManager.SpawnInputDialog("How much money do you want to add?", "Add Money", "100000", IncreaseMoneyAction);
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

		public override void OnActivate() { /* Mandatory but not needed */ }

		public override void OnDeactivate() { /* Mandatory but not needed */ }

		#endregion
	}
}
