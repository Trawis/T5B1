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
			var placed = false;
			try
			{
				var allAdded = true;
				for (var i = 0; i < components.Count; i++)
				{
					allAdded &= components[i].gameObject.AddToElement("DetailWindow", slots[i]);
				}

				placed = allAdded;
			}
			finally
			{
				// A missing window element or an exception partway drops every control, so a retry starts clean
				// instead of stacking a second set on top of the buttons that were already added.
				if (!placed)
				{
					for (var i = 0; i < components.Count; i++)
					{
						Object.Destroy(components[i].gameObject);
					}
				}
			}

			if (!placed)
				return;

			// Marked installed only after every control was placed, so a failure leaves a retry possible.
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
