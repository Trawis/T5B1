using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Window
{
	public class EmployeeSkillChangeWindow : MonoBehaviour
	{
		private static EmployeeTarget _target;
		private static string _targetName;
		private static Toggle _allSelected;

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

			// Bulk editing is always a fresh choice: every opening starts with the shown employee.
			if (_allSelected != null)
			{
				_allSelected.isOn = false;
			}

			Refresh();
		}

		public static void Refresh()
		{
			TargetSource source;
			if (!EmployeeTarget.TryResolveFor(Window, out _target, out source))
			{
				return;
			}

			_targetName = EmployeeTarget.Describe(_target, source);
			UpdateScope();
		}

		// The window's toggles are bound to the specialization list of one game, so both go together.
		public static void Reset()
		{
			Window = null;
			_target = null;
			_targetName = null;
			_allSelected = null;
		}

		private static GUIWindow CreateWindow()
		{
			var window = UIHelper.SpawnWindow(string.Format("EditSkillsTitle".LocDef("Edit skills for {0}"), EmployeeTarget.Describe(null, TargetSource.None)), "EditEmployee", "EditEmployeePanel");

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
			_allSelected = UIFactory.Toggle(ScopeLabel(0), false, isOn => UpdateScope());

			return new[]
			{
				UIFactory.Label("RolesLabel".LocDef("Roles")).gameObject,
				_allSelected.gameObject
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
				UIFactory.Button("EditorRefreshButton".LocDef("Refresh"), Refresh).gameObject
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

		private static string ScopeLabel(int selectedCount)
		{
			return string.Format("SkillScopeAllSelected".LocDef("All selected ({0})"), selectedCount);
		}

		private static void UpdateScope()
		{
			var window = Window;
			if (window == null || _allSelected == null)
			{
				return;
			}

			var selectedCount = EmployeeTarget.ResolveSelection().Count;
			_allSelected.GetComponentInChildren<Text>().text = ScopeLabel(selectedCount);

			var title = _allSelected.isOn
				? string.Format("EditSkillsSelectedTitle".LocDef("Edit skills for {0} selected employee(s)"), selectedCount)
				: string.Format("EditSkillsTitle".LocDef("Edit skills for {0}"), _targetName);
			window.SetTitle(title);
		}

		// The employees the next write may touch, captured at the click; null after the error shown for an empty choice.
		private static List<EmployeeTarget> CaptureTargets()
		{
			var allSelected = _allSelected != null && _allSelected.isOn;
			var selection = allSelected ? EmployeeTarget.ResolveSelection() : new List<EmployeeTarget>();
			var viewedIsValid = _target != null && _target.IsStillValid();

			switch (SkillEdit.Decide(allSelected, viewedIsValid, selection.Count))
			{
				case SkillTargets.Selection:
					return selection;
				case SkillTargets.Viewed:
					return new List<EmployeeTarget> { _target };
			}

			if (allSelected)
			{
				Notification.ShowError("SelectEmployeesError".LocDef("Select one or more employees."));
			}
			else if (_target == null)
			{
				Notification.ShowError("SelectEmployeeFirst".LocDef("Select an employee first (select one employee, then close and reopen this window)."));
			}
			else
			{
				Notification.ShowError("EmployeeChangedPressRefresh".LocDef("The employee changed or is not available. Press Refresh."));
			}

			return null;
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
					Notification.ShowError("EmployeeChangedNothingEdited".LocDef("The employee changed or is not available. Nothing was edited."));
					break;
			}
		}
	}
}
