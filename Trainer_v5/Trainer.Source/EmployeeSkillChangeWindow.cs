using System;
using System.Linq;
using UnityEngine;

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

			var firstHeader = FirstColumnHeader();
			var roleToggles = RoleToggles();
			var firstFooter = FirstColumnFooter();

			var secondHeader = SecondColumnHeader();
			var specToggles = SpecializationToggles();
			var secondFooter = new GameObject[0];

			var reservedRows = Math.Max(firstHeader.Length + firstFooter.Length, secondHeader.Length + secondFooter.Length);
			var maxVisibleRows = UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, reservedRows);

			var firstColumnHeight = UIHelper.CreateScrollableColumn(
				window, new Rect(Constants.FIRST_COLUMN, 0, Constants.ELEMENT_WIDTH, 0),
				firstHeader, roleToggles, firstFooter,
				Constants.ELEMENT_HEIGHT, 0, maxVisibleRows);

			var secondColumnHeight = UIHelper.CreateScrollableColumn(
				window, new Rect(Constants.SECOND_COLUMN, 0, Constants.ELEMENT_WIDTH, 0),
				secondHeader, specToggles, secondFooter,
				Constants.ELEMENT_HEIGHT, 0, maxVisibleRows);

			window.MinSize = new Vector2(Constants.X_EMPLOYEESKILLCHANGE_WINDOW, Mathf.Max(firstColumnHeight, secondColumnHeight) + Constants.ELEMENT_HEIGHT);

			return window;
		}

		private static GameObject[] FirstColumnHeader()
		{
			return new[]
			{
				UIFactory.Label("Roles").gameObject,
				UIFactory.EmptyBox().gameObject
			};
		}

		private static GameObject[] RoleToggles()
		{
			var rolesList = Helpers.RolesList;
			return rolesList
				.Select(role => UIFactory.Toggle(role.Key, rolesList.GetOrDefault(role.Key), a => rolesList.Toggle(role.Key)).gameObject)
				.ToArray();
		}

		private static GameObject[] FirstColumnFooter()
		{
			return new[]
			{
				UIFactory.Button("Set Skills", TrainerBehaviour.SetSkillPerEmployee).gameObject,
				UIFactory.EmptyBox().gameObject,
				UIFactory.Button("Set Base Skills", SetBaseSkills).gameObject
			};
		}

		private static GameObject[] SecondColumnHeader()
		{
			return new[]
			{
				UIFactory.Label("Specializations").gameObject,
				UIFactory.EmptyBox().gameObject
			};
		}

		private static GameObject[] SpecializationToggles()
		{
			var specs = Helpers.SpecializationsList;
			return specs
				.Select(spec => UIFactory.Toggle(spec.Key, specs.GetOrDefault(spec.Key), a => specs.Toggle(spec.Key)).gameObject)
				.ToArray();
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