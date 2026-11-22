namespace Trainer_v5
{
	// A founder is unpaid and cannot be fired.
	internal static class FounderToggle
	{
		public static void Start()
		{
			var target = EmployeeTarget.RequireOne();
			if (target == null)
				return;

			var blocked = FindBlocker(target);
			if (blocked != null)
			{
				Notification.ShowError(blocked);
				return;
			}

			var employee = target.Employee;
			var shown = employee.Founder;
			Confirmation.AskAndApply("Founder", string.Format("FounderPrompt".LocDef("{0}: Founder {1} -> {2}"), employee.Name, Label(shown), Label(!shown)), () => FindChange(target, shown), () => Apply(target, shown));
		}

		private static string FindChange(EmployeeTarget target, bool shown)
		{
			var blocked = FindBlocker(target);
			if (blocked != null || target.Employee.Founder == shown)
			{
				return blocked;
			}

			return "EmployeeChangedNoEdit".LocDef("The employee changed. Nothing was edited.");
		}

		private static string Apply(EmployeeTarget target, bool shown)
		{
			target.Employee.Founder = !shown;
			return string.Format("FounderChanged".LocDef("{0}: Founder is now {1}. Select the employee again to refresh the Fire button."), target.Employee.Name, Label(!shown));
		}

		private static string FindBlocker(EmployeeTarget target)
		{
			if (!target.IsStillValid())
			{
				return SharedMessages.EmployeeChangedNothingEdited();
			}

			return null;
		}

		private static string Label(bool founder)
		{
			return founder ? "FounderOn".LocDef("on") : "FounderOff".LocDef("off");
		}
	}
}
