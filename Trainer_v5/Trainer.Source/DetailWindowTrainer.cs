﻿﻿﻿﻿﻿﻿﻿using System.Collections.Generic;
// Removed using Trainer_v5.Trainer.Source.Window;
using Trainer_v5.Window; // This namespace now contains the relevant window classes
using UnityEngine;

namespace Trainer_v5
{
	internal static class DetailWindowTrainer
	{
		private static bool _installed;

		// Layout constants for buttons
		private const int ButtonWidth = 80;
		private const int ButtonHeight = 32;
		private const int ButtonSpacing = 4;
		private const int ButtonYOffset = 0 - ButtonSpacing - ButtonHeight; // Position below existing elements

		// Safer property access
		private static Employee CurrentEmployee
		{
			get
			{
				// Added null checks for safety
				var detailWindow = HUD.Instance?.DetailWindow;
				return detailWindow?.CurrentEmployee?.employee;
			}
		}


		public static void Install()
		{
			if (_installed) return;
			_installed = true;

			// Changed to List<GameObject> and use UIHelper
			var components = new List<GameObject>
			{
				UIHelper.CreateButton("Trait",  () => EmployeeTraitChangeWindow.Instance.Show()),
				UIHelper.CreateButton("Demand", () => EmployeeDemandChangeWindow.Instance.Show()),
				UIHelper.CreateButton("Creativity",  SetCreativity),
				UIHelper.CreateButton("Inspiration", SetInspiration),
				UIHelper.CreateButton("LeadSpec", () => EmployeeLeadSpecChangeWindow.Instance.Show()),
			};

			// add components to DetailWindow using constants
			for (var i = 0; i < components.Count; i++)
			{
				var x = i * (ButtonWidth + ButtonSpacing);
				// Use UIHelper extension method AddToElement
				components[i].AddToElement("DetailWindow", new Rect(x, ButtonYOffset, ButtonWidth, ButtonHeight));
			}
		}

		/// <summary>
		/// Creates a new Employee instance with updated creativity.
		/// This is a workaround, likely because directly modifying creativity might have side effects
		/// or is not persisted correctly by the game's systems.
		/// </summary>
		private static Employee CloneEmployeeWithNewCreativity(Employee originalEmployee, float newCreativity)
		{
			var skills = new float[5];
			for(int i = 0; i < 5; i++) {
				skills[i] = originalEmployee.GetSkillI(i);
			}

			// clone employee with new creativity value
			var newEmployee = new Employee(
				currentTime: SDateTime.Now(),
				female: originalEmployee.Female,
				name: originalEmployee.Name,
				skills: skills,
				creativity: newCreativity, // Use the new value
				person: originalEmployee.PersonalityTraits,
				traits: originalEmployee.Traits,
				specs: originalEmployee.GetAllSpecializations(),
				graph: GameSettings.Instance.Personalities,
				style: originalEmployee.StyleGen,
				forceBrain: originalEmployee.HiredFor
			);

			// transfer properties
			newEmployee.Salary = originalEmployee.Salary;
			newEmployee.CreativityKnown = 1f; // Assume creativity is now known
			newEmployee.MyEmployer = originalEmployee.MyEmployer;
			newEmployee.BirthDate = originalEmployee.BirthDate;
			newEmployee.Hired = originalEmployee.Hired;
			newEmployee.Thoughts = originalEmployee.Thoughts;
			newEmployee.JobSatisfaction = originalEmployee.JobSatisfaction;

			// transfer lead specs
			foreach (var kvp in originalEmployee.LeadSpecializationFix)
			{
				newEmployee.LeadSpecializationFix[kvp.Key] = kvp.Value;
			}

			return newEmployee;
		}

		private static void SetCreativity()
		{
			var originalEmployee = CurrentEmployee;
			if (originalEmployee == null) return;

			InputHelper.RequestFloat(
				$"Current is {originalEmployee.Creativity}\nMin = 0, Max = 1.0",
				$"Set creativity for {originalEmployee.Name}",
				newCreativityValue =>
				{
					// Use the helper method to create the new employee instance
					var newEmployee = CloneEmployeeWithNewCreativity(originalEmployee, newCreativityValue);

					var actor = originalEmployee.MyActor;
					if (actor != null)
					{
						// Update actor references to point to the new employee instance
						originalEmployee.MyActor = null; // Disassociate old employee
						actor.employee = newEmployee;
						newEmployee.MyActor = actor;

						// Update the detail window if it's showing the employee we just replaced
						if (HUD.Instance?.DetailWindow?.CurrentEmployee?.employee == originalEmployee)
						{
							HUD.Instance.DetailWindow.CurrentEmployee.employee = newEmployee;
							// Optionally force a refresh if the window doesn't update automatically
							// HUD.Instance.DetailWindow.Show(actor);
						}
					}
					else
					{
						// Handle case where actor might not be found? Log warning?
						$"Could not find Actor for employee {originalEmployee.Name} during creativity update.".Log(false);
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
