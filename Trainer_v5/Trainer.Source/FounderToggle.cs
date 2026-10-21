namespace Trainer_v5
{
	// Flips Employee.Founder on the one selected employee (#190). A founder is unpaid and cannot be fired.
	internal static class FounderToggle
	{
		public static string BuildPrompt(string name, bool founder)
		{
			return $"{name}: Founder {Label(!founder)} -> {Label(founder)}";
		}

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
			Confirmation.Ask("Founder", BuildPrompt(employee.Name, !shown), () => Apply(target, shown));
		}

		// The dialog stays open while the game runs, so the target is checked again before writing.
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
			Notification.ShowInfo($"{target.Employee.Name}: Founder is now {Label(!shown)}. Select the employee again to refresh the Fire button.");
		}

		private static string FindBlocker(EmployeeTarget target)
		{
			if (!target.IsStillValid())
			{
				return SharedMessages.EmployeeChangedNothingEdited();
			}

			if (GameData.MultiplayerMode)
			{
				return "NotAvailableInMultiplayer".LocDef("Not available in multiplayer.");
			}

			return null;
		}

		private static string Label(bool founder)
		{
			return founder ? "on" : "off";
		}
	}
}
