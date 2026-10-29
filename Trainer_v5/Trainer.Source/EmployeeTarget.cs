using System.Collections.Generic;

namespace Trainer_v5
{
	// The only origin is the Employees window's own list: no other open window and no world selection is consulted.
	internal enum TargetSource
	{
		None = 0,
		// Exactly one employee is selected in the shown Employees window.
		Selection = 1,
		// Several are selected there: there is no single intended employee.
		Ambiguous = 2
	}

	// The employee an editor acts on: resolved once, checked again before every write (#235).
	internal sealed class EmployeeTarget
	{
		public readonly Actor Actor;
		public readonly Employee Employee;

		private EmployeeTarget(Actor actor)
		{
			Actor = actor;
			Employee = actor.employee;
		}

		public static TargetSource Decide(int liveSelectedCount)
		{
			if (liveSelectedCount == 1)
			{
				return TargetSource.Selection;
			}

			return liveSelectedCount > 1 ? TargetSource.Ambiguous : TargetSource.None;
		}

		public static EmployeeTarget Resolve(out TargetSource source)
		{
			var selected = LiveSelectedActors();

			source = Decide(selected.Count);
			return source == TargetSource.Selection ? new EmployeeTarget(selected[0]) : null;
		}

		// For an action on exactly one employee: tells the player why nothing happens without a single selection.
		public static EmployeeTarget RequireOne()
		{
			TargetSource source;
			var target = Resolve(out source);
			if (target == null)
			{
				Notification.ShowError(SharedMessages.SelectOneEmployee());
			}

			return target;
		}

		// For an editor that also edits several employees on request: a launch needs at least one live selected employee.
		public static bool RequireAny()
		{
			if (!HasAny(ResolveSelection().Count))
			{
				Notification.ShowError(SharedMessages.SelectEmployeesError());
				return false;
			}

			return true;
		}

		public static bool HasAny(int liveSelectedCount)
		{
			return liveSelectedCount > 0;
		}

		// An editor window that is closed or missing has no target; true means the target and source are fresh.
		public static bool TryResolveFor(GUIWindow window, out EmployeeTarget target, out TargetSource source)
		{
			if (window == null || !window.Shown)
			{
				target = null;
				source = TargetSource.None;
				return false;
			}

			target = Resolve(out source);
			return true;
		}

		// Every live selected employee, captured at once so a later selection change cannot change who is edited.
		public static List<EmployeeTarget> ResolveSelection()
		{
			var targets = new List<EmployeeTarget>();
			foreach (var actor in LiveSelectedActors())
			{
				targets.Add(new EmployeeTarget(actor));
			}

			return targets;
		}

		public static string Describe(EmployeeTarget target, TargetSource source)
		{
			if (target != null)
			{
				return target.Employee.Name;
			}

			return source == TargetSource.Ambiguous ? "EmployeeSeveralSelected".LocDef("one employee (several are selected)") : "EmployeeNobody".LocDef("Nobody");
		}

		public static bool IsLive(Actor actor)
		{
			return actor != null && actor.employee != null && !actor.employee.Dismissed;
		}

		// False once the actor is gone, dismissed, replaced, or no longer part of the active game.
		public bool IsStillValid()
		{
			return IsLive(Actor) && Actor.employee == Employee && IsInActiveGame(Actor);
		}

		// What the Employees window's list shows as selected right now; nothing while that window is closed or missing.
		private static List<Actor> LiveSelectedActors()
		{
			var selected = new List<Actor>();
			var window = HUD.Instance != null ? HUD.Instance.employeeWindow : null;
			if (window == null || window.Window == null || !window.Window.Shown || window.EmployeeList == null)
			{
				return selected;
			}

			foreach (var actor in window.EmployeeList.GetSelected<Actor>())
			{
				if (IsLive(actor) && IsInActiveGame(actor))
				{
					selected.Add(actor);
				}
			}

			return selected;
		}

		// A save reload or scene change leaves old actors referenced; only the current game's actors count.
		private static bool IsInActiveGame(Actor actor)
		{
			var settings = GameSettings.Instance;
			return settings != null && settings.sActorManager != null && settings.sActorManager.Actors.Contains(actor);
		}
	}
}
