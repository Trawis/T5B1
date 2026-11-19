using System.Collections.Generic;

namespace Trainer_v5
{
	internal enum TargetSource
	{
		None = 0,
		Selection = 1,
		Ambiguous = 2
	}

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

		public static bool RequireAny()
		{
			if (ResolveSelection().Count == 0)
			{
				Notification.ShowError(SharedMessages.SelectEmployeesError());
				return false;
			}

			return true;
		}

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

		public static List<EmployeeTarget> ResolveSelection()
		{
			var targets = new List<EmployeeTarget>();
			foreach (var actor in LiveSelectedActors())
			{
				targets.Add(new EmployeeTarget(actor));
			}

			return targets;
		}

		public static List<EmployeeTarget> ResolveAll()
		{
			var targets = new List<EmployeeTarget>();
			var settings = GameSettings.Instance;
			if (settings == null || settings.sActorManager == null)
			{
				return targets;
			}

			foreach (var actor in settings.sActorManager.Actors)
			{
				if (IsLive(actor))
				{
					targets.Add(new EmployeeTarget(actor));
				}
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

		public bool IsStillValid()
		{
			return IsLive(Actor) && Actor.employee == Employee && IsInActiveGame(Actor);
		}

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

		private static bool IsInActiveGame(Actor actor)
		{
			var settings = GameSettings.Instance;
			return settings != null && settings.sActorManager != null && settings.sActorManager.Actors.Contains(actor);
		}
	}
}
