namespace Trainer_v5.Experimental
{
	// Actor.Fire skips the coworker TeammateFired mood when QuitAmicably is set; it also skips NoteFiring (called here
	// instead) and the Negligence lawsuit check, so the action is refused unless that lawsuit is impossible.
	internal static class QuietFire
	{
		private const string ActionName = "Fire Quietly";

		internal static void Run()
		{
			if (!ExperimentalOptions.Enabled || !Helpers.RequireGameReady(ActionName))
				return;

			var target = EmployeeTarget.RequireOne();
			if (target == null)
				return;

			var blocked = FindBlocker(target);
			if (blocked != null)
			{
				Notification.ShowError(blocked);
				return;
			}

			Confirmation.Ask(ActionName, string.Format("QuietFireConfirm".LocDef("Fire {0} without upsetting their teammates?\nThis is a real firing: severance and the usual hiring reputation effect apply, and the employee leaves for good. Only the mood penalty that teammates get for a fired coworker is skipped.\nThis cannot be undone."), target.Employee.Name), () => Apply(target));
		}

		// The dialog stays open while the game runs, so the same employee is checked again before firing.
		private static void Apply(EmployeeTarget target)
		{
			var blocked = FindBlocker(target);
			if (blocked != null)
			{
				Notification.ShowError(blocked);
				return;
			}

			GameSettings game = GameSettings.Instance;
			game.ApplicantScore.NoteFiring(target.Employee);
			target.Actor.QuitAmicably = true;
			target.Actor.Fire(false, false);
			game.RegisterStat("Fired", 1f);
			Notification.ShowInfo(string.Format("QuietFireDone".LocDef("{0} was fired. Their teammates were not upset."), target.Employee.Name));
		}

		// Mirrors the game's Negligence lawsuit condition, which needs more than 12 months employed and a complaint level above 1.
		internal static bool LawsuitImpossible(float monthsEmployed, float complaintLevel)
		{
			return !(monthsEmployed > 12f) || !(complaintLevel > 1f);
		}

		private static string FindBlocker(EmployeeTarget target)
		{
			if (!ExperimentalOptions.Enabled || !target.IsStillValid())
				return SharedMessages.EmployeeChangedNothingEdited();

			var employee = target.Employee;
			if (target.Actor.AItype != AI.AIType.Employee || employee.Dismissed || employee.MyEmployer != GameSettings.Instance.MyCompany)
				return "QuietFireNotEmployee".LocDef("Only your own current employees can be fired. Nothing was changed.");

			if (employee.Founder)
				return "QuietFireFounder".LocDef("A founder cannot be fired. Nothing was changed.");

			if (!LawsuitImpossible(SDateTime.GetMonths(employee.Hired, SDateTime.Now()), target.Actor.ComplaintLevel))
				return "QuietFireMaySue".LocDef("This employee has complaints and could sue for negligence. Use the game's own Fire button. Nothing was changed.");

			return null;
		}
	}
}
