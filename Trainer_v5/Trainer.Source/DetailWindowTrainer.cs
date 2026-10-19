using System;
using System.Collections.Generic;
using Trainer_v5.Trainer.Source.Window;
using Trainer_v5.Window;
using UnityEngine;

namespace Trainer_v5
{
	internal static class DetailWindowTrainer
	{
		// The DetailWindow the controls were injected into; the game recreates it on save reloads.
		private static DetailWindow _installedOn;

		public static void Reset()
		{
			_installedOn = null;
		}

		public static void Install()
		{
			var target = HUD.Instance?.DetailWindow;
			if (target == null || target == _installedOn)
				return;

			var components = new List<Component>
			{
				UIFactory.Button("Trait",  () => EmployeeTraitChangeWindow.Instance.Show()),
				UIFactory.Button("Demand", () => EmployeeDemandChangeWindow.Instance.Show()),
				UIFactory.Button("Creativity",  SetCreativity),
				UIFactory.Button("Inspiration", SetInspiration),
				UIFactory.Button("LeadSpec", () => EmployeeLeadSpecChangeWindow.Instance.Show()),
				UIFactory.Button("Founder", FounderToggle.Start),
			};

			// add components to DetailWindow, above it and within its width
			const int width = 80, height = 32, spacing = 4;
			var windowRect = target.Window != null ? ((RectTransform)target.Window.transform).rect : new Rect();
			var slots = DetailButtonLayout.Place(components.Count, windowRect.width, width, height, spacing);
			for (var i = 0; i < components.Count; i++)
			{
				components[i].gameObject.AddToElement("DetailWindow", slots[i]);
			}

			// Marked installed only after the loop succeeds, so an exception leaves a retry possible.
			_installedOn = target;
		}

		private static void SetCreativity()
		{
			TargetSource source;
			var target = EmployeeTarget.Resolve(out source);
			if (source != TargetSource.DetailWindow)
				return;
			var employee = target.Employee;

			if (!CreativityEditGuard.IsEnabled())
			{
				Notification.ShowError("Editing creativity replaces the employee object and is experimental. Enable Experimental in the Trainer Settings first.");
				return;
			}

			var blocked = CreativityEditGuard.FindBlockingReference(employee);
			if (blocked != null)
			{
				Notification.ShowError($"{employee.Name} {blocked}. Nothing was edited.");
				return;
			}

			InputHelper.RequestFloat(
				$"Current is {employee.Creativity}\nMin = 0, Max = 1.0",
				$"Set creativity for {employee.Name}",
				val => ApplyCreativity(target, employee, val),
				min: 0,
				max: 1);
		}

		// Re-checks the guards at confirmation, then replaces the employee and migrates the game's references to it.
		private static void ApplyCreativity(EmployeeTarget target, Employee employee, float val)
		{
			if (!CreativityEditGuard.IsEnabled())
			{
				Notification.ShowError("Experimental was switched off. Nothing was edited.");
				return;
			}

			if (!target.IsStillValid())
			{
				Notification.ShowError("The employee changed or is not available. Nothing was edited.");
				return;
			}

			if (Math.Abs(val - employee.Creativity) < 0.0001f)
			{
				return;
			}

			var stillBlocked = CreativityEditGuard.FindBlockingReference(employee);
			if (stillBlocked != null)
			{
				Notification.ShowError($"{employee.Name} {stillBlocked}. Nothing was edited.");
				return;
			}

			var newEmployee = BuildReplacement(employee, val);

			var actor = employee.MyActor;
			if (actor != null)
			{
				employee.MyActor = null;
				actor.employee = newEmployee;
				newEmployee.MyActor = actor;

				if (HUD.Instance?.DetailWindow?.CurrentEmployee?.employee == employee)
				{
					HUD.Instance.DetailWindow.CurrentEmployee.employee = newEmployee;
				}

				// everything in the game that held the old object now holds the new one
				try
				{
					EmployeeReferenceMigration.Migrate(employee, newEmployee);
				}
				catch (Exception failure)
				{
					RevertCreativityEdit(actor, employee, newEmployee);
					failure.LogException("CreativityEdit");
					Notification.ShowError("The creativity edit failed and was reverted.");
				}
			}
		}

