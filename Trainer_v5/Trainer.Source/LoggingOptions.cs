using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// The opt-in for extra diagnostic lines (#278). Off by default; independent of Enable Experimental Features. Like
	// ExperimentalOptions it lives in the mod's own game-managed settings (ModController.DLLMod.Settings, saved by the
	// game to DLLMods/<mod>Setting.txt): global, read once when the mod loads, never part of a save. Normal logging
	// (feature failures and skipped-action warnings) does not look at it. Every call site of Logger.LogDebug is guarded
	// by DebugEnabled so nothing is built while it is off.
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

		// Anything that is not the word True (missing, empty, damaged) stays off.
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
