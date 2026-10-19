using System;
using System.Collections.Generic;
using System.Linq;
using OrbCreationExtensions;

namespace Trainer_v5
{
	internal static class EmployeeFeatures
	{
		private static GameSettings Settings => GameSettings.Instance;
		private static bool _specializationsLoaded;

		internal static bool SpecializationsLoaded => _specializationsLoaded;

		internal static void SetSkillPerEmployeeAction(string input)
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
						foreach (var specialization in selectedSpecializations)
						{
							actor.employee.AddSpecialization(role.Key.ToEmployeeRole(), specialization.Key, false, true, amount);
						}
					}
				});

				HUD.Instance.AddPopupMessage("Trainer: Employee skills/specializations are set!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			}
		}

		internal static void SetSkillPerEmployee()
		{
			WindowManager.SpawnInputDialog("How many specialization stars do you want?\nMin = -3, Max = 3", "Stars amount", "3", SetSkillPerEmployeeAction);
		}

		internal static void HREmployees()
		{
			if (!Helpers.IsGameReady(requireSelector: true))
				return;

			Actor[] Actors = Settings.sActorManager.Actors
									 .Where(actor => actor.employee.IsRole(Employee.RoleBit.Lead))
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

		internal static void ResetAgeOfEmployees()
		{
			var currentTime = SDateTime.Now();
			foreach (var actor in Settings.sActorManager.Actors)
			{
				actor.employee.BirthDate = currentTime - Employee.Youngest * 12;
				actor.UpdateAgeLook();
			}

			HUD.Instance.AddPopupMessage("Trainer: Employees age has been reset!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void EmployeesToMax()
		{
			SoftwareType[] softwareTypes = MarketSimulation.Active.SoftwareTypes.Values.ToArray();
			Employee.EmployeeRole[] employeeRoles = (Employee.EmployeeRole[])Enum.GetValues(typeof(Employee.EmployeeRole));

			if (!Helpers.IsGameReady(requireSelector: true))
				return;

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

		internal static void RestoreIncreaseWalkSpeed()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Settings.sActorManager.Actors[i].WalkSpeed = Constants.WALK_SPEED_DEFAULT;
			}
		}

		internal static void ApplyFullSatisfaction()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				ApplyFullSatisfactionToActor(Settings.sActorManager.Actors[i]);
			}
		}

		internal static void ApplyNoStress()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Settings.sActorManager.Actors[i].employee.Stress = 1f;
			}
		}

		internal static void ApplyLeadEfficiencyStore()
		{
			float value = Helpers.GetProperty(Helpers.StoresSettings, SettingKeys.LeadEfficiencyStore).MakeFloat();
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				if (actor.employee.IsRole(Employee.RoleBit.Lead))
				{
					actor.Effectiveness = value;
				}
			}
		}

		internal static void ApplyEfficiencyStore()
		{
			float value = Helpers.GetProperty(Helpers.StoresSettings, SettingKeys.EfficiencyStore).MakeFloat();
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				if (!actor.employee.IsRole(Employee.RoleBit.Lead))
				{
					actor.Effectiveness = value;
				}
			}
		}

		internal static void ApplyNoNeeds()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];
				Employee employee = actor.employee;
				actor.NextSmell = 0f;
				employee.Bladder = 1f;
				employee.Hunger = 1f;
				employee.Energy = 1f;
				employee.Social = 1f;
				employee.Posture = 1f;
				employee.ActiveComplaint = false;
				employee.HadProperFood = true;
			}
		}

		internal static void ApplyMoreInspiration()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Settings.sActorManager.Actors[i].employee.LastInpirationUse = new SDateTime(0);
			}
		}

		internal static void ApplyNoVacation()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];

				actor.VacationMonth = SDateTime.NextMonth(24);
				if (actor.SpecialState == Actor.HomeState.Vacation)
					actor.SpecialState = Actor.HomeState.Default;
			}
		}

		internal static void ApplyFreeEmployees()
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

		internal static void LoadSpecializations()
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

		internal static void ApplyIncreaseWalkSpeed()
		{
			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Settings.sActorManager.Actors[i].WalkSpeed = Constants.WALK_SPEED_BOOSTED;
			}
		}

		internal static void ApplyLockAge()
		{
			Settings.sActorManager.Actors.ForEach(x => x.employee.BirthDate += 1);
		}

		// Walks backwards so removing an entry never shifts one still to be visited, and copies nothing (Thoughts.Values is the live list).
		internal static void RemoveUnwantedThoughts(DictionaryList<string, Employee.ThoughtEffect> thoughts)
		{
			for (int i = thoughts.Count - 1; i >= 0; i--)
			{
				Employee.ThoughtEffect thought = thoughts[i];
				if (IsUnwantedThought(thought.Mood))
				{
					thoughts.Remove(thought.Thought);
				}
			}
		}

		internal static bool IsUnwantedThought(MoodEffect mood)
		{
			return mood.Negative || mood.Sue || !string.IsNullOrEmpty(mood.QuitReason);
		}

		private static void ApplyFullSatisfactionToActor(Actor actor)
		{
			Employee employee = actor.employee;

			employee.JobSatisfaction = 2f;
			employee.ActiveComplaint = false;
			RemoveUnwantedThoughts(employee.Thoughts);

			employee.SetMood("LoveWork", actor, 1f);
		}
	}
}
