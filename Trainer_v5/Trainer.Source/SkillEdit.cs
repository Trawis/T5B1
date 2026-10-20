namespace Trainer_v5
{
	internal enum SkillTargets
	{
		None = 0,
		// The employee the editor shows (scope "This employee").
		Viewed = 1,
		// Every live selected employee (scope "All selected").
		Selection = 2
	}

	internal enum SkillOutcome
	{
		// No target was still valid at the write.
		None = 0,
		Partial = 1,
		All = 2
	}

	// Rules of the skill editor that need no game objects.
	internal static class SkillEdit
	{
		public const int MinStars = -3;
		public const int MaxStars = 3;

		// A zero change is rejected, as before; the game clamps each specialization to 0..3 stars.
		public static bool IsValidStars(int stars)
		{
			return stars != 0 && stars >= MinStars && stars <= MaxStars;
		}

		public static bool TryParseStars(string input, out int stars)
		{
			return InputHelper.ParseInt(input ?? string.Empty, out stars) && IsValidStars(stars);
		}

		public static SkillTargets Decide(bool allSelected, bool viewedIsValid, int liveSelectedCount)
		{
			if (allSelected)
			{
				return liveSelectedCount > 0 ? SkillTargets.Selection : SkillTargets.None;
			}

			return viewedIsValid ? SkillTargets.Viewed : SkillTargets.None;
		}

		public static SkillOutcome Outcome(int applied, int total)
		{
			if (applied <= 0 || total <= 0)
			{
				return SkillOutcome.None;
			}

			return applied >= total ? SkillOutcome.All : SkillOutcome.Partial;
		}
	}
}
