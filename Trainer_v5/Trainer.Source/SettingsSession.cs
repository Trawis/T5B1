namespace Trainer_v5
{
	// GameSettings is created per loaded game, so its instance id tells one full-game session from the next. The game
	// calls Main.Deserialize only when the save has a trainer entry (and the mod is active), which is why a game without
	// one must be detected here instead.
	internal static class SettingsSession
	{
		private static int _gameId;
		private static bool _fullLoadApplied;

		// A full load handed the current game its saved values.
		public static void FullLoadApplied()
		{
			_fullLoadApplied = true;
		}

		// A new scene starts a new game; anything remembered about the last one no longer applies.
		public static void BeginGame()
		{
			_fullLoadApplied = false;
		}

		public static void Deactivated()
		{
			_fullLoadApplied = false;
		}

		// True once for a game the settings were not loaded for: its values must come from its own ModData or defaults.
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

		// For tests only: forgets every game.
		public static void Forget()
		{
			_gameId = 0;
			_fullLoadApplied = false;
		}
	}
}
