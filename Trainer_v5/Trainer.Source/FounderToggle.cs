namespace Trainer_v5
{
	// Flips Employee.Founder on the shown employee (#190). A founder is unpaid and cannot be fired.
	internal static class FounderToggle
	{
		public static string BuildPrompt(string name, bool founder)
		{
			return $"{name}: Founder {Label(!founder)} -> {Label(founder)}\nType YES to confirm.";
		}

		public static void Start()
		{
			TargetSource source;
			var target = EmployeeTarget.Resolve(out source);
			if (source != TargetSource.DetailWindow)
				return;

			var blocked = FindBlocker(target);
			if (blocked != null)
			{
				Notification.ShowError(blocked);
				return;
			}

			var employee = target.Employee;
			var shown = employee.Founder;
			WindowManager.SpawnInputDialog(BuildPrompt(employee.Name, !shown), "ToggleFounderTitle".LocDef("Toggle Founder"), "YES", answer => Apply(target, shown, answer));
		}

		// The dialog stays open while the game runs, so the target is checked again before writing.
		private static void Apply(EmployeeTarget target, bool shown, string answer)
		{
			if (answer != "YES")
				return;

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
				return "EmployeeChangedNothingEdited".LocDef("The employee changed or is not available. Nothing was edited.");
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
