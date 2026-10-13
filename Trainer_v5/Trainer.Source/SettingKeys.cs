using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// The only place a persisted setting is declared. The string is the save-file key and must never change.
	public static class SettingKeys
	{
		private static readonly HashSet<string> _registeredKeys = new HashSet<string>();
		private static readonly List<SettingKey<bool>> _toggles = new List<SettingKey<bool>>();
		private static readonly List<SettingKey<object>> _stores = new List<SettingKey<object>>();

		public static readonly SettingKey<bool> NoStress = Toggle("NoStress");
		public static readonly SettingKey<bool> NoVacation = Toggle("NoVacation");
		public static readonly SettingKey<bool> FullRoomBrightness = Toggle("FullRoomBrightness");
		public static readonly SettingKey<bool> CleanRooms = Toggle("CleanRooms");
		public static readonly SettingKey<bool> FullEnvironment = Toggle("FullEnvironment");
		public static readonly SettingKey<bool> NoiseReduction = Toggle("NoiseReduction");
		public static readonly SettingKey<bool> FreeStaff = Toggle("FreeStaff");
		public static readonly SettingKey<bool> TemperatureLock = Toggle("TemperatureLock");
		public static readonly SettingKey<bool> NoWaterElectricity = Toggle("NoWaterElectricity");
		public static readonly SettingKey<bool> NoNeeds = Toggle("NoNeeds");
		public static readonly SettingKey<bool> FreeEmployees = Toggle("FreeEmployees");
		public static readonly SettingKey<bool> LockAge = Toggle("LockAge");
		public static readonly SettingKey<bool> MoreHostingDeals = Toggle("MoreHostingDeals");
		public static readonly SettingKey<bool> IncreaseCourierCapacity = Toggle("IncreaseCourierCapacity");
		public static readonly SettingKey<bool> ReduceISPCost = Toggle("ReduceISPCost");
		public static readonly SettingKey<bool> IncreasePrintSpeed = Toggle("IncreasePrintSpeed");
		public static readonly SettingKey<bool> FreePrint = Toggle("FreePrint");
		public static readonly SettingKey<bool> IncreaseBookshelfSkill = Toggle("IncreaseBookshelfSkill");
		public static readonly SettingKey<bool> NoMaintenance = Toggle("NoMaintenance");
		public static readonly SettingKey<bool> NoSickness = Toggle("NoSickness");
		public static readonly SettingKey<bool> FullSatisfaction = Toggle("FullSatisfaction");
		public static readonly SettingKey<bool> DisableSkillDecay = Toggle("DisableSkillDecay");
		public static readonly SettingKey<bool> DisableBurglars = Toggle("DisableBurglars");
		public static readonly SettingKey<bool> DisableFires = Toggle("DisableFires");
		public static readonly SettingKey<bool> NoServerCost = Toggle("NoServerCost");
		public static readonly SettingKey<bool> ReduceExpansionCost = Toggle("ReduceExpansionCost");
		public static readonly SettingKey<bool> NoEducationCost = Toggle("NoEducationCost");
		public static readonly SettingKey<bool> IncreaseWalkSpeed = Toggle("IncreaseWalkSpeed");
		public static readonly SettingKey<bool> AutoEndDesign = Toggle("AutoEndDesign");
		public static readonly SettingKey<bool> AutoContractProgression = Toggle("AutoContractProgression");
		public static readonly SettingKey<bool> AutoEndResearch = Toggle("AutoEndResearch");
		public static readonly SettingKey<bool> AutoEndPatent = Toggle("AutoEndPatent");
		public static readonly SettingKey<bool> ReduceBoxPrice = Toggle("ReduceBoxPrice");
		public static readonly SettingKey<bool> DisableFurnitureStealing = Toggle("DisableFurnitureStealing");
		public static readonly SettingKey<bool> MoreInspiration = Toggle("MoreInspiration");
		public static readonly SettingKey<bool> MoreCreativity = Toggle("MoreCreativity");
		public static readonly SettingKey<bool> AutoResearchStart = Toggle("AutoResearchStart");
		public static readonly SettingKey<bool> DigitalDistributionMonopol = Toggle("DigitalDistributionMonopol");
		public static readonly SettingKey<bool> DisableFireInspection = Toggle("DisableFireInspection");
		public static readonly SettingKey<bool> DisableForcePause = Toggle("DisableForcePause");
		public static readonly SettingKey<bool> DisableForceFreeze = Toggle("DisableForceFreeze");
		public static readonly SettingKey<bool> AutoAcceptHostingDeals = Toggle("AutoAcceptHostingDeals");
		public static readonly SettingKey<bool> AutoMaxMarketShare = Toggle("AutoMaxMarketShare");
		public static readonly SettingKey<bool> Experimental = Toggle("Experimental");
		public static readonly SettingKey<bool> ForceLights = Toggle("ForceLights");
		public static readonly SettingKey<bool> ShowRoomCeilings = Toggle("ShowRoomCeilings");
		public static readonly SettingKey<bool> UnlimitedSubsidiaries = Toggle("UnlimitedSubsidiaries");
		public static readonly SettingKey<bool> NoMissedSupportTickets = Toggle("NoMissedSupportTickets");
		public static readonly SettingKey<bool> NoInsuranceCost = Toggle("NoInsuranceCost");
		public static readonly SettingKey<bool> NoLoanInterest = Toggle("NoLoanInterest");
		public static readonly SettingKey<bool> AutoPorting = Toggle("AutoPorting");
		public static readonly SettingKey<bool> FreeMarketing = Toggle("FreeMarketing");
		public static readonly SettingKey<bool> NoOffshoreHeat = Toggle("NoOffshoreHeat");
		public static readonly SettingKey<bool> NoFounderDividends = Toggle("NoFounderDividends");

		public static readonly SettingKey<object> EfficiencyStore = Store("EfficiencyStore");
		public static readonly SettingKey<object> LeadEfficiencyStore = Store("LeadEfficiencyStore");

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

		private static SettingKey<bool> Toggle(string key)
		{
			var setting = new SettingKey<bool>(Register(key), false);
			_toggles.Add(setting);
			return setting;
		}

		private static SettingKey<object> Store(string key)
		{
			var setting = new SettingKey<object>(Register(key), null);
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
