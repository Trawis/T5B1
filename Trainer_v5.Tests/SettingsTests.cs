using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Trainer_v5.Tests
{
	[Collection(SettingsCollection.Name)]
	public class SettingsTests
	{
		// Every key below is persisted in player saves. Renaming or removing one silently resets
		// that toggle for existing players, so a change here must be deliberate and add a migration.
		internal static readonly string[] PersistedToggleKeys =
		{
			"NoStress", "NoVacation", "FullRoomBrightness", "CleanRooms", "FullEnvironment",
			"NoiseReduction", "FreeStaff", "TemperatureLock", "NoWaterElectricity", "NoNeeds",
			"FreeEmployees", "LockAge", "MoreHostingDeals", "IncreaseCourierCapacity", "ReduceISPCost",
			"IncreasePrintSpeed", "FreePrint", "IncreaseBookshelfSkill", "NoMaintenance", "NoSickness",
			"FullSatisfaction", "DisableSkillDecay", "DisableBurglars", "DisableFires", "NoServerCost",
			"ReduceExpansionCost", "NoEducationCost", "IncreaseWalkSpeed", "AutoEndDesign",
			"AutoContractProgression", "AutoEndResearch", "AutoEndPatent", "ReduceBoxPrice",
			"DisableFurnitureStealing", "MoreInspiration", "MoreCreativity", "AutoResearchStart",
			"DigitalDistributionMonopol", "DisableFireInspection", "DisableForcePause",
			"DisableForceFreeze", "AutoAcceptHostingDeals", "AutoMaxMarketShare", "Experimental",
			"ForceLights", "ShowRoomCeilings", "UnlimitedSubsidiaries", "NoMissedSupportTickets",
			"NoInsuranceCost", "NoLoanInterest", "AutoPorting", "FreeMarketing", "NoOffshoreHeat",
			"NoFounderDividends"
		};

		internal static readonly string[] PersistedStoreKeys = { "EfficiencyStore", "LeadEfficiencyStore" };

		[Fact]
		public void ToggleKeys_MatchThePersistedKeySet()
		{
			Assert.Equal(PersistedToggleKeys.OrderBy(k => k), Helpers.Settings.Keys.OrderBy(k => k));
		}

		[Fact]
		public void StoreKeys_MatchThePersistedKeySet()
		{
			Assert.Equal(PersistedStoreKeys.OrderBy(k => k), Helpers.StoresSettings.Keys.OrderBy(k => k));
		}

		[Fact]
		public void FreshSettings_AreAllDisabledAndStoresAreUnset()
		{
			using (new SettingsSnapshot())
			{
				ResetToDefaults();

				Assert.All(Helpers.Settings, pair => Assert.False(pair.Value, pair.Key));
				Assert.All(Helpers.StoresSettings, pair => Assert.Null(pair.Value));
			}
		}

		[Fact]
		public void GetProperty_ReturnsStoredValueForRegisteredKey()
		{
			using (new SettingsSnapshot())
			{
				Helpers.Settings["NoStress"] = true;

				Assert.True(Helpers.GetProperty(Helpers.Settings, "NoStress"));
				Assert.False(Helpers.GetProperty(Helpers.Settings, "NoVacation"));
			}
		}

		[Fact]
		public void GetProperty_ForUnregisteredKeyReturnsDefaultAndDoesNotRegisterIt()
		{
			Assert.False(Helpers.GetProperty(Helpers.Settings, "NoSuchSetting"));
			Assert.Null(Helpers.GetProperty(Helpers.StoresSettings, "NoSuchStore"));

			Assert.DoesNotContain("NoSuchSetting", Helpers.Settings.Keys);
			Assert.DoesNotContain("NoSuchStore", Helpers.StoresSettings.Keys);
		}

		[Fact]
		public void SetProperty_UpdatesRegisteredKeysAndIgnoresUnregisteredOnes()
		{
			using (new SettingsSnapshot())
			{
				var toggleCount = Helpers.Settings.Count;
				var storeCount = Helpers.StoresSettings.Count;

				Helpers.SetProperty(Helpers.Settings, "FreePrint", true);
				Helpers.SetProperty(Helpers.Settings, "NoSuchSetting", true);
				Helpers.SetProperty(Helpers.StoresSettings, "EfficiencyStore", 5);
				Helpers.SetProperty(Helpers.StoresSettings, "NoSuchStore", 5);

				Assert.True(Helpers.Settings["FreePrint"]);
				Assert.Equal(5, Helpers.StoresSettings["EfficiencyStore"]);
				Assert.Equal(toggleCount, Helpers.Settings.Count);
				Assert.Equal(storeCount, Helpers.StoresSettings.Count);
			}
		}

		[Fact]
		public void ExtensionAccessors_BehaveLikeTheHelpersForRegisteredAndUnregisteredKeys()
		{
			using (new SettingsSnapshot())
			{
				Helpers.Settings.Toggle("LockAge");
				Assert.True(Helpers.Settings.Get("LockAge"));
				Helpers.Settings.Toggle("LockAge");
				Assert.False(Helpers.Settings.Get("LockAge"));

				Helpers.Settings.Toggle("NoSuchSetting");
				Assert.False(Helpers.Settings.Get("NoSuchSetting"));
				Assert.DoesNotContain("NoSuchSetting", Helpers.Settings.Keys);

				Helpers.StoresSettings.Set("LeadEfficiencyStore", 10);
				Assert.Equal(10, Helpers.StoresSettings.Get("LeadEfficiencyStore"));

				Helpers.StoresSettings.Set("NoSuchStore", 10);
				Assert.Null(Helpers.StoresSettings.Get("NoSuchStore"));
				Assert.DoesNotContain("NoSuchStore", Helpers.StoresSettings.Keys);
			}
		}

		[Fact]
		public void EfficiencySelectItems_StartWithTheDefaultOptionAndKeepTheirValues()
		{
			var items = Helpers.EfficiencySelectItems;

			Assert.Equal(new[] { "Default", "100%", "200%", "500%", "1000%", "2000%", "4000%", "8000%" }, items.Keys);
			Assert.Null(items["Default"]);
			Assert.Equal(new object[] { null, 1, 2, 5, 10, 20, 40, 80 }, items.Values);
		}

		internal static void ResetToDefaults()
		{
			foreach (var key in Helpers.Settings.Keys.ToList())
			{
				Helpers.Settings[key] = false;
			}

			foreach (var key in Helpers.StoresSettings.Keys.ToList())
			{
				Helpers.StoresSettings[key] = null;
			}
		}
	}
}
