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
				// UIFactory.Button("Creativity",  SetCreativity),
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

			Notification.ShowError(string.Join("\n", employee.LastCreatity));
			InputHelper.RequestFloat(
				$"Current is {employee.Creativity}\nMin = 0, Max = 1.0",
				$"Set creativity for {employee.Name}",
				val =>
				{
					// set by CreativityKnown: not work
					var factor = val / employee.Creativity;
					employee.CreativityKnown = factor;

					// LastCreativity: not wok
					// employee.LastCreatity = new []{val, val};
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
