using System.Collections.Generic;
using Trainer_v5.Trainer.Source.Window;
using Trainer_v5.Window;
using UnityEngine;

namespace Trainer_v5
{
	internal static class DetailWindowTrainer
	{
		private static bool _installed;

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
