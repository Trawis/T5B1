using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Every scheduled trainer feature: when it runs, under which FeatureGuard id, and what it calls. Order within a
	// trigger is execution order. To add a feature, add its entries here.
	// Not registered on purpose: AutoMaxMarketShare (disabled, see #150) and the Cheats.* flags that
	// TrainerBehaviour pushes once per load.
	internal static class FeatureRegistry
	{
		// Periodic features that deliberately have no Enabled row: switching them on changes nothing until the next
		// tick of their cadence. Every other Minute, Hour, Day or Month feature must catch up when it is enabled,
		// which FeatureScheduleTests checks, so a new toggle cannot omit the decision.
		public static readonly string[] NoCatchUp =
		{
			"NoMissedSupportTickets",
			"AutoPorting",
			"NoInsuranceCost",
			"FreeMarketing",
			"LockAge",
			"NoLoanInterest",
			"NoFounderDividends"
		};

		public static List<FeatureEntry> Create()
		{
			var entries = new List<FeatureEntry>();
			// Hour
			Add(entries, FeatureTrigger.Hour, "MoreHostingDeals", SettingKeys.MoreHostingDeals, MarketFeatures.ApplyHostingDealTiming);
			Add(entries, FeatureTrigger.Hour, "FreeStaff", SettingKeys.FreeStaff, CompanyFeatures.ApplyFreeStaff);
			Add(entries, FeatureTrigger.Hour, "NoServerCost", SettingKeys.NoServerCost, CompanyFeatures.ApplyNoServerCost);
			Add(entries, FeatureTrigger.Hour, "NoWaterElectricity", SettingKeys.NoWaterElectricity, CompanyFeatures.ApplyNoWaterElectricityBills);
			Add(entries, FeatureTrigger.Hour, "NoMaintenance", SettingKeys.NoMaintenance, EnvironmentFeatures.ApplyNoMaintenance);
			Add(entries, FeatureTrigger.Hour, "NoMissedSupportTickets", SettingKeys.NoMissedSupportTickets, WorkItemFeatures.ApplyNoMissedSupportTickets);
			Add(entries, FeatureTrigger.Hour, "FreePrint", SettingKeys.FreePrint, ProductFeatures.ApplyFreePrint);
			Add(entries, FeatureTrigger.Hour, "IncreasePrintSpeed", SettingKeys.IncreasePrintSpeed, ProductFeatures.ApplyIncreasePrintSpeed);

			// Minute
			Add(entries, FeatureTrigger.Minute, "AutoEndDesign", SettingKeys.AutoEndDesign, WorkItemFeatures.ApplyAutoEndDesign);
			Add(entries, FeatureTrigger.Minute, "AutoContractProgression", SettingKeys.AutoContractProgression, WorkItemFeatures.ApplyAutoContractProgression);
			Add(entries, FeatureTrigger.Minute, "AutoEndResearch", SettingKeys.AutoEndResearch, WorkItemFeatures.ApplyAutoEndResearch);
			Add(entries, FeatureTrigger.Minute, "AutoEndPatent", SettingKeys.AutoEndPatent, WorkItemFeatures.ApplyAutoEndPatent);
			Add(entries, FeatureTrigger.Minute, "AutoResearchStart", SettingKeys.AutoResearchStart, WorkItemFeatures.ApplyAutoResearchStart);
			Add(entries, FeatureTrigger.Minute, "AutoAcceptHostingDeals", SettingKeys.AutoAcceptHostingDeals, MarketFeatures.ApplyAutoAcceptHostingDeals);
			Add(entries, FeatureTrigger.Minute, "AutoPorting", SettingKeys.AutoPorting, WorkItemFeatures.ApplyAutoPorting);
			Add(entries, FeatureTrigger.Minute, "FullEnvironment", SettingKeys.FullEnvironment, EnvironmentFeatures.ApplyFullEnvironment);
			Add(entries, FeatureTrigger.Minute, "FullRoomBrightness", SettingKeys.FullRoomBrightness, EnvironmentFeatures.ApplyFullRoomBrightness);
			Add(entries, FeatureTrigger.Minute, "IncreaseBookshelfSkill", SettingKeys.IncreaseBookshelfSkill, EnvironmentFeatures.ApplyIncreaseBookshelfSkill);
			Add(entries, FeatureTrigger.Minute, "NoWaterElectricity", SettingKeys.NoWaterElectricity, EnvironmentFeatures.ApplyNoWaterElectricityToAllFurniture);
			Add(entries, FeatureTrigger.Minute, "DisableFires", SettingKeys.DisableFires, EnvironmentFeatures.ApplyDisableFiresFireStarter);
			Add(entries, FeatureTrigger.Minute, "IncreaseWalkSpeed", SettingKeys.IncreaseWalkSpeed, EmployeeFeatures.ApplyIncreaseWalkSpeed);
			Add(entries, FeatureTrigger.Minute, "IncreaseCourierCapacity", SettingKeys.IncreaseCourierCapacity, CompanyFeatures.ApplyIncreaseCourierCapacity);
			Add(entries, FeatureTrigger.Minute, "ReduceISPCost", SettingKeys.ReduceISPCost, CompanyFeatures.ApplyReduceISPCost);
			Add(entries, FeatureTrigger.Minute, "ReduceExpansionCost", SettingKeys.ReduceExpansionCost, CompanyFeatures.ApplyReduceExpansionCost);

			// Day
			Add(entries, FeatureTrigger.Day, "DigitalDistributionMonopol", SettingKeys.DigitalDistributionMonopol, MarketFeatures.ApplyDigitalDistributionMonopol);
			Add(entries, FeatureTrigger.Day, "NoInsuranceCost", SettingKeys.NoInsuranceCost, CompanyFeatures.ApplyNoInsuranceCost);
			Add(entries, FeatureTrigger.Day, "FreeMarketing", SettingKeys.FreeMarketing, MarketFeatures.ApplyFreeMarketing);

			// Month
			Add(entries, FeatureTrigger.Month, "FreeEmployees", SettingKeys.FreeEmployees, EmployeeFeatures.ApplyFreeEmployees);
			Add(entries, FeatureTrigger.Month, "MoreCreativity", SettingKeys.MoreCreativity, EmployeeFeatures.ApplyMoreCreativity);
			Add(entries, FeatureTrigger.Month, "DisableBurglars", SettingKeys.DisableBurglars, EnvironmentFeatures.ApplyDisableBurglars);
			Add(entries, FeatureTrigger.Month, "DisableFireInspection", SettingKeys.DisableFireInspection, EnvironmentFeatures.ApplyDisableFireInspection);
			Add(entries, FeatureTrigger.Month, "DisableFurnitureStealing", SettingKeys.DisableFurnitureStealing, EnvironmentFeatures.ApplyDisableFurnitureStealing);
			Add(entries, FeatureTrigger.Month, "NoVacation", SettingKeys.NoVacation, EmployeeFeatures.ApplyNoVacation);
			Add(entries, FeatureTrigger.Month, "LockAge", SettingKeys.LockAge, EmployeeFeatures.ApplyLockAge);
			Add(entries, FeatureTrigger.Month, "NoLoanInterest", SettingKeys.NoLoanInterest, CompanyFeatures.ApplyNoLoanInterest);
			Add(entries, FeatureTrigger.Month, "NoFounderDividends", SettingKeys.NoFounderDividends, CompanyFeatures.ApplyNoFounderDividends);

			// Enabled / Disabled (observed once per frame, in this order)
			Add(entries, FeatureTrigger.Enabled, "NoMaintenance", SettingKeys.NoMaintenance, EnvironmentFeatures.ApplyNoMaintenance);
			Add(entries, FeatureTrigger.Enabled, "NoVacation", SettingKeys.NoVacation, EmployeeFeatures.ApplyNoVacation);
			Add(entries, FeatureTrigger.Enabled, "MoreHostingDeals", SettingKeys.MoreHostingDeals, MarketFeatures.ApplyHostingDealTiming);
			Add(entries, FeatureTrigger.Enabled, "AutoEndDesign", SettingKeys.AutoEndDesign, WorkItemFeatures.ApplyAutoEndDesign);
			Add(entries, FeatureTrigger.Enabled, "AutoContractProgression", SettingKeys.AutoContractProgression, WorkItemFeatures.ApplyAutoContractProgression);
			Add(entries, FeatureTrigger.Enabled, "AutoEndResearch", SettingKeys.AutoEndResearch, WorkItemFeatures.ApplyAutoEndResearch);
			Add(entries, FeatureTrigger.Enabled, "AutoEndPatent", SettingKeys.AutoEndPatent, WorkItemFeatures.ApplyAutoEndPatent);
			Add(entries, FeatureTrigger.Enabled, "FreeStaff", SettingKeys.FreeStaff, CompanyFeatures.ApplyFreeStaff);
			Add(entries, FeatureTrigger.Enabled, "NoServerCost", SettingKeys.NoServerCost, CompanyFeatures.ApplyNoServerCost);
			Add(entries, FeatureTrigger.Enabled, "NoWaterElectricity", SettingKeys.NoWaterElectricity, () =>
			{
				CompanyFeatures.ApplyNoWaterElectricityBills();
				EnvironmentFeatures.ApplyNoWaterElectricityToAllFurniture();
			});
			Add(entries, FeatureTrigger.Enabled, "DisableFurnitureStealing", SettingKeys.DisableFurnitureStealing, EnvironmentFeatures.ApplyDisableFurnitureStealing);
			Add(entries, FeatureTrigger.Enabled, "NoEducationCost", SettingKeys.NoEducationCost, CompanyFeatures.ApplyNoEducationCost);
			Add(entries, FeatureTrigger.Disabled, "NoEducationCost", SettingKeys.NoEducationCost, CompanyFeatures.RestoreEducationCost);
			Add(entries, FeatureTrigger.Enabled, "AutoResearchStart", SettingKeys.AutoResearchStart, WorkItemFeatures.ApplyAutoResearchStart);
			Add(entries, FeatureTrigger.Enabled, "DigitalDistributionMonopol", SettingKeys.DigitalDistributionMonopol, MarketFeatures.ApplyDigitalDistributionMonopol);
			Add(entries, FeatureTrigger.Enabled, "AutoAcceptHostingDeals", SettingKeys.AutoAcceptHostingDeals, MarketFeatures.ApplyAutoAcceptHostingDeals);
			Add(entries, FeatureTrigger.Enabled, "FreeEmployees", SettingKeys.FreeEmployees, EmployeeFeatures.ApplyFreeEmployees);
			Add(entries, FeatureTrigger.Enabled, "MoreCreativity", SettingKeys.MoreCreativity, EmployeeFeatures.ApplyMoreCreativity);
			Add(entries, FeatureTrigger.Enabled, "DisableBurglars", SettingKeys.DisableBurglars, EnvironmentFeatures.ApplyDisableBurglars);
			Add(entries, FeatureTrigger.Enabled, "DisableFireInspection", SettingKeys.DisableFireInspection, EnvironmentFeatures.ApplyDisableFireInspection);
			Add(entries, FeatureTrigger.Enabled, "FreePrint", SettingKeys.FreePrint, ProductFeatures.ApplyFreePrint);
			Add(entries, FeatureTrigger.Disabled, "FreePrint", SettingKeys.FreePrint, ProductFeatures.RestoreFreePrint);
			Add(entries, FeatureTrigger.Enabled, "IncreasePrintSpeed", SettingKeys.IncreasePrintSpeed, ProductFeatures.ApplyIncreasePrintSpeed);
			Add(entries, FeatureTrigger.Enabled, "FullEnvironment", SettingKeys.FullEnvironment, EnvironmentFeatures.ApplyFullEnvironment);
			Add(entries, FeatureTrigger.Disabled, "FullEnvironment", SettingKeys.FullEnvironment, EnvironmentFeatures.RestoreFullEnvironment);
			Add(entries, FeatureTrigger.Enabled, "FullRoomBrightness", SettingKeys.FullRoomBrightness, EnvironmentFeatures.ApplyFullRoomBrightness);
			Add(entries, FeatureTrigger.Disabled, "FullRoomBrightness", SettingKeys.FullRoomBrightness, EnvironmentFeatures.RestoreFullRoomBrightness);
			Add(entries, FeatureTrigger.Enabled, "IncreaseBookshelfSkill", SettingKeys.IncreaseBookshelfSkill, EnvironmentFeatures.ApplyIncreaseBookshelfSkill);
			Add(entries, FeatureTrigger.Enabled, "DisableFires", SettingKeys.DisableFires, EnvironmentFeatures.ApplyDisableFiresFireStarter);
			Add(entries, FeatureTrigger.Enabled, "IncreaseWalkSpeed", SettingKeys.IncreaseWalkSpeed, EmployeeFeatures.ApplyIncreaseWalkSpeed);
			Add(entries, FeatureTrigger.Disabled, "IncreaseWalkSpeed", SettingKeys.IncreaseWalkSpeed, EmployeeFeatures.RestoreIncreaseWalkSpeed);
			Add(entries, FeatureTrigger.Enabled, "IncreaseCourierCapacity", SettingKeys.IncreaseCourierCapacity, CompanyFeatures.ApplyIncreaseCourierCapacity);
			Add(entries, FeatureTrigger.Disabled, "IncreaseCourierCapacity", SettingKeys.IncreaseCourierCapacity, CompanyFeatures.RestoreIncreaseCourierCapacity);
			Add(entries, FeatureTrigger.Enabled, "ReduceISPCost", SettingKeys.ReduceISPCost, CompanyFeatures.ApplyReduceISPCost);
			Add(entries, FeatureTrigger.Disabled, "ReduceISPCost", SettingKeys.ReduceISPCost, CompanyFeatures.RestoreReduceISPCost);
			Add(entries, FeatureTrigger.Enabled, "ReduceExpansionCost", SettingKeys.ReduceExpansionCost, CompanyFeatures.ApplyReduceExpansionCost);
			Add(entries, FeatureTrigger.Disabled, "ReduceExpansionCost", SettingKeys.ReduceExpansionCost, CompanyFeatures.RestoreReduceExpansionCost);

			// Frame
			Add(entries, FeatureTrigger.Frame, "NoiseReduction", SettingKeys.NoiseReduction, EnvironmentFeatures.ApplyNoiseReduction);
			Add(entries, FeatureTrigger.Frame, "DisableFires", SettingKeys.DisableFires, EnvironmentFeatures.ApplyDisableActiveFires);
			Add(entries, FeatureTrigger.Frame, "CleanRooms", SettingKeys.CleanRooms, EnvironmentFeatures.ApplyCleanRooms);
			Add(entries, FeatureTrigger.Frame, "NoSickness", SettingKeys.NoSickness, EnvironmentFeatures.ApplyNoSickness);
			Add(entries, FeatureTrigger.Frame, "TemperatureLock", SettingKeys.TemperatureLock, EnvironmentFeatures.ApplyTemperatureLock);
			Add(entries, FeatureTrigger.Frame, "FullSatisfaction", SettingKeys.FullSatisfaction, EmployeeFeatures.ApplyFullSatisfaction);
			Add(entries, FeatureTrigger.Frame, "NoStress", SettingKeys.NoStress, EmployeeFeatures.ApplyNoStress);
			AddStore(entries, FeatureTrigger.Frame, "LeadEfficiencyStore", SettingKeys.LeadEfficiencyStore, EmployeeFeatures.ApplyLeadEfficiencyStore);
			AddStore(entries, FeatureTrigger.Frame, "EfficiencyStore", SettingKeys.EfficiencyStore, EmployeeFeatures.ApplyEfficiencyStore);
			Add(entries, FeatureTrigger.Frame, "NoNeeds", SettingKeys.NoNeeds, EmployeeFeatures.ApplyNoNeeds);
			Add(entries, FeatureTrigger.Frame, "MoreInspiration", SettingKeys.MoreInspiration, EmployeeFeatures.ApplyMoreInspiration);
			Add(entries, FeatureTrigger.Frame, "DisableForcePause", SettingKeys.DisableForcePause, CompanyFeatures.ApplyDisableForcePause);
			Add(entries, FeatureTrigger.Frame, "DisableForceFreeze", SettingKeys.DisableForceFreeze, CompanyFeatures.ApplyDisableForceFreeze);
			Add(entries, FeatureTrigger.Frame, "NoOffshoreHeat", SettingKeys.NoOffshoreHeat, CompanyFeatures.ApplyNoOffshoreHeat);
			return entries;
		}

		private static void Add(List<FeatureEntry> entries, FeatureTrigger trigger, string id, ToggleKey toggle, Action action)
		{
			entries.Add(new FeatureEntry(id, trigger, toggle, null, action));
		}

		private static void AddStore(List<FeatureEntry> entries, FeatureTrigger trigger, string id, StoreKey store, Action action)
		{
			entries.Add(new FeatureEntry(id, trigger, null, store, action));
		}
	}
}
