using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace Trainer_v5.Tests
{
	// A full game whose save has no trainer entry is never given to Main.Deserialize by the game.
	[Collection(SettingsCollection.Name)]
	public class SettingsSessionTests
	{
		private static void Dirty()
		{
			SettingsTests.ResetToDefaults();
			Helpers.Settings["FreeEmployees"] = true;
			Helpers.StoresSettings["EfficiencyStore"] = 80;
		}

		private static WriteDictionary ModData(WriteDictionary trainerEntry)
		{
			var modData = new WriteDictionary();
			if (trainerEntry != null)
			{
				modData[Main.ModName] = trainerEntry;
			}

			return modData;
		}

		[Fact]
		public void AGameWithoutModDataOrATrainerEntryGetsTheDeclaredDefaults()
		{
			using (new SettingsSnapshot())
			{
				Dirty();
				Main.ApplyForGame(null);
				Assert.False(Helpers.Settings["FreeEmployees"]);
				Assert.Null(Helpers.StoresSettings["EfficiencyStore"]);

				Dirty();
				Main.ApplyForGame(ModData(null));
				Assert.False(Helpers.Settings["FreeEmployees"]);
				Assert.Null(Helpers.StoresSettings["EfficiencyStore"]);
			}
		}

		[Fact]
		public void AGameLoadedWhileInactiveTakesItsOwnEntryAndTheDefaultsForMissingKeys()
		{
			using (new SettingsSnapshot())
			{
				Dirty();
				var entry = new WriteDictionary();
				entry["FreePrint"] = true;
				entry["NoStress"] = false;
				entry["LeadEfficiencyStore"] = 40;

				Main.ApplyForGame(ModData(entry));

				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.False(Helpers.Settings["NoStress"]);
				Assert.False(Helpers.Settings["FreeEmployees"]);
				Assert.Equal(40, Helpers.StoresSettings["LeadEfficiencyStore"]);
				Assert.Null(Helpers.StoresSettings["EfficiencyStore"]);
			}
		}

		[Fact]
		public void APartialLoadKeepsTheValuesInUseAndDoesNotCountAsAFullLoad()
		{
			using (new SettingsSnapshot())
			{
				SettingsSession.Forget();
				Dirty();
				var building = new WriteDictionary();
				building["FreePrint"] = true;

				new Main().Deserialize(building, GameReader.LoadMode.Building);
				new Main().Deserialize(new WriteDictionary(), GameReader.LoadMode.Company);

				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.True(Helpers.Settings["FreeEmployees"]);
				Assert.Equal(80, Helpers.StoresSettings["EfficiencyStore"]);
				Assert.True(SettingsSession.NeedsDefaults(1), "a partial load must not satisfy the first full game");
			}
		}

		[Fact]
		public void AFullLoadMarksTheNextGameAsLoadedSoItsValuesAreKept()
		{
			using (new SettingsSnapshot())
			{
				SettingsSession.Forget();
				var save = new WriteDictionary();
				save["FreePrint"] = true;

				new Main().Deserialize(save, GameReader.LoadMode.Full);

				Assert.False(SettingsSession.NeedsDefaults(1));
				Assert.True(Helpers.Settings["FreePrint"]);
			}
		}

		[Fact]
		public void ConsecutiveGamesEachNeedDefaultsUnlessTheyWereLoaded()
		{
			SettingsSession.Forget();

			Assert.True(SettingsSession.NeedsDefaults(1), "first game, nothing loaded");
			Assert.False(SettingsSession.NeedsDefaults(1), "same game again (ready, then reactivation)");

			SettingsSession.FullLoadApplied();
			Assert.False(SettingsSession.NeedsDefaults(2), "second game loaded its data");
			Assert.False(SettingsSession.NeedsDefaults(2));

			Assert.True(SettingsSession.NeedsDefaults(3), "third game has no trainer data");
			Assert.False(SettingsSession.NeedsDefaults(3));
			Assert.True(SettingsSession.NeedsDefaults(4));
		}

		[Fact]
		public void ReactivatingInTheSameGameKeepsTheUnsavedChoices()
		{
			using (new SettingsSnapshot())
			{
				SettingsSession.Forget();
				Assert.True(SettingsSession.NeedsDefaults(7));
				Dirty();

				SettingsSession.Deactivated();
				var again = SettingsSession.NeedsDefaults(7);

				Assert.False(again);
				Assert.True(Helpers.Settings["FreeEmployees"]);
			}
		}

	}
}
