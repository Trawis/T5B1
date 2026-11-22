using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Trainer_v5.Tests
{
	[Collection(SettingsCollection.Name)]
	public class SettingsTests
	{
		// Every key below is persisted in saves: renaming or removing one resets that toggle for players, so it needs a migration.
		// Retired keys (RetiredSettingsTests) are deliberately absent: they are no longer written or read.
		internal static readonly string[] PersistedToggleKeys =
		{
			"NoStress", "NoVacation", "FullRoomBrightness", "CleanRooms", "FullEnvironment",
			"NoiseReduction", "FreeStaff", "TemperatureLock", "NoWaterElectricity", "NoNeeds",
			"FreeEmployees", "LockAge", "MoreHostingDeals", "IncreaseCourierCapacity", "ReduceISPCost",
			"IncreasePrintSpeed", "FreePrint", "IncreaseBookshelfSkill", "NoMaintenance", "NoSickness",
			"FullSatisfaction", "DisableBurglars", "DisableFires", "NoServerCost",
			"NoEducationCost", "IncreaseWalkSpeed", "AutoEndDesign",
			"AutoContractProgression", "AutoEndResearch", "AutoEndPatent",
			"DisableFurnitureStealing", "MoreInspiration", "AutoResearchStart",
			"DigitalDistributionMonopol", "DisableFireInspection", "DisableForcePause",
			"DisableForceFreeze", "AutoAcceptHostingDeals",
			"ForceLights", "ShowRoomCeilings", "UnlimitedSubsidiaries", "NoMissedSupportTickets",
			"NoInsuranceCost", "NoLoanInterest", "AutoPorting", "FreeMarketing", "NoOffshoreHeat",
			"NoFounderDividends", "NoForgetfulSkillLoss", "AutoInstantPatent", "AutoInstantResearch", "AutoMaxFollowers"
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
			// Read from the declared keys, not from ResetToDefaults, so a changed SettingKeys default fails here.
			var toggles = SettingKeys.CreateToggleDefaults();
			var stores = SettingKeys.CreateStoreDefaults();

			Assert.Equal(PersistedToggleKeys.Length, toggles.Count);
			Assert.Equal(PersistedStoreKeys.Length, stores.Count);
			Assert.All(toggles, pair => Assert.False(pair.Value, pair.Key));
			Assert.All(stores, pair => Assert.Null(pair.Value));

			// The live dictionaries start from the same declared defaults; no other test leaves them modified.
			foreach (var pair in toggles)
			{
				Assert.Equal(pair.Value, Helpers.Settings[pair.Key]);
			}

			foreach (var pair in stores)
			{
				Assert.Equal(pair.Value, Helpers.StoresSettings[pair.Key]);
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
