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
					// Creativity is a plain public field on Employee, so it can be updated
					// directly on the existing object instead of replacing it. This keeps
					// the object identity intact for any other references the game holds
					// (e.g. actor.employee) and preserves every other field automatically.
					employee.Creativity = val;
					employee.CreativityKnown = 1f;
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
