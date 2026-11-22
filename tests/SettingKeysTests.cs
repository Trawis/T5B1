using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	[Collection(SettingsCollection.Name)]
	public class SettingKeysTests
	{
		private static List<FieldInfo> DeclaredKeys()
		{
			var keys = new List<FieldInfo>();
			foreach (var field in typeof(SettingKeys).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				keys.Add(field);
			}
			return keys;
		}

		[Fact]
		public void EveryDeclaredKeyIsNamedAfterItsSavedKeyAndIsALiveSettingUnlessRetired()
		{
			var mismatched = new List<string>();
			foreach (var field in DeclaredKeys())
			{
				var isToggle = field.FieldType == typeof(ToggleKey);
				var key = isToggle
					? ((ToggleKey)field.GetValue(null)).Key
					: ((StoreKey)field.GetValue(null)).Key;
				if (field.Name != key)
				{
					mismatched.Add(field.Name + " saves as " + key);
				}

				var live = isToggle ? Helpers.Settings.ContainsKey(key) : Helpers.StoresSettings.ContainsKey(key);
				if (live == (System.Array.IndexOf(RetiredSettingsTests.RetiredKeys, key) >= 0))
				{
					mismatched.Add(key + (live ? " is retired but live" : " is declared but not a live setting"));
				}
			}

			Assert.Empty(mismatched);
		}

		[Fact]
		public void TypedAccessorsReadAndWriteTheSameValuesAsTheStringKeys()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();

				Helpers.Settings.Toggle(SettingKeys.LockAge);
				Assert.True(Helpers.Settings["LockAge"]);
				Assert.True(Helpers.Settings.Get(SettingKeys.LockAge));
				Assert.True(Helpers.GetProperty(Helpers.Settings, SettingKeys.LockAge));

				Helpers.StoresSettings.Set(SettingKeys.EfficiencyStore, 20);
				Assert.Equal(20, Helpers.StoresSettings["EfficiencyStore"]);
				Assert.Equal(20, Helpers.StoresSettings.Get(SettingKeys.EfficiencyStore));
				Assert.Equal(20, Helpers.GetProperty(Helpers.StoresSettings, SettingKeys.EfficiencyStore));
			}
		}
	}
}
