using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	public static class Extensions
	{
		public static object Get(this Dictionary<string, object> settings, string key)
		{
			object value;
			if (settings.TryGetValue(key, out value))
			{
				return value;
			}
			return null;
		}

		public static bool Get(this Dictionary<string, bool> settings, string key)
		{
			bool value;
			if (settings.TryGetValue(key, out value))
			{
				return value;
			}
			return false;
		}

		public static void Set(this Dictionary<string, object> settings, string key, object value)
		{
			settings[key] = value;
		}

		public static void Toggle(this Dictionary<string, bool> settings, string key)
		{
			bool value;
			if (settings.TryGetValue(key, out value))
			{
				settings[key] = !value;
			}
		}

		public static int GetIndex(this Dictionary<string, object> items, Dictionary<string, object> settings, string key, int valueType)
		{
			var stored = settings.Get(key);
			switch (valueType)
			{
				case 1:
					return stored == null ? 0 : items.FindIndex(x => x.Value.MakeInt() == stored.MakeInt());
				case 2:
					return stored == null ? 0 : items.FindIndex(x => x.Value.MakeFloat() == stored.MakeFloat());
				case 3:
					return stored == null ? 0 : items.FindIndex(x => x.Value.MakeString() == stored.MakeString());
				case 4:
					return stored == null ? 0 : items.FindIndex(x => x.Value.MakeBool() == stored.MakeBool());
				default:
					"Method GetIndex received an unknown value type as parameter".Log();
					return -1;
			}
		}

	}
}
