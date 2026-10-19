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
