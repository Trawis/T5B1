using System;
using System.Collections.Generic;

namespace Trainer_v5.Experimental
{
	// The one opt-in for unfinished trainer work (#245). Code in this folder and namespace is experimental: it is not
	// registered in FeatureRegistry or SettingToggleRows, has no Trainer Settings control, and runs only when Enabled.
	// The value lives in the mod's own game-managed settings (ModController.DLLMod.Settings, saved by the game to
	// DLLMods/<mod>Setting.txt), which is global: it is read once when the mod loads, before any game, and is never part
	// of a save. The legacy SettingKeys.Experimental save key is ignored on purpose, so an old save cannot switch it on.
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

		// Anything that is not the word True (missing, empty, damaged) stays off.
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
