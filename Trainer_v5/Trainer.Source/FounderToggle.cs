namespace Trainer_v5
{
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
			Confirmation.Ask("Founder", string.Format("FounderPrompt".LocDef("{0}: Founder {1} -> {2}"), employee.Name, Label(shown), Label(!shown)), () => Apply(target, shown));
		}

		private static void Apply(EmployeeTarget target, bool shown)
		{
			var blocked = FindBlocker(target);
			if (blocked != null)
			{
				Notification.ShowError(blocked);
				return;
			}

			if (target.Employee.Founder != shown)
			{
				Notification.ShowError("EmployeeChangedNoEdit".LocDef("The employee changed. Nothing was edited."));
				return;
			}

			target.Employee.Founder = !shown;
			Notification.ShowInfo(string.Format("FounderChanged".LocDef("{0}: Founder is now {1}. Select the employee again to refresh the Fire button."), target.Employee.Name, Label(!shown)));
		}

		private static string FindBlocker(EmployeeTarget target)
		{
			if (!target.IsStillValid())
			{
				return SharedMessages.EmployeeChangedNothingEdited();
			}

			if (GameData.MultiplayerMode)
			{
				return SharedMessages.NotAvailableInMultiplayer();
			}

			return null;
		}

		private static string Label(bool founder)
		{
			return founder ? "FounderOn".LocDef("on") : "FounderOff".LocDef("off");
		}
	}
}
