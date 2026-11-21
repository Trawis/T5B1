using System;

namespace Trainer_v5.Experimental
{
	// The game's own skip commands advance hour by hour through TimeOfDay.AddHour, so payroll, deadlines and autosaves run.
	internal static class SkipTime
	{
		private const string ActionName = "Skip Time";

		internal static void SkipHour()
		{
			if (CanSkip())
				Skip(() => Example.SkipHour(1));
		}

		internal static void SkipDay()
		{
			if (CanSkip())
				Confirm(24, null, () => Example.SkipDay(1));
		}

		internal static void SkipToHour()
		{
			if (!CanSkip())
				return;

			InputHelper.RequestInt("SkipToHourPrompt".LocDef("Skip to which hour of the day? (0-23)"), "SkipToHour".LocDef("Skip To Hour"), string.Empty, target =>
			{
				int hours = HoursUntil(TimeOfDay.Instance.Hour, target);
				if (hours == 0)
				{
					Notification.ShowInfo("SkipToHourAlready".LocDef("It is already that hour. Nothing was changed."));
					return;
				}

				Confirm(hours, () => Confirmation.ChangedUnless(TimeOfDay.Instance.Hour != target), () => Example.SkipTo(target));
			}, 0, 23);
		}

		internal static int HoursUntil(int currentHour, int targetHour)
		{
			return ((targetHour - currentHour) % 24 + 24) % 24;
		}

		private static bool CanSkip()
		{
			if (!ExperimentalOptions.Enabled || !Helpers.RequireGameReady(ActionName))
				return false;

			string blocked = FindBlocker();
			if (blocked != null)
			{
				Notification.ShowError(blocked);
				return false;
			}

			return true;
		}

		private static string FindBlocker()
		{
			if (!ExperimentalOptions.Enabled)
				return Confirmation.ScopeChangedText();

			return GameSettings.ForcePause ? "SkipTimeForcePaused".LocDef("The game is holding time (a dialog or event is waiting). Nothing was changed.") : null;
		}

		// findChange is null when only the game itself has to be checked again.
		private static void Confirm(int hours, Func<string> findChange, Action skip)
		{
			Confirmation.AskAndApply(ActionName, string.Format("SkipTimeConfirm".LocDef("Skip {0} hour(s) of game time?\nSalaries, bills, deadlines, events and autosaves happen as usual, but employees in the office do no work during the skipped hours and stay where they are.\nThis cannot be undone."), hours), () => FindBlocker() ?? (findChange != null ? findChange() : null), () =>
			{
				Skip(skip);
				return null;
			});
		}

		// Shows its own result, an error when the game did not advance.
		private static void Skip(Action skip)
		{
			SDateTime before = SDateTime.Now();
			skip();
			SDateTime after = SDateTime.Now();
			if (after.ToInt() == before.ToInt())
			{
				Notification.ShowError("SkipTimeNotAdvanced".LocDef("The game did not advance time. Nothing was changed."));
				return;
			}

			Notification.Popup(string.Format("SkipTimeDone".LocDef("Trainer: time advanced to {0}."), after.ToCompactString()));
		}
	}
}
