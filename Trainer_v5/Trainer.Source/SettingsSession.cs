namespace Trainer_v5
{
	internal static class SettingsSession
	{
		private static int _gameId;
		private static bool _fullLoadApplied;

		public static void FullLoadApplied()
		{
			_fullLoadApplied = true;
		}

		public static void BeginGame()
		{
			_fullLoadApplied = false;
		}

		public static void Deactivated()
		{
			_fullLoadApplied = false;
		}

		public static bool NeedsDefaults(int gameId)
		{
			bool applied = _fullLoadApplied;
			_fullLoadApplied = false;

			if (gameId == 0 || gameId == _gameId)
			{
				return false;
			}

			_gameId = gameId;
			return !applied;
		}

		public static void Forget()
		{
			_gameId = 0;
			_fullLoadApplied = false;
		}
	}
}
