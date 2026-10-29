using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace Trainer_v5.Tests
{
	// #493: a full game whose save has no trainer entry is never given to Main.Deserialize by the game.
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
		public void ModNameIsTheNameTheGameFilesTheEntryUnder()
		{
			Assert.Equal("TrainerByTrawis", Main.ModName);
			Assert.Equal(Main.ModName, new Main().Name);
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
		public void APresentButEmptyEntryAlsoGetsTheDefaults()
		{
			using (new SettingsSnapshot())
			{
				Dirty();
				Main.ApplyForGame(ModData(new WriteDictionary()));

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
		public void TheSettingDictionariesKeepTheirIdentityAndKeys()
		{
			using (new SettingsSnapshot())
			{
				var toggles = Helpers.Settings;
				var stores = Helpers.StoresSettings;
				var toggleKeys = new List<string>(toggles.Keys);
				var storeKeys = new List<string>(stores.Keys);

				Main.ApplyForGame(null);

				Assert.Same(toggles, Helpers.Settings);
				Assert.Same(stores, Helpers.StoresSettings);
				Assert.Equal(toggleKeys, new List<string>(Helpers.Settings.Keys));
				Assert.Equal(storeKeys, new List<string>(Helpers.StoresSettings.Keys));
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
		public void NoGameMeansNothingToEstablish()
		{
			SettingsSession.Forget();

			Assert.False(SettingsSession.NeedsDefaults(0));
			Assert.True(SettingsSession.NeedsDefaults(1));
		}

		[Fact]
		public void ALoadFlagDoesNotOutliveItsGame()
		{
			SettingsSession.Forget();

			SettingsSession.FullLoadApplied();
			SettingsSession.BeginGame();
			Assert.True(SettingsSession.NeedsDefaults(1), "a new scene discards the previous game's flag");

			SettingsSession.FullLoadApplied();
			Assert.False(SettingsSession.NeedsDefaults(1), "the ready check consumes it for the same game");
			Assert.True(SettingsSession.NeedsDefaults(2), "so it cannot satisfy the next game");

			SettingsSession.FullLoadApplied();
			SettingsSession.Deactivated();
			Assert.True(SettingsSession.NeedsDefaults(3), "deactivation drops an unobserved flag");
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

		// The game's wrapper: Meta.Deserialize runs only for an active mod whose entry exists in the incoming ModData.
		// The wrapper needs the game's Utilities class, which cannot load on every test host; then its IL is pinned instead.
		private static int RunWrapper(WriteDictionary modData, bool active)
		{
			var mod = (ModController.DLLMod)FormatterServices.GetUninitializedObject(typeof(ModController.DLLMod));
			var settings = new Dictionary<string, string>();
			settings["Active"] = active ? "True" : "False";
			typeof(ModController.DLLMod).GetField("Settings").SetValue(mod, settings);
			typeof(ModController.DLLMod).GetField("Meta").SetValue(mod, new Main());
			Helpers.Settings["FreeEmployees"] = true;

			try
			{
				mod.Deserialize(modData, GameReader.LoadMode.Full);
			}
			catch (TypeInitializationException)
			{
				return -1;
			}

			return Helpers.Settings["FreeEmployees"] ? 0 : 1;
		}

		[Fact]
		public void TheGamesWrapperSkipsTheTrainerWhenInactiveOrTheEntryIsMissing()
		{
			using (new SettingsSnapshot())
			{
				var withEntry = ModData(new WriteDictionary());
				var inactive = RunWrapper(withEntry, false);
				if (inactive < 0)
				{
					AssertWrapperIlSkipsBothCases();
					return;
				}

				Assert.Equal(0, inactive);
				Assert.Equal(0, RunWrapper(new WriteDictionary(), true));
				Assert.Equal(0, RunWrapper(ModData(null), true));
				Assert.Equal(1, RunWrapper(withEntry, true));
			}
		}

		// ldarg0 call get_Active brfalse.s ... ldloc0 brfalse.s ... callvirt ModMeta.Deserialize ret (assembly 1.8.42).
		private static void AssertWrapperIlSkipsBothCases()
		{
			var il = typeof(ModController.DLLMod).GetMethod("Deserialize").GetMethodBody().GetILAsByteArray();

			Assert.Equal(44, il.Length);
			Assert.Equal(0x2C, il[0x06]);
			Assert.Equal(0x2C, il[0x1C]);
			Assert.Equal(0x6F, il[0x26]);
			Assert.Equal(0x2A, il[0x2B]);
		}

		[Fact]
		public void TheBehaviourEstablishesSettingsAtReadyAndOnActivationAndResetsTheFlagWithTheScene()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");

			Assert.Contains("Helpers.TryExecute(\"EstablishSettings\", EstablishSettingsForGame);", SourceCode.Body(source, "private void OnGameReady("));
			Assert.Contains("Helpers.TryExecute(\"EstablishSettings\", EstablishSettingsForGame);", SourceCode.Body(source, "public override void OnActivate("));
			Assert.Contains("SettingsSession.BeginGame();", SourceCode.Body(source, "private void OnLevelFinishedLoading("));
			Assert.Contains("SettingsSession.Deactivated();", SourceCode.Body(source, "public override void OnDeactivate("));
			var establish = SourceCode.Body(source, "private void EstablishSettingsForGame(");
			Assert.Contains("SettingsSession.NeedsDefaults(game.GetInstanceID())", establish);
			Assert.Contains("Main.ApplyForGame(game.ModData);", establish);
		}

		[Fact]
		public void OnlyFullLoadsCountAsLoadedAndReadyDoesNotApplyDefaultsUnconditionally()
		{
			var main = SourceCode.Read("Main.cs");
			var deserialize = SourceCode.Body(main, "public override void Deserialize(");

			SourceCode.AssertInOrder(deserialize, "ApplySaved(data, isFullLoad);", "SettingsSession.FullLoadApplied();", "values first");
			SourceCode.AssertInOrder(deserialize, "if (isFullLoad)", "SettingsSession.FullLoadApplied();", "full loads only");
			Assert.DoesNotContain("ApplyForGame", SourceCode.Body(SourceCode.Read("TrainerBehaviour.cs"), "private void OnGameReady("));
		}
	}
}
