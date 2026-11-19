namespace Trainer_v5
{
	// Compares the running game's Versioning.CurrentVersion (a runtime struct, so DLL and Workshop builds read the
	// same value) with the vendored target. Diagnostic only: a mismatch is logged and never blocks anything.
	internal static class GameVersionCheck
	{
		public const int TargetTypeInt = 2;
		public const int TargetMajor = 1;
		public const int TargetMinor = 8;
		public const int TargetRevision = 42;
		public const string TargetText = "Beta 1.8.42";

		public static bool IsTarget(int typeInt, int major, int minor, int revision)
		{
			return typeInt == TargetTypeInt && major == TargetMajor && minor == TargetMinor && revision == TargetRevision;
		}

		public static string Describe(string running, bool isTarget)
		{
			if (isTarget)
				return "Game version " + running + " matches the version this trainer was built for.";

			return "Game version " + running + " differs from the version this trainer was built for (" + TargetText
				+ "). The trainer keeps running; some features may misbehave until it is updated.";
		}

		public static string Running()
		{
			return Versioning.CurrentVersion.SimpleVersion();
		}

		public static bool RunningIsTarget()
		{
			Versioning.Version running = Versioning.CurrentVersion;
			return IsTarget(running.TypeInt, running.Major, running.Minor, running.Revision);
		}

		public static void LogRunningVersion()
		{
			Describe(Running(), RunningIsTarget()).Log();
		}
	}
}
