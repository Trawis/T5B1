namespace Trainer_v5
{
	// Creativity has no setter, so the edit installs a new Employee; the migration re-points the game's references (#183).
	internal static class CreativityEditGuard
	{
		public static bool IsEnabled()
		{
			return Helpers.GetProperty(Helpers.Settings, SettingKeys.Experimental) || Helpers.IsDebug;
		}

		// A reason the employee must not be replaced, or null when the migration covers it.
		public static string FindBlockingReference(Employee employee)
		{
			// Founders are held by the company's share holders and by the owner flow; that is not migrated.
			if (employee.Founder)
			{
				return "is a founder, whose ownership records are not migrated";
			}

			// Without an actor there is nothing to install the new employee on.
			if (employee.MyActor == null)
			{
				return "has no actor in the office";
			}

			// Employees are synchronised by network id between players; a replacement is not tested for that.
			if (SINetworking.NetworkManager.IsConnected)
			{
				return "is in a multiplayer game";
			}

			return null;
		}
	}
}
