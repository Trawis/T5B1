using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils = Trainer_v5.Utilities;

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

			var firstColumn = FirstColumn().ToArray();
			var secondColumn = SecondColumn().ToArray();

			Utils.CreateGameObjects(Constants.FIRST_COLUMN, firstColumn, window);
			Utils.CreateGameObjects(Constants.SECOND_COLUMN, secondColumn, window);

			var maxRows = Math.Max(firstColumn.Length, secondColumn.Length);
			Utils.SetWindowSize(maxRows, Constants.X_EMPLOYEESKILLCHANGE_WINDOW, window);

			return window;
		}

		private static IEnumerable<GameObject> FirstColumn()
		{
			yield return UIFactory.Label("Roles").gameObject;
			yield return UIFactory.EmptyBox().gameObject;

			var rolesList = Helpers.RolesList;
			foreach (var role in rolesList)
			{
				yield return UIFactory.Toggle(role.Key, rolesList.GetOrDefault(role.Key), a => rolesList.Toggle(role.Key)).gameObject;
			}

			yield return UIFactory.Button("Set Skills", TrainerBehaviour.SetSkillPerEmployee).gameObject;
			yield return UIFactory.EmptyBox().gameObject;
			yield return UIFactory.Button("Set Base Skills", SetBaseSkills).gameObject;
		}

		private static IEnumerable<GameObject> SecondColumn()
		{
			yield return UIFactory.Label("Specializations").gameObject;
			yield return UIFactory.EmptyBox().gameObject;

			var specs = Helpers.SpecializationsList;
			foreach (var spec in specs)
			{
				yield return UIFactory.Toggle(spec.Key, specs.GetOrDefault(spec.Key), a => specs.Toggle(spec.Key)).gameObject;
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