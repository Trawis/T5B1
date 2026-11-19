using System.Linq;
using Xunit;

namespace Trainer_v5.Tests
{
	[Collection(SettingsCollection.Name)]
	public class SerializationTests
	{
		private static WriteDictionary Serialize()
		{
			return new Main().Serialize(GameReader.LoadMode.Full);
		}

		private static void Deserialize(WriteDictionary data)
		{
			new Main().Deserialize(data, GameReader.LoadMode.Full);
		}

		[Fact]
		public void Serialize_WritesEveryRegisteredToggleAndStoreKey()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();

				var data = Serialize();

				Assert.Equal(Helpers.Settings.Count + Helpers.StoresSettings.Count, data.Count);
				foreach (var key in Helpers.Settings.Keys)
				{
					Assert.False(data.Get<bool>(key, true), key);
				}
			}
		}

		[Fact]
		public void RoundTrip_RestoresToggleAndStoreValues()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				Helpers.Settings["FreePrint"] = true;
				Helpers.Settings["NoStress"] = true;
				Helpers.StoresSettings["EfficiencyStore"] = 5;
				Helpers.StoresSettings["LeadEfficiencyStore"] = 80;
				var saved = Serialize();

				SettingsTests.ResetToDefaults();
				Deserialize(saved);

				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.True(Helpers.Settings["NoStress"]);
				Assert.False(Helpers.Settings["NoVacation"]);
				Assert.Equal(5, Helpers.StoresSettings["EfficiencyStore"]);
				Assert.Equal(80, Helpers.StoresSettings["LeadEfficiencyStore"]);
			}
		}

		[Fact]
		public void Deserialize_LoadsEverySavedKeyFromASaveWrittenWithRawKeyNames()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				var save = new WriteDictionary();
				foreach (var key in SettingsTests.PersistedToggleKeys)
				{
					save[key] = true;
				}

				foreach (var key in SettingsTests.PersistedStoreKeys)
				{
					save[key] = 40;
				}

				Deserialize(save);

				Assert.All(SettingsTests.PersistedToggleKeys, key => Assert.True(Helpers.Settings[key], key));
				Assert.All(SettingsTests.PersistedStoreKeys, key => Assert.Equal(40, Helpers.StoresSettings[key]));
			}
		}

		[Fact]
		public void Deserialize_ResetsKeysMissingFromAnOlderSaveToTheirDefaults()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				Helpers.Settings["NoFounderDividends"] = true;
				Helpers.StoresSettings["EfficiencyStore"] = 10;

				var olderSave = new WriteDictionary();
				olderSave["FreePrint"] = true;
				Deserialize(olderSave);

				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.False(Helpers.Settings["NoFounderDividends"]);
				Assert.Null(Helpers.StoresSettings["EfficiencyStore"]);
			}
		}

		[Fact]
		public void Deserialize_ConsecutiveSavesDoNotShareSettings()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				var first = new WriteDictionary();
				first["NoStress"] = true;
				first["EfficiencyStore"] = 7;
				var second = new WriteDictionary();
				second["FreePrint"] = true;

				Deserialize(first);
				Assert.True(Helpers.Settings["NoStress"]);
				Assert.Equal(7, Helpers.StoresSettings["EfficiencyStore"]);

				Deserialize(second);
				Assert.False(Helpers.Settings["NoStress"]);
				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.Null(Helpers.StoresSettings["EfficiencyStore"]);
			}
		}

		[Fact]
		public void Deserialize_KeepsAnExplicitlySavedFalseOverAnEarlierTrue()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				Helpers.Settings["FreePrint"] = true;
				var save = new WriteDictionary();
				save["FreePrint"] = false;

				Deserialize(save);

				Assert.False(Helpers.Settings["FreePrint"]);
			}
		}

		[Fact]
		public void Deserialize_PartialLoadsKeepTheValuesInUse()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				Helpers.Settings["AutoPorting"] = true;
				Helpers.StoresSettings["LeadEfficiencyStore"] = 40;

				new Main().Deserialize(new WriteDictionary(), GameReader.LoadMode.Building);
				new Main().Deserialize(new WriteDictionary(), GameReader.LoadMode.Company);

				Assert.True(Helpers.Settings["AutoPorting"]);
				Assert.Equal(40, Helpers.StoresSettings["LeadEfficiencyStore"]);
			}
		}

		[Fact]
		public void Deserialize_IgnoresKeysThatAreNoLongerRegistered()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				var toggleKeys = Helpers.Settings.Keys.ToList();

				var newerSave = new WriteDictionary();
				newerSave["RemovedFeature"] = true;
				newerSave["FreePrint"] = true;
				Deserialize(newerSave);

				Assert.Equal(toggleKeys.Count, Helpers.Settings.Count);
				Assert.DoesNotContain("RemovedFeature", Helpers.Settings.Keys);
				Assert.True(Helpers.Settings["FreePrint"]);
			}
		}

		[Fact]
		public void Deserialize_OfAnEmptyFullSaveRestoresTheDefaults()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				Helpers.Settings["AutoPorting"] = true;
				Helpers.StoresSettings["LeadEfficiencyStore"] = 40;

				Deserialize(new WriteDictionary());

				Assert.All(Helpers.Settings, pair => Assert.False(pair.Value, pair.Key));
				Assert.All(Helpers.StoresSettings, pair => Assert.Null(pair.Value));
			}
		}
	}
}
