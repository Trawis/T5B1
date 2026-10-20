using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Global opt-in in the mod's own game settings (DLLMod.Settings); read at load, never part of a save.
	internal static class LoggingOptions
	{
		public const string StorageKey = "DebugLogging";

		private static bool _debug;

		public static bool DebugEnabled => _debug;

		public static void Load(Dictionary<string, string> modSettings)
		{
			string stored;
			_debug = modSettings != null && modSettings.TryGetValue(StorageKey, out stored) && Parse(stored);
		}

		public static bool Parse(string stored)
		{
			bool value;
			return bool.TryParse(stored, out value) && value;
		}

		public static void Set(bool enabled, Action<string, string> save)
		{
			_debug = enabled;
			if (save != null)
			{
				save(StorageKey, enabled.ToString());
			}
		}
	}
}
