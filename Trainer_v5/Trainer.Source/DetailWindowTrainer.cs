using System.Collections.Generic;
using Trainer_v5.Trainer.Source.Window;
using Trainer_v5.Window;
using UnityEngine;

namespace Trainer_v5
{
	internal static class DetailWindowTrainer
	{
		private static bool _installed;

		public static void Reset() => _installed = false;

		private static Employee CurrentEmployee => HUD.Instance.DetailWindow?.CurrentEmployee?.employee;

		public static void Install()
		{
			if (_installed) return;
			_installed = true;

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
					newEmployee.Salary = employee.Salary;
					newEmployee.CreativityKnown = 1f;
					newEmployee.MyEmployer = employee.MyEmployer;
					newEmployee.BirthDate = employee.BirthDate;
					newEmployee.Hired = employee.Hired;
					newEmployee.Thoughts = employee.Thoughts;
					newEmployee.JobSatisfaction = employee.JobSatisfaction;
					newEmployee.NickName = employee.NickName;
					newEmployee.Founder = employee.Founder;
					newEmployee.MadeCEO = employee.MadeCEO;
					newEmployee.Dismissed = employee.Dismissed;
					newEmployee.Retired = employee.Retired;
					newEmployee.PreviousEmployment = employee.PreviousEmployment;
					newEmployee.AgeMonth = employee.AgeMonth;
					newEmployee.SkillCeiling = employee.SkillCeiling;
					newEmployee.LowestSatisfaction = employee.LowestSatisfaction;
					newEmployee.DemandsMet = employee.DemandsMet;
					newEmployee.DemandsRequested = employee.DemandsRequested;
					newEmployee.CustomBenefits = employee.CustomBenefits;

					// transfer lead specs and projects
					foreach (var kvp in employee.LeadSpecializationFix)
						newEmployee.LeadSpecializationFix[kvp.Key] = kvp.Value;

					foreach (var p in employee.LeadProjects)
						newEmployee.LeadProjects.Add(p);

					foreach (var id in employee.LeadProjectsFix)
						newEmployee.LeadProjectsFix.Add(id);

					var actor = employee.MyActor;
					if (actor != null)
					{
						employee.MyActor = null;
						actor.employee = newEmployee;
						newEmployee.MyActor = actor;

						// DetailWindow.CurrentEmployee is the Actor; since actor.employee now
						// points to newEmployee the window will read the updated data, but we
						// force a Show() so any cached UI state is also refreshed.
						if (HUD.Instance?.DetailWindow?.CurrentEmployee == actor)
							HUD.Instance.DetailWindow.Show(actor);
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