		private static Employee BuildReplacement(Employee employee, float val)
		{
			var skills = new float[5];
			for (int i = 0; i < 5; i++)
			{
				skills[i] = employee.GetSkillI(i);
			}

			var newEmployee = new Employee(
				currentTime: SDateTime.Now(),
				female: employee.Female,
				name: employee.Name,
				skills: skills,
				creativity: val,
				person: employee.PersonalityTraits,
				traits: employee.Traits,
				specs: employee.GetAllSpecializations(),
				graph: GameSettings.Instance.Personalities,
				style: employee.StyleGen,
				forceBrain: employee.HiredFor
			);

			// This overload always sets Founder = true, so the previous value is restored.
			newEmployee.Founder = employee.Founder;
			newEmployee.Dismissed = employee.Dismissed;
			newEmployee.Retired = employee.Retired;
			newEmployee.MadeCEO = employee.MadeCEO;

			newEmployee.NetworkID = employee.NetworkID;
			newEmployee.SkillFact = employee.SkillFact;
			newEmployee.Salary = employee.Salary;
			newEmployee.CreativityKnown = 1f;
			newEmployee.MyEmployer = employee.MyEmployer;
			newEmployee.BirthDate = employee.BirthDate;
			newEmployee.Hired = employee.Hired;
			newEmployee.LastWage = employee.LastWage;
			newEmployee.LastBid = employee.LastBid;
			newEmployee.AskedFor = employee.AskedFor;
			newEmployee.Demanded = employee.Demanded;
			newEmployee.UpfrontDemand = employee.UpfrontDemand;
			newEmployee.AgeMonth = employee.AgeMonth;
			newEmployee.NickName = employee.NickName;
			newEmployee.PlayerQuarantine = employee.PlayerQuarantine;
			newEmployee.ActiveComplaint = employee.ActiveComplaint;
			newEmployee.Filter = employee.Filter;
			newEmployee.PreviousEmployment = employee.PreviousEmployment;
			newEmployee.CustomBenefits = employee.CustomBenefits;
			newEmployee.SkillCeiling = employee.SkillCeiling;
			newEmployee.Inspiration = employee.Inspiration;
			newEmployee.LastInpirationUse = employee.LastInpirationUse;

			CopyRoles(employee, newEmployee);

			newEmployee.Thoughts = employee.Thoughts;
			newEmployee.JobSatisfaction = employee.JobSatisfaction;
			newEmployee.Hunger = employee.Hunger;
			newEmployee.Energy = employee.Energy;
			newEmployee.Bladder = employee.Bladder;
			newEmployee.Social = employee.Social;
			newEmployee.Stress = employee.Stress;
			newEmployee.Posture = employee.Posture;
			newEmployee.CoffeeQual = employee.CoffeeQual;
			newEmployee.LowestSatisfaction = employee.LowestSatisfaction;
			newEmployee.SatisfactionHitZero = employee.SatisfactionHitZero;
			newEmployee.InteractedWithBestFriend = employee.InteractedWithBestFriend;
			newEmployee.HadProperFood = employee.HadProperFood;

			// Friendships/LeadSpecialization/LeadProjects are obsolete (superseded by the Fix-suffixed fields).
			newEmployee.LeadProjectsFix = employee.LeadProjectsFix;
			newEmployee.DemandsMet = employee.DemandsMet;
			newEmployee.DemandsRequested = employee.DemandsRequested;
			newEmployee.DemandResults = employee.DemandResults;
			newEmployee.LastDemandScore = employee.LastDemandScore;

			foreach (var kvp in employee.LeadSpecializationFix)
			{
				newEmployee.LeadSpecializationFix[kvp.Key] = kvp.Value;
			}

			// Not set by the constructor: specialization experience and creativity history.
			for (var role = 0; role < Employee.RoleCount; role++)
			{
				newEmployee.SetSpecExperience((Employee.EmployeeRole)role, employee.GetSpecExperience((Employee.EmployeeRole)role));
			}

			if (employee.LastCreatity != null)
			{
				newEmployee.LastCreatity = (float[])employee.LastCreatity.Clone();
			}

			return newEmployee;
		}

		private static void CopyRoles(Employee employee, Employee newEmployee)
		{
			var roles = Employee.RoleBit.None;
			var secondaryRoles = Employee.RoleBit.None;
			for (var i = 0; i < Employee.RoleCount; i++)
			{
				if (employee.IsRoleIndex(i))
				{
					roles |= Employee.RoleToMask[i];
				}

				if (employee.IsSecondaryRoleIndex(i))
				{
					secondaryRoles |= Employee.RoleToMask[i];
				}
			}
			newEmployee.SetRoles(roles, secondaryRoles);
		}

		// Puts the original employee back on its actor and points the game's references at it again.
		private static void RevertCreativityEdit(Actor actor, Employee original, Employee replacement)
		{
			actor.employee = original;
			original.MyActor = actor;
			replacement.MyActor = null;
			EmployeeReferenceMigration.Migrate(replacement, original);
		}

		private static void SetInspiration()
		{
			TargetSource source;
			var target = EmployeeTarget.Resolve(out source);
			if (source != TargetSource.DetailWindow)
				return;
			var employee = target.Employee;

			InputHelper.RequestFloat(
				$"Current is {employee.Inspiration}\nMin = 0, Max = 2.0",
				$"Set inspiration for {employee.Name}",
				val =>
				{
					if (!target.IsStillValid())
					{
						Notification.ShowError("The employee changed or is not available. Nothing was edited.");
						return;
					}

					employee.Inspiration = val;
				},
				min: 0,
				max: 2);
		}
	}
}
