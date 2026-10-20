using System.Collections.Generic;

namespace Trainer_v5
{
	// Not generic: the game's source-mod compiler emits invalid IL for fields of user-defined generic types.
	internal sealed class SettingToggleRow
	{
		public readonly TrainerDomain Domain;
		public readonly string LocKey;
		public readonly string DefaultText;
		public readonly ToggleKey Key;

		public SettingToggleRow(TrainerDomain domain, string locKey, string defaultText, ToggleKey key)
		{
			Domain = domain;
			LocKey = locKey;
			DefaultText = defaultText;
			Key = key;
		}
	}

	// The Trainer Settings toggles, each in one domain (#449), in display order within the domain. Standard toggles only
	// flip their setting; the three in CreateImmediate also apply an effect at once (SettingsWindow runs it).
	// Built in methods (see Workshop Source Build).
	internal static class SettingToggleRows
	{
		public static List<SettingToggleRow> Create()
		{
			var rows = new List<SettingToggleRow>();
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "DisableNeeds", "Disable Needs", SettingKeys.NoNeeds));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "DisableStress", "Disable Stress", SettingKeys.NoStress));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "FreeEmployees", "Free Employees", SettingKeys.FreeEmployees));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "FreeStaff", "Free Staff", SettingKeys.FreeStaff));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "FullSatisfaction", "Full Satisfaction", SettingKeys.FullSatisfaction));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "LockAge", "Lock Age of Employees", SettingKeys.LockAge));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "NoVacation", "No Vacation", SettingKeys.NoVacation));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "NoSickness", "No Sickness", SettingKeys.NoSickness));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "AutoResearchStart", "Auto Research Start", SettingKeys.AutoResearchStart));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "DigitalDistributionMonopol", "Digital Distribution Monopol", SettingKeys.DigitalDistributionMonopol));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "DisableFireInspection", "Disable Fire Inspection", SettingKeys.DisableFireInspection));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "FullEnvironment", "Full Environment", SettingKeys.FullEnvironment));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "FullSunLight", "Full Sun Light", SettingKeys.FullRoomBrightness));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "LockTemperature", "Lock Temperature To 21", SettingKeys.TemperatureLock));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "NoMaintenance", "No Maintenance", SettingKeys.NoMaintenance));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "NoiseReduction", "Noise Reduction", SettingKeys.NoiseReduction));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "RoomsNeverDirty", "Rooms Never Dirty", SettingKeys.CleanRooms));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "NoEducationCost", "No Education Cost", SettingKeys.NoEducationCost));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "DisableFires", "Disable Fires", SettingKeys.DisableFires));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "DisableBurglars", "Disable Burglars", SettingKeys.DisableBurglars));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "AutoDesignEnd", "Auto Design End", SettingKeys.AutoEndDesign));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "AutoContractProgression", "Auto Contract Progression", SettingKeys.AutoContractProgression));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "AutoResearchEnd", "Auto Research End", SettingKeys.AutoEndResearch));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "AutoPatentEnd", "Auto Patent End", SettingKeys.AutoEndPatent));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "IncreaseWalkSpeed", "Increase Walk Speed", SettingKeys.IncreaseWalkSpeed));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "DisableFurnitureStealing", "Disable Furniture Stealing", SettingKeys.DisableFurnitureStealing));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "FreePrint", "Free Print", SettingKeys.FreePrint));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "FreeWaterElectricity", "Free Water & Electricity", SettingKeys.NoWaterElectricity));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "IncreaseBookshelfSkill", "Increase Bookshelf Skill", SettingKeys.IncreaseBookshelfSkill));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "IncreaseCourierCapacity", "Increase Courier Capacity", SettingKeys.IncreaseCourierCapacity));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "IncreasePrintSpeed", "Increase Print Speed", SettingKeys.IncreasePrintSpeed));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "MoreHostingDeals", "More Hosting Deals", SettingKeys.MoreHostingDeals));
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "ReduceInternetCost", "Reduce Internet Cost", SettingKeys.ReduceISPCost));
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "NoServerCost", "No Server Cost", SettingKeys.NoServerCost));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "DisableForcePause", "Disable Force Pause", SettingKeys.DisableForcePause));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "DisableForceFreeze", "Disable Force Freeze", SettingKeys.DisableForceFreeze));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "AutoAcceptHostingDeals", "Auto Accept Hosting Deals", SettingKeys.AutoAcceptHostingDeals));
			rows.Add(new SettingToggleRow(TrainerDomain.Work, "AutoPorting", "Auto Porting", SettingKeys.AutoPorting));
			rows.Add(new SettingToggleRow(TrainerDomain.Employee, "MoreInspiration", "More Inspiration", SettingKeys.MoreInspiration));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "NoMissedSupportTickets", "No Missed Support Tickets", SettingKeys.NoMissedSupportTickets));
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "NoInsuranceCost", "No Insurance Cost", SettingKeys.NoInsuranceCost));
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "NoLoanInterest", "No Loan Interest", SettingKeys.NoLoanInterest));
			rows.Add(new SettingToggleRow(TrainerDomain.Product, "FreeMarketing", "Free Marketing", SettingKeys.FreeMarketing));
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "NoOffshoreHeat", "No Offshore Heat", SettingKeys.NoOffshoreHeat));
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "NoFounderDividends", "No Founder Dividends", SettingKeys.NoFounderDividends));
			return rows;
		}

		// Toggles whose handler applies the new value to the running game as well; shown after the standard rows of their domain.
		public static List<SettingToggleRow> CreateImmediate()
		{
			var rows = new List<SettingToggleRow>();
			rows.Add(new SettingToggleRow(TrainerDomain.Company, "UnlimitedSubsidiaries", "Unlimited Subsidiaries", SettingKeys.UnlimitedSubsidiaries));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "ForceLights", "Force Lights", SettingKeys.ForceLights));
			rows.Add(new SettingToggleRow(TrainerDomain.Environment, "ShowRoomCeilings", "Show Room Ceilings", SettingKeys.ShowRoomCeilings));
			return rows;
		}
	}
}
