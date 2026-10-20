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

		internal static void ResetAgeOfEmployees()
		{
			if (!Helpers.RequireGameReady("Reset age of employees"))
				return;

			var currentTime = SDateTime.Now();
			foreach (var actor in Settings.sActorManager.Actors)
			{
				actor.employee.BirthDate = currentTime - Employee.Youngest * 12;
				actor.UpdateAgeLook();
			}

			Notification.Popup("EmployeesAgeReset".LocDef("Trainer: Employees age has been reset!"));
		}

		internal static void EmployeesToMax()
		{
			if (!Helpers.RequireGameReady("Max skill of employees", requireSelector: true))
				return;

			SoftwareType[] softwareTypes = MarketSimulation.Active.SoftwareTypes.Values.ToArray();
			Employee.EmployeeRole[] employeeRoles = (Employee.EmployeeRole[])Enum.GetValues(typeof(Employee.EmployeeRole));

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

			Notification.Popup("AllEmployeesMaxSkilled".LocDef("Trainer: All employees are now max skilled!"));
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

		// The game loses a Forgetful employee's specialization only when the actor leaves the office after ForgetfulETA has
		// passed (AI.Despawn); keeping the timer ahead of the clock skips the loss, the popup and the re-arm. Unarmed (0) stays.
		internal static void ApplyNoForgetfulSkillLoss()
		{
			if (TimeOfDay.Instance == null)
				return;

			SDateTime now = SDateTime.Now();
			SDateTime unarmed = new SDateTime(0);
			SDateTime threshold = now + 3;

			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = Settings.sActorManager.Actors[i];

				if (actor.employee == null || !actor.employee.HasTrait(Employee.Trait.Forgetful))
					continue;

				if (actor.ForgetfulETA != unarmed && actor.ForgetfulETA <= threshold)
					actor.ForgetfulETA = now + 12;
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

		// The list belongs to one game (mods change the specializations), so a new game recomputes it. The loaded flag
		// stays set: it also gates the one-time Discord popup in TrainerBehaviour.Update.
		internal static void ResetSpecializations()
		{
			Helpers.SpecializationsList = null;
		}

		// Recomputes the list for a later game; the first load of a session still belongs to TrainerBehaviour.Update.
		internal static void RestoreSpecializationsAfterReset()
		{
			if (_specializationsLoaded && Helpers.SpecializationsList == null)
			{
				LoadSpecializations();
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
