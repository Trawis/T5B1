using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Window
{
	public class EmployeeSkillChangeWindow : MonoBehaviour
	{
		public static GUIWindow Window { get; set; }

		public static void Show()
		{
			if (!EmployeeTarget.RequireAny())
			{
				return;
			}

			if (Window == null)
			{
				Window = CreateWindow();
			}
			else
			{
				Window.Toggle();
			}

			Refresh();
		}

		public static void Refresh()
		{
			var window = Window;
			if (window == null || !window.Shown)
			{
				return;
			}

			var selection = EmployeeTarget.ResolveSelection();
			window.SetTitle(selection.Count == 1
				? SharedMessages.EditSkillsTitle(selection[0].Employee.Name)
				: string.Format("EditSkillsSelectedTitle".LocDef("Edit skills for {0} selected employee(s)"), selection.Count));
		}

		// The window's toggles are bound to the specialization list of one game, so both go together.
		public static void Reset()
		{
			Window = null;
		}

		private static GUIWindow CreateWindow()
		{
			var window = UIHelper.SpawnWindow(SharedMessages.EditSkillsTitle(EmployeeTarget.Describe(null, TargetSource.None)), "EditEmployee", "EditEmployeePanel");

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
				UIFactory.Label("RolesLabel".LocDef("Roles")).gameObject
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
				UIFactory.Button("SetSkillsButton".LocDef("Set Skills"), SetSkills).gameObject,
				UIFactory.Button("SetBaseSkillsButton".LocDef("Set Base Skills"), SetBaseSkills).gameObject,
				UIFactory.Button(SharedMessages.EditorRefreshButton(), Refresh).gameObject
			};
		}

		private static GameObject[] SecondColumnHeader()
		{
			return new[]
			{
				UIFactory.Label("SpecializationsLabel".LocDef("Specializations")).gameObject
			};
		}

		private static GameObject[] SpecializationToggles()
		{
			var specs = Helpers.SpecializationsList;
			return specs
				.Select(spec => UIFactory.Toggle(spec.Key, specs.GetOrDefault(spec.Key), a => specs.Toggle(spec.Key)).gameObject)
				.ToArray();
		}

		private static List<EmployeeTarget> CaptureTargets()
		{
			var targets = EmployeeTarget.ResolveSelection();
			if (targets.Count == 0)
			{
				Notification.ShowError(SharedMessages.SelectEmployeesError());
				return null;
			}

			return targets;
		}

		private static List<Employee.EmployeeRole> CaptureRoles()
		{
			var roles = Helpers.RolesList
				.Where(r => r.Value)
				.Select(e => e.Key.ToEmployeeRole())
				.ToList();

			if (roles.Count == 0)
			{
				Notification.ShowError("SelectRolesError".LocDef("Select one or more roles."));
				return null;
			}

			return roles;
		}

		private static List<string> CaptureSpecializations()
		{
			var specializations = Helpers.SpecializationsList
				.Where(s => s.Value)
				.Select(s => s.Key)
				.ToList();

			if (specializations.Count == 0)
			{
				Notification.ShowError("SelectSpecializationsError".LocDef("Select one or more specializations."));
				return null;
			}

			return specializations;
		}

		private static void SetSkills()
		{
			var targets = CaptureTargets();
			if (targets == null)
			{
				return;
			}

			var roles = CaptureRoles();
			if (roles == null)
			{
				return;
			}

			var specializations = CaptureSpecializations();
			if (specializations == null)
			{
				return;
			}

			WindowManager.SpawnInputDialog(
				"StarsPrompt".LocDef("By how many stars do you want to change the specialization?\nAllowed: -3, -2, -1, 1, 2, 3 (a specialization stays between 0 and 3 stars)"),
				"StarsTitle".LocDef("Stars amount"),
				"3",
				input =>
				{
					int stars;
					if (!SkillEdit.TryParseStars(input, out stars))
					{
						Notification.ShowError("InvalidStarsInput".LocDef("Invalid input!\nAllowed inputs are: -3, -2, -1, 1, 2, 3"));
						return;
					}

					var applied = 0;
					foreach (var target in targets)
					{
						if (!target.IsStillValid())
							continue;

						foreach (var role in roles)
						{
							foreach (var specialization in specializations)
							{
								target.Employee.AddSpecialization(role, specialization, false, true, stars);
							}
						}

						applied++;
					}

					Report(applied, targets.Count, true);
				});
		}

		private static void SetBaseSkills()
		{
			var targets = CaptureTargets();
			if (targets == null)
			{
				return;
			}

			var roles = CaptureRoles();
			if (roles == null)
			{
				return;
			}

			InputHelper.RequestFloat(
				"BaseSkillPrompt".LocDef("How many base skill do you want?\nMin = 0, Max = 1.0\nThis also raises each selected employee's skill ceiling to 1.0."),
				string.Format("SetBaseSkillTitle".LocDef("Set base skill for {0} actor(s)"), targets.Count),
				val =>
				{
					var applied = 0;
					foreach (var target in targets)
					{
						if (!target.IsStillValid())
							continue;

						target.Employee.SkillCeiling = 1f;
						foreach (var role in roles)
						{
							target.Employee.ChangeSkillDirect(role, val);
						}

						applied++;
					}

					Report(applied, targets.Count, false);
				},
				0f,
				1f);
		}

		private static void Report(int applied, int total, bool announceSuccess)
		{
			switch (SkillEdit.Outcome(applied, total))
			{
				case SkillOutcome.All:
					if (announceSuccess)
					{
						Notification.Popup("EmployeeSkillsSet".LocDef("Trainer: Employee skills/specializations are set!"));
					}

					break;
				case SkillOutcome.Partial:
					Notification.ShowWarning(string.Format("SkillsPartlyApplied".LocDef("Only {0} of {1} employees could be edited. The others changed or are not available."), applied, total));
					break;
				default:
					Notification.ShowError(SharedMessages.EmployeeChangedNothingEdited());
					break;
			}
		}
	}
}
