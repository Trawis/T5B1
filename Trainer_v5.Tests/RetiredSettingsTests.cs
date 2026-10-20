using System.Linq;
using Xunit;

namespace Trainer_v5.Tests
{
	// Retired toggles keep their declared names but must stay unavailable, inert and unpersisted (#270, #439).
	[Collection(SettingsCollection.Name)]
	public class RetiredSettingsTests
	{
		internal static readonly string[] RetiredKeys = { "ReduceBoxPrice", "AutoMaxMarketShare", "MoreCreativity", "ReduceExpansionCost", "Experimental", "DisableSkillDecay" };

		[Fact]
		public void RetiredKeysAreStillDeclaredWithTheirOriginalNamesAndDefaultOffButAreNotLiveSettings()
		{
			Assert.Equal("ReduceBoxPrice", SettingKeys.ReduceBoxPrice.Key);
			Assert.Equal("AutoMaxMarketShare", SettingKeys.AutoMaxMarketShare.Key);
			Assert.Equal("MoreCreativity", SettingKeys.MoreCreativity.Key);
			Assert.Equal("ReduceExpansionCost", SettingKeys.ReduceExpansionCost.Key);
			Assert.Equal("Experimental", SettingKeys.Experimental.Key);
			Assert.Equal("DisableSkillDecay", SettingKeys.DisableSkillDecay.Key);
			foreach (var key in RetiredKeys)
			{
				Assert.False(((ToggleKey)typeof(SettingKeys).GetField(key).GetValue(null)).DefaultValue, key);
				Assert.False(Helpers.Settings.ContainsKey(key), key);
			}
		}

		[Fact]
		public void RetiredKeysAreNotInTheToggleDefaults()
		{
			var defaults = SettingKeys.CreateToggleDefaults();

			Assert.All(RetiredKeys, key => Assert.DoesNotContain(key, defaults.Keys));
		}

		[Fact]
		public void RetiredKeyNamesStayReservedAgainstReuse()
		{
			foreach (var key in RetiredKeys)
			{
				var field = typeof(SettingKeys).GetField(key);
				Assert.NotNull(field);
				Assert.Equal(key, ((ToggleKey)field.GetValue(null)).Key);
			}
		}

		[Fact]
		public void NewSavesDoNotContainRetiredKeys()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				var data = new Main().Serialize(GameReader.LoadMode.Full);

				Assert.All(RetiredKeys, key => Assert.False(data.Contains(key), key));
				Assert.Equal(SettingsTests.PersistedToggleKeys.Length + SettingsTests.PersistedStoreKeys.Length, data.Count);
			}
		}

		[Fact]
		public void OldSavesWithRetiredKeysEnabledStillLoadAndActivateNothing()
		{
			using (new SettingsSnapshot())
			{
				SettingsTests.ResetToDefaults();
				var oldSave = new WriteDictionary();
				foreach (var key in RetiredKeys)
				{
					oldSave[key] = true;
				}

				oldSave["FreePrint"] = true;
				oldSave["EfficiencyStore"] = 9;

				new Main().Deserialize(oldSave, GameReader.LoadMode.Full);
				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.Equal(9, Helpers.StoresSettings["EfficiencyStore"]);
				Assert.All(RetiredKeys, key => Assert.DoesNotContain(key, Helpers.Settings.Keys));

				var rewritten = new Main().Serialize(GameReader.LoadMode.Full);
				Assert.All(RetiredKeys, key => Assert.False(rewritten.Contains(key), key));
				Assert.True(rewritten.Get<bool>("FreePrint", false));

				// Partial loads of the same data behave the same.
				new Main().Deserialize(oldSave, GameReader.LoadMode.Building);
				new Main().Deserialize(oldSave, GameReader.LoadMode.Company);
				Assert.All(RetiredKeys, key => Assert.DoesNotContain(key, Helpers.Settings.Keys));
				Assert.True(Helpers.Settings["FreePrint"]);
			}
		}

		[Fact]
		public void RetiredKeysHaveNoScheduledBehavior()
		{
			var scheduled = Enumerable.ToList(Enumerable.Select(Enumerable.Where(FeatureRegistry.Create(), entry => entry.Toggle != null), entry => entry.Toggle.Key));

			Assert.DoesNotContain(scheduled, key => System.Array.IndexOf(RetiredKeys, key) >= 0);
		}

		[Fact]
		public void RetiredKeysHaveNoSettingsWindowToggle()
		{
			var shown = Enumerable.ToList(Enumerable.Select(SettingToggleRows.Create(), row => row.Key.Key));

			Assert.DoesNotContain(shown, key => System.Array.IndexOf(RetiredKeys, key) >= 0);
		}
	}
}
