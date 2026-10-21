using System.Collections.Generic;
using Trainer_v5.Window;
using UnityEngine;

namespace Trainer_v5
{
	internal static class EmployeesWindowTrainer
	{
		// The Employees window the controls were added to; the game recreates it with the HUD on save reloads.
		private static EmployeeWindow _installedOn;

		public static void Reset()
		{
			_installedOn = null;
		}

		public static void Install()
		{
			var target = HUD.Instance?.employeeWindow;
			if (target == null || target.Window == null || target == _installedOn)
				return;

			var components = new List<Component>
			{
				UIFactory.Button("DetailTraitButton".LocDef("Trait"),  () => EmployeeTraitChangeWindow.Instance.Show()),
				UIFactory.Button("DetailDemandButton".LocDef("Demand"), () => EmployeeDemandChangeWindow.Instance.Show()),
				UIFactory.Button("DetailInspirationButton".LocDef("Inspiration"), SetInspiration),
				UIFactory.Button("DetailLeadSpecButton".LocDef("LeadSpec"), () => EmployeeLeadSpecChangeWindow.Instance.Show()),
				UIFactory.Button("DetailFounderButton".LocDef("Founder"), FounderToggle.Start),
				UIFactory.Button("DetailSkillsButton".LocDef("Skills"), EmployeeSkillChangeWindow.Show),
			};

			const int width = 80, height = 32, spacing = 4;
			var windowRect = ((RectTransform)target.Window.transform).rect;
			var slots = DetailButtonLayout.Place(components.Count, windowRect.width, width, height, spacing);
			var placed = false;
			try
			{
				// Above the window's top edge, so no list row, filter, header or scrollbar of the native window is covered.
				for (var i = 0; i < components.Count; i++)
				{
					UIHelper.PlaceInElement(components[i].gameObject, target.Window.gameObject, slots[i]);
				}

				placed = true;
			}
			finally
			{
				// An exception partway drops every control, so a retry starts clean
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
			var target = EmployeeTarget.RequireOne();
			if (target == null)
				return;
			var employee = target.Employee;

			InputHelper.RequestFloat(
				string.Format("InspirationPrompt".LocDef("Current is {0}\nMin = 0, Max = 2.0"), employee.Inspiration),
				string.Format("SetInspirationTitle".LocDef("Set inspiration for {0}"), employee.Name),
				val =>
				{
					if (!target.IsStillValid())
					{
						Notification.ShowError(SharedMessages.EmployeeChangedNothingEdited());
						return;
					}

					employee.Inspiration = val;
				},
				min: 0,
				max: 2);
		}
	}
}
