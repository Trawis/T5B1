using System.Collections.Generic;
using Trainer_v5.Trainer.Source.Window;
using Trainer_v5.Window;
using UnityEngine;

namespace Trainer_v5
{
	internal static class DetailWindowTrainer
	{
		// Tracks the specific DetailWindow instance the trainer controls were injected into.
		// The game destroys and recreates DetailWindow on save reloads and some scene
		// transitions, so a plain "installed once" flag would permanently skip
		// re-installation on every DetailWindow after the first one.
		private static DetailWindow _installedOn;

		public static void Reset()
		{
			_installedOn = null;
		}

		private static Employee CurrentEmployee => HUD.Instance.DetailWindow?.CurrentEmployee?.employee;

		public static void Install()
		{
			var target = HUD.Instance?.DetailWindow;
			if (target == null || target == _installedOn) return;

			var components = new List<Component>
			{
				UIFactory.Button("Trait",  () => EmployeeTraitChangeWindow.Instance.Show()),
				UIFactory.Button("Demand", () => EmployeeDemandChangeWindow.Instance.Show()),
				UIFactory.Button("Creativity",  SetCreativity),
				UIFactory.Button("Inspiration", SetInspiration),
				UIFactory.Button("LeadSpec", () => EmployeeLeadSpecChangeWindow.Instance.Show()),
			};

			// add components to DetailWindow
			const int width = 80, height = 32, spacing = 4;
			for (var i = 0; i < components.Count; i++)
			{
				var x = i * (width + spacing);
				const int y = 0 - spacing - height;
				Utilities.AddElementToElement(components[i].gameObject, "DetailWindow", new Rect(x, y, width, height));
			}

			// Only mark this instance as installed once the loop above completes
			// successfully, so a mid-loop exception leaves retry possible instead of
			// permanently blocking installation.
			_installedOn = target;
		}

		private static void SetCreativity()
		{
			var employee = CurrentEmployee;
			if (employee == null) return;

			InputHelper.RequestFloat(
				$"Current is {employee.Creativity}\nMin = 0, Max = 1.0",
				$"Set creativity for {employee.Name}",
				val =>
				{
					var skills = new float[5];
					for(int i = 0; i < 5; i++) {
						skills[i] = employee.GetSkillI(i);
					}
					
					// clone employee with new creativity value
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
					
					// transfer properties
					// This constructor overload also generates founder characters, so it always sets Founder = true.
					newEmployee.Founder = employee.Founder;
					newEmployee.Dismissed = employee.Dismissed;
					newEmployee.Retired = employee.Retired;
					newEmployee.MadeCEO = employee.MadeCEO;

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
					newEmployee.LastCreatity = employee.LastCreatity;
					newEmployee.ActiveComplaint = employee.ActiveComplaint;
					newEmployee.Filter = employee.Filter;
					newEmployee.PreviousEmployment = employee.PreviousEmployment;
					newEmployee.CustomBenefits = employee.CustomBenefits;

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

					// Friendships/LeadSpecialization/LeadProjects are obsolete; superseded by the Fix-suffixed fields.
					newEmployee.LeadProjectsFix = employee.LeadProjectsFix;
					newEmployee.DemandsMet = employee.DemandsMet;
					newEmployee.DemandsRequested = employee.DemandsRequested;
					newEmployee.DemandResults = employee.DemandResults;
					newEmployee.LastDemandScore = employee.LastDemandScore;

					// transfer lead specs
					foreach (var kvp in employee.LeadSpecializationFix)
					{
						newEmployee.LeadSpecializationFix[kvp.Key] = kvp.Value;
					}
					
					var actor = employee.MyActor;
					if (actor != null)
					{
						// update actor references
						employee.MyActor = null;
						actor.employee = newEmployee;
						newEmployee.MyActor = actor;
						
						if (HUD.Instance?.DetailWindow?.CurrentEmployee?.employee == employee)
						{
							HUD.Instance.DetailWindow.CurrentEmployee.employee = newEmployee;
						}
					}
				},
				min: 0,
				max: 1);
		}

		private static void SetInspiration()
		{
			var employee = CurrentEmployee;
			if (employee == null) return;

			InputHelper.RequestFloat(
				$"Current is {employee.Inspiration}\nMin = 0, Max = 2.0",
				$"Set inspiration for {employee.Name}",
				val => employee.Inspiration = val,
				min: 0, 
				max: 2);
		}
	}
}
