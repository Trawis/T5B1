﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Trainer_v5.Actions;
using Trainer_v5;

namespace Trainer_v5
{
	public class EmployeeSkillChangeWindow : MonoBehaviour
	{
		private static readonly string _title = "Employee Skill Change, by Trawis";

		public static GUIWindow Window { get; set; }

		public static void Show()
		{
			if (Window == null)
			{
				Window = CreateWindow();
			}
			else
			{
				Window.Toggle();
			}
		}

		private static GUIWindow CreateWindow()
		{
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = _title;
			window.name = "EditEmployee";
			window.MainPanel.name = "EditEmployeePanel";

			var firstColumn = FirstColumn().ToList();
			var secondColumn = SecondColumn().ToList();

			firstColumn.AddToWindow(window, Constants.FIRST_COLUMN);
			secondColumn.AddToWindow(window, Constants.SECOND_COLUMN);

			var maxRows = Math.Max(firstColumn.Count, secondColumn.Count);
			window.SetWindowSize(maxRows, Constants.X_EMPLOYEESKILLCHANGE_WINDOW);

			return window;
		}

		private static IEnumerable<GameObject> FirstColumn()
		{
			yield return UIHelper.CreateLabel("Roles");
			yield return UIHelper.EmptyBox();

			var rolesList = Helpers.RolesList;
			foreach (var role in rolesList)
			{
				yield return UIHelper.CreateToggle(role.Key, rolesList.GetOrDefault(role.Key), a => rolesList.Toggle(role.Key));
			}

			yield return UIHelper.CreateButton("Set Skills", EmployeeActions.SetSkillPerEmployee);
			yield return UIHelper.EmptyBox();
			yield return UIHelper.CreateButton("Set Base Skills", SetBaseSkills);
		}

		private static IEnumerable<GameObject> SecondColumn()
		{
			yield return UIHelper.CreateLabel("Specializations");
			yield return UIHelper.EmptyBox();

			var specs = Helpers.SpecializationsList;
			foreach (var spec in specs)
			{
				yield return UIHelper.CreateToggle(spec.Key, specs.GetOrDefault(spec.Key), a => specs.Toggle(spec.Key));
			}
		}

		private static void SetBaseSkills()
		{
			var selectedActors = SelectorController.Instance.Selected.OfType<Actor>().ToList();
			var selectedRoles = Helpers.RolesList
				.Where(r => r.Value)
				.Select(e => e.Key.ToEmployeeRole())
				.ToList();

			if (selectedActors.Count == 0)
			{
				Notification.ShowError("Select one or more employees.");
			}
			else if (selectedRoles.Count == 0)
			{
				Notification.ShowError("Select one or more roles.");
			}
			else
			{

				InputHelper.RequestFloat(
					"How many base skill do you want?\nMin = 0, Max = 1.0",
					$"Set base skill for {selectedActors.Count} actor(s)",
					val => selectedActors.ForEach(actor => selectedRoles.ForEach(role =>
					{
						actor.employee.SkillCeiling = 1f;
						actor.employee.ChangeSkillDirect(role, val);
					}
					)));
			}
		}
	}

}
