using System;
using System.Linq;
using UnityEngine;

namespace Trainer_v5.Window
{
	public class EmployeeSkillChangeWindow : MonoBehaviour
	{
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

		// The window's toggles are bound to the specialization list of one game, so both go together.
		public static void Reset()
		{
			Window = null;
		}

		private static GUIWindow CreateWindow()
		{
			var window = UIHelper.SpawnWindow("SkillChangeWindowTitle".LocDef("Employee Skill Change, by Trawis"), "EditEmployee", "EditEmployeePanel");

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

			window.MinSize = new Vector2(Constants.X_EMPLOYEESKILLCHANGE_WINDOW, UIHelper.GetColumnWindowHeight(firstColumnHeight, secondColumnHeight, 0));

			return window;
		}

		private static GameObject[] FirstColumnHeader()
		{
			return new[]
			{
				UIFactory.Label("RolesLabel".LocDef("Roles")).gameObject,
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
				UIFactory.Button("SetSkillsButton".LocDef("Set Skills"), EmployeeFeatures.SetSkillPerEmployee).gameObject,
				UIFactory.EmptyBox().gameObject,
				UIFactory.Button("SetBaseSkillsButton".LocDef("Set Base Skills"), SetBaseSkills).gameObject
			};
		}

		private static GameObject[] SecondColumnHeader()
		{
			return new[]
			{
				UIFactory.Label("SpecializationsLabel".LocDef("Specializations")).gameObject,
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
				Notification.ShowError("SelectEmployeesError".LocDef("Select one or more employees."));
			}
			else if (selectedRoles.Count == 0)
			{
				Notification.ShowError("SelectRolesError".LocDef("Select one or more roles."));
			}
			else
			{

				InputHelper.RequestFloat(
					"BaseSkillPrompt".LocDef("How many base skill do you want?\nMin = 0, Max = 1.0\nThis also raises each selected employee's skill ceiling to 1.0."),
					string.Format("SetBaseSkillTitle".LocDef("Set base skill for {0} actor(s)"), selectedActors.Count),
					val => selectedActors.Where(EmployeeTarget.IsLive).ToList().ForEach(actor => selectedRoles.ForEach(role =>
					{
						actor.employee.SkillCeiling = 1f;
						actor.employee.ChangeSkillDirect(role, val);
					}
					)),
					0f,
					1f);
			}
		}
	}

}
