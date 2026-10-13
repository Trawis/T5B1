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
		public void EveryDeclaredKeyIsNamedAfterItsSavedKey()
		{
			var mismatched = new List<string>();
			foreach (var field in DeclaredKeys())
			{
				var key = field.FieldType == typeof(SettingKey<bool>)
					? ((SettingKey<bool>)field.GetValue(null)).Key
					: ((SettingKey<object>)field.GetValue(null)).Key;
				if (field.Name != key)
				{
					mismatched.Add(field.Name + " saves as " + key);
				}
			}

			Assert.Empty(mismatched);
		}

		[Fact]
		public void DeclaredKeysAreExactlyTheKeysInTheSettingsDictionaries()
		{
			var declaredToggles = new List<string>();
			var declaredStores = new List<string>();
			foreach (var field in DeclaredKeys())
			{
				if (field.FieldType == typeof(SettingKey<bool>))
				{
					declaredToggles.Add(((SettingKey<bool>)field.GetValue(null)).Key);
				}
				else
				{
					declaredStores.Add(((SettingKey<object>)field.GetValue(null)).Key);
				}
			}

			Assert.Equal(declaredToggles.Count, Helpers.Settings.Count);
			Assert.All(declaredToggles, key => Assert.True(Helpers.Settings.ContainsKey(key), key));
			Assert.Equal(declaredStores.Count, Helpers.StoresSettings.Count);
			Assert.All(declaredStores, key => Assert.True(Helpers.StoresSettings.ContainsKey(key), key));
		}

		[Fact]
		public void DefaultsComeFromTheDeclaredKeys()
		{
			var toggleDefaults = SettingKeys.CreateToggleDefaults();
			var storeDefaults = SettingKeys.CreateStoreDefaults();

			Assert.False(toggleDefaults[SettingKeys.FreePrint.Key]);
			Assert.Equal(SettingKeys.NoStress.DefaultValue, toggleDefaults[SettingKeys.NoStress.Key]);
			Assert.Null(storeDefaults[SettingKeys.EfficiencyStore.Key]);
			Assert.Equal(SettingKeys.LeadEfficiencyStore.DefaultValue, storeDefaults[SettingKeys.LeadEfficiencyStore.Key]);
			Assert.Equal(Helpers.Settings.Count, toggleDefaults.Count);
			Assert.Equal(Helpers.StoresSettings.Count, storeDefaults.Count);
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

		[Fact]
		public void SettingKeyToStringIsTheSavedKey()
		{
			Assert.Equal("NoStress", SettingKeys.NoStress.ToString());
		}
	}
}
