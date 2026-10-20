using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// The only place a persisted setting is declared. The string is the save-file key and must never change.
	public static class SettingKeys
	{
		private static readonly HashSet<string> _registeredKeys = new HashSet<string>();
		private static readonly List<ToggleKey> _toggles = new List<ToggleKey>();
		private static readonly List<StoreKey> _stores = new List<StoreKey>();

		public static readonly ToggleKey NoStress = Toggle("NoStress");
		public static readonly ToggleKey NoVacation = Toggle("NoVacation");
		public static readonly ToggleKey FullRoomBrightness = Toggle("FullRoomBrightness");
		public static readonly ToggleKey CleanRooms = Toggle("CleanRooms");
		public static readonly ToggleKey FullEnvironment = Toggle("FullEnvironment");
		public static readonly ToggleKey NoiseReduction = Toggle("NoiseReduction");
		public static readonly ToggleKey FreeStaff = Toggle("FreeStaff");
		public static readonly ToggleKey TemperatureLock = Toggle("TemperatureLock");
		public static readonly ToggleKey NoWaterElectricity = Toggle("NoWaterElectricity");
		public static readonly ToggleKey NoNeeds = Toggle("NoNeeds");
		public static readonly ToggleKey FreeEmployees = Toggle("FreeEmployees");
		public static readonly ToggleKey LockAge = Toggle("LockAge");
		public static readonly ToggleKey MoreHostingDeals = Toggle("MoreHostingDeals");
		public static readonly ToggleKey IncreaseCourierCapacity = Toggle("IncreaseCourierCapacity");
		public static readonly ToggleKey ReduceISPCost = Toggle("ReduceISPCost");
		public static readonly ToggleKey IncreasePrintSpeed = Toggle("IncreasePrintSpeed");
		public static readonly ToggleKey FreePrint = Toggle("FreePrint");
		public static readonly ToggleKey IncreaseBookshelfSkill = Toggle("IncreaseBookshelfSkill");
		public static readonly ToggleKey NoMaintenance = Toggle("NoMaintenance");
		public static readonly ToggleKey NoSickness = Toggle("NoSickness");
		public static readonly ToggleKey FullSatisfaction = Toggle("FullSatisfaction");
		public static readonly ToggleKey DisableSkillDecay = Toggle("DisableSkillDecay");
		public static readonly ToggleKey DisableBurglars = Toggle("DisableBurglars");
		public static readonly ToggleKey DisableFires = Toggle("DisableFires");
		public static readonly ToggleKey NoServerCost = Toggle("NoServerCost");
		// Retired (#354): same as ReduceBoxPrice (#439: not persisted); the only write was GameSettings.ExpansionCost, which the game never reads.
		public static readonly ToggleKey ReduceExpansionCost = Retired("ReduceExpansionCost");
		public static readonly ToggleKey NoEducationCost = Toggle("NoEducationCost");
		public static readonly ToggleKey IncreaseWalkSpeed = Toggle("IncreaseWalkSpeed");
		public static readonly ToggleKey AutoEndDesign = Toggle("AutoEndDesign");
		public static readonly ToggleKey AutoContractProgression = Toggle("AutoContractProgression");
		public static readonly ToggleKey AutoEndResearch = Toggle("AutoEndResearch");
		public static readonly ToggleKey AutoEndPatent = Toggle("AutoEndPatent");
		// Retired: no UI and no scheduled behavior (#270). #439: the name stays reserved but is no longer persisted or loaded, so
		// new saves omit it and a true stored in an old save is ignored.
		public static readonly ToggleKey ReduceBoxPrice = Retired("ReduceBoxPrice");
		public static readonly ToggleKey DisableFurnitureStealing = Toggle("DisableFurnitureStealing");
		public static readonly ToggleKey MoreInspiration = Toggle("MoreInspiration");
		// Retired (#374): same as ReduceBoxPrice.
		public static readonly ToggleKey MoreCreativity = Retired("MoreCreativity");
		public static readonly ToggleKey AutoResearchStart = Toggle("AutoResearchStart");
		public static readonly ToggleKey DigitalDistributionMonopol = Toggle("DigitalDistributionMonopol");
		public static readonly ToggleKey DisableFireInspection = Toggle("DisableFireInspection");
		public static readonly ToggleKey DisableForcePause = Toggle("DisableForcePause");
		public static readonly ToggleKey DisableForceFreeze = Toggle("DisableForceFreeze");
		public static readonly ToggleKey AutoAcceptHostingDeals = Toggle("AutoAcceptHostingDeals");
		// Retired (see #150): same as ReduceBoxPrice.
		public static readonly ToggleKey AutoMaxMarketShare = Retired("AutoMaxMarketShare");
		// Retired (#245): the Experimental opt-in moved to the mod Options (ExperimentalOptions). The key stays declared so old
		// saves load, but nothing reads it, so a saved true cannot enable experiments.
		public static readonly ToggleKey Experimental = Retired("Experimental");
		public static readonly ToggleKey ForceLights = Toggle("ForceLights");
		public static readonly ToggleKey ShowRoomCeilings = Toggle("ShowRoomCeilings");
		public static readonly ToggleKey UnlimitedSubsidiaries = Toggle("UnlimitedSubsidiaries");
		public static readonly ToggleKey NoMissedSupportTickets = Toggle("NoMissedSupportTickets");
		public static readonly ToggleKey NoInsuranceCost = Toggle("NoInsuranceCost");
		public static readonly ToggleKey NoLoanInterest = Toggle("NoLoanInterest");
		public static readonly ToggleKey AutoPorting = Toggle("AutoPorting");
		public static readonly ToggleKey FreeMarketing = Toggle("FreeMarketing");
		public static readonly ToggleKey NoOffshoreHeat = Toggle("NoOffshoreHeat");
		public static readonly ToggleKey NoFounderDividends = Toggle("NoFounderDividends");

		public static readonly StoreKey EfficiencyStore = Store("EfficiencyStore");
		public static readonly StoreKey LeadEfficiencyStore = Store("LeadEfficiencyStore");

		internal static Dictionary<string, bool> CreateToggleDefaults()
		{
			var defaults = new Dictionary<string, bool>();
			foreach (var toggle in _toggles)
			{
				defaults[toggle.Key] = toggle.DefaultValue;
			}
			return defaults;
		}

		internal static Dictionary<string, object> CreateStoreDefaults()
		{
			var defaults = new Dictionary<string, object>();
			foreach (var store in _stores)
			{
				defaults[store.Key] = store.DefaultValue;
			}
			return defaults;
		}

		private static ToggleKey Toggle(string key)
		{
			var setting = new ToggleKey(Register(key), false);
			_toggles.Add(setting);
			return setting;
		}

		// A retired setting keeps its declaration and reserved name but is not a live toggle: it is left out of the
		// defaults, Helpers.Settings, Serialize and Deserialize.
		private static ToggleKey Retired(string key)
		{
			return new ToggleKey(Register(key), false);
		}

		private static StoreKey Store(string key)
		{
			var setting = new StoreKey(Register(key), null);
			_stores.Add(setting);
			return setting;
		}

		private static string Register(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("A setting key is required.", nameof(key));
			}

			if (!_registeredKeys.Add(key))
			{
				throw new ArgumentException("Setting key '" + key + "' is registered twice.", nameof(key));
			}

			return key;
		}
	}
}
