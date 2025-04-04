﻿﻿﻿﻿﻿﻿﻿using System.Collections.Generic;
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

		public static int GetIndex(this Dictionary<string, object> items, Dictionary<string, object> settings, string key, ValueDataTypeEnum valueType)
		{
			try
			{
				object settingValue = settings.Get(key); // Use the existing Get extension method
				if (settingValue == null)
				{
					$"Setting '{key}' not found or is null in GetIndex extension.".Log();
					return -1;
				}

				var itemList = new List<KeyValuePair<string, object>>(items);

				switch (valueType)
				{
					case ValueDataTypeEnum.Int:
						int intValue = settingValue.MakeInt();
						return itemList.FindIndex(x => x.Value != null && x.Value.MakeInt() == intValue);
					case ValueDataTypeEnum.Float:
						float floatValue = settingValue.MakeFloat();
						return itemList.FindIndex(x => x.Value != null && x.Value.MakeFloat() == floatValue);
					case ValueDataTypeEnum.String:
						string stringValue = settingValue.MakeString();
						return itemList.FindIndex(x => x.Value != null && x.Value.MakeString() == stringValue);
					case ValueDataTypeEnum.Bool:
						bool boolValue = settingValue.MakeBool();
						return itemList.FindIndex(x => x.Value != null && x.Value.MakeBool() == boolValue);
					default:
						$"Extension method GetIndex received an unknown value type: {valueType}".Log();
						return -1;
				}
			}
			catch (System.Exception ex)
			{
				$"Error in GetIndex extension for key '{key}' and type '{valueType}'".Log(false);
				ex.LogException();
				return -1;
			}
		}

	}
}
