using System;
using System.Collections.Generic;

namespace Trainer_v5.Experimental
{
	// Stored in the mod's game-managed settings (DLLMods/<mod>Setting.txt), read once at mod load and never saved in a game.
	internal static class ExperimentalOptions
	{
		public const string StorageKey = "EnableExperimentalFeatures";

		private static bool _enabled;

		public static bool Enabled => _enabled;

		public static void Load(Dictionary<string, string> modSettings)
		{
			string stored;
			_enabled = modSettings != null && modSettings.TryGetValue(StorageKey, out stored) && Parse(stored);
		}

		public static bool Parse(string stored)
		{
			bool value;
			return bool.TryParse(stored, out value) && value;
		}

		public static void Set(bool enabled, Action<string, string> save)
		{
			_enabled = enabled;
			if (save != null)
			{
				save(StorageKey, enabled.ToString());
			}
		}
	}
}
