using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	// The only settings accessors. Feature and UI code passes a ToggleKey or StoreKey; the string overloads exist for
	// save serialization (Main), which iterates the raw save keys.
	public static class Extensions
	{
		private static readonly HashSet<string> _loggedUnregisteredKeys;

		// The game's mod loader mis-parses calls in member initializers, so it is assigned here.
		static Extensions()
		{
			_loggedUnregisteredKeys = new HashSet<string>();
		}

		public static bool Get(this Dictionary<string, bool> settings, ToggleKey key) => settings.Get(key.Key);

		public static void Set(this Dictionary<string, bool> settings, ToggleKey key, bool value) => settings.Set(key.Key, value);

		public static void Toggle(this Dictionary<string, bool> settings, ToggleKey key) => settings.Set(key.Key, !settings.Get(key.Key));

		public static object Get(this Dictionary<string, object> settings, StoreKey key) => settings.Get(key.Key);

		public static void Set(this Dictionary<string, object> settings, StoreKey key, object value) => settings.Set(key.Key, value);

		// The position of the stored value among the choices; the first choice when nothing is stored.
		public static int GetIndex(this Dictionary<string, object> items, Dictionary<string, object> settings, StoreKey key)
		{
			var stored = settings.Get(key);
			return stored == null ? 0 : items.FindIndex(x => x.Value.MakeFloat() == stored.MakeFloat());
		}

		public static bool Get(this Dictionary<string, bool> settings, string key)
		{
			bool value;
			if (settings.TryGetValue(key, out value))
			{
				return value;
			}

			LogUnregisteredKey("Get", key);
			return false;
		}

		public static object Get(this Dictionary<string, object> settings, string key)
		{
			object value;
			if (settings.TryGetValue(key, out value))
			{
				return value;
			}

			LogUnregisteredKey("Get", key);
			return null;
		}

		public static void Set(this Dictionary<string, bool> settings, string key, bool value)
		{
			if (!settings.ContainsKey(key))
			{
				LogUnregisteredKey("Set", key);
				return;
			}

			settings[key] = value;
		}

		public static void Set(this Dictionary<string, object> settings, string key, object value)
		{
			if (!settings.ContainsKey(key))
			{
				LogUnregisteredKey("Set", key);
				return;
			}

			settings[key] = value;
		}

		// A key missing here was never registered (a typo); a registered key absent from an old save never reaches these accessors.
		private static void LogUnregisteredKey(string source, string key)
		{
			string message = string.Format("{0}: setting key '{1}' is not registered - check for a typo in the key name. Falling back to the default value.", source, key);
			if (_loggedUnregisteredKeys.Add(message))
			{
				message.Log();
			}
		}
	}
}
