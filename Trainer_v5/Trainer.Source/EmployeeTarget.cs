using System.Collections.Generic;

namespace Trainer_v5
{
	internal enum TargetSource
	{
		None = 0,
		// The employee shown in the open detail window: the editors are opened from it.
		DetailWindow = 1,
		// Exactly one employee selected while no detail window is open.
		Selection = 2,
		// Several selected and no detail window: there is no single intended employee.
		Ambiguous = 3
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

		// An open detail window is the context the editor buttons live in, so it wins over a wider selection.
		public static TargetSource Decide(bool detailWindowHasLiveEmployee, int liveSelectedCount)
		{
			if (detailWindowHasLiveEmployee)
			{
				return TargetSource.DetailWindow;
			}

			if (liveSelectedCount == 1)
			{
				return TargetSource.Selection;
			}

			return liveSelectedCount > 1 ? TargetSource.Ambiguous : TargetSource.None;
		}

		public static EmployeeTarget Resolve(out TargetSource source)
		{
			var detailActor = ShownDetailActor();
			var selected = LiveSelectedActors();

			source = Decide(IsLive(detailActor), selected.Count);
			switch (source)
			{
				case TargetSource.DetailWindow:
					return new EmployeeTarget(detailActor);
				case TargetSource.Selection:
					return new EmployeeTarget(selected[0]);
				default:
					return null;
			}
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

		private static List<Actor> LiveSelectedActors()
		{
			var selected = new List<Actor>();
			var selection = SelectorController.Instance != null ? SelectorController.Instance.Selected : null;
			if (selection != null)
			{
				foreach (var item in selection)
				{
					var actor = item as Actor;
					if (IsLive(actor))
					{
						selected.Add(actor);
					}
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

		// DetailWindow.CurrentEmployee can stay set after the window closes, so it only counts while shown.
		private static Actor ShownDetailActor()
		{
			var detail = HUD.Instance != null ? HUD.Instance.DetailWindow : null;
			if (detail == null || detail.Window == null || !detail.Window.Shown)
			{
				return null;
			}

			return detail.CurrentEmployee;
		}
	}
}
