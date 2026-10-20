using System.Collections.Generic;

namespace Trainer_v5
{
	// Not generic: the game's source-mod compiler emits invalid IL for fields of user-defined generic types.
	internal sealed class SettingToggleRow
	{
		public readonly int Column;
		public readonly string LocKey;
		public readonly string DefaultText;
		public readonly ToggleKey Key;

		public SettingToggleRow(int column, string locKey, string defaultText, ToggleKey key)
		{
			Column = column;
			LocKey = locKey;
			DefaultText = defaultText;
			Key = key;
		}
	}

	// The standard Trainer Settings toggles in display order per column (#274). Side-effect toggles stay in
	// SettingsWindow. Built in a method (see Workshop Source Build).
	internal static class SettingToggleRows
	{
		public static List<SettingToggleRow> Create()
		{
			var rows = new List<SettingToggleRow>();
			rows.Add(new SettingToggleRow(2, "DisableNeeds", "Disable Needs", SettingKeys.NoNeeds));
			rows.Add(new SettingToggleRow(2, "DisableStress", "Disable Stress", SettingKeys.NoStress));
			rows.Add(new SettingToggleRow(2, "FreeEmployees", "Free Employees", SettingKeys.FreeEmployees));
			rows.Add(new SettingToggleRow(2, "FreeStaff", "Free Staff", SettingKeys.FreeStaff));
			rows.Add(new SettingToggleRow(2, "FullSatisfaction", "Full Satisfaction", SettingKeys.FullSatisfaction));
			rows.Add(new SettingToggleRow(2, "LockAge", "Lock Age of Employees", SettingKeys.LockAge));
			rows.Add(new SettingToggleRow(2, "NoVacation", "No Vacation", SettingKeys.NoVacation));
			rows.Add(new SettingToggleRow(2, "NoSickness", "No Sickness", SettingKeys.NoSickness));
			rows.Add(new SettingToggleRow(2, "AutoResearchStart", "Auto Research Start", SettingKeys.AutoResearchStart));
			rows.Add(new SettingToggleRow(2, "DigitalDistributionMonopol", "Digital Distribution Monopol", SettingKeys.DigitalDistributionMonopol));
			rows.Add(new SettingToggleRow(2, "DisableFireInspection", "Disable Fire Inspection", SettingKeys.DisableFireInspection));
			rows.Add(new SettingToggleRow(3, "FullEnvironment", "Full Environment", SettingKeys.FullEnvironment));
			rows.Add(new SettingToggleRow(3, "FullSunLight", "Full Sun Light", SettingKeys.FullRoomBrightness));
			rows.Add(new SettingToggleRow(3, "LockTemperature", "Lock Temperature To 21", SettingKeys.TemperatureLock));
			rows.Add(new SettingToggleRow(3, "NoMaintenance", "No Maintenance", SettingKeys.NoMaintenance));
			rows.Add(new SettingToggleRow(3, "NoiseReduction", "Noise Reduction", SettingKeys.NoiseReduction));
			rows.Add(new SettingToggleRow(3, "RoomsNeverDirty", "Rooms Never Dirty", SettingKeys.CleanRooms));
			rows.Add(new SettingToggleRow(3, "NoEducationCost", "No Education Cost", SettingKeys.NoEducationCost));
			rows.Add(new SettingToggleRow(3, "DisableFires", "Disable Fires", SettingKeys.DisableFires));
			rows.Add(new SettingToggleRow(3, "AutoDesignEnd", "Auto Design End", SettingKeys.AutoEndDesign));
			rows.Add(new SettingToggleRow(3, "AutoContractProgression", "Auto Contract Progression", SettingKeys.AutoContractProgression));
			rows.Add(new SettingToggleRow(3, "AutoResearchEnd", "Auto Research End", SettingKeys.AutoEndResearch));
			rows.Add(new SettingToggleRow(3, "AutoPatentEnd", "Auto Patent End", SettingKeys.AutoEndPatent));
			rows.Add(new SettingToggleRow(3, "IncreaseWalkSpeed", "Increase Walk Speed", SettingKeys.IncreaseWalkSpeed));
			rows.Add(new SettingToggleRow(3, "DisableFurnitureStealing", "Disable Furniture Stealing", SettingKeys.DisableFurnitureStealing));
			rows.Add(new SettingToggleRow(4, "FreePrint", "Free Print", SettingKeys.FreePrint));
			rows.Add(new SettingToggleRow(4, "FreeWaterElectricity", "Free Water & Electricity", SettingKeys.NoWaterElectricity));
			rows.Add(new SettingToggleRow(4, "IncreaseBookshelfSkill", "Increase Bookshelf Skill", SettingKeys.IncreaseBookshelfSkill));
			rows.Add(new SettingToggleRow(4, "IncreaseCourierCapacity", "Increase Courier Capacity", SettingKeys.IncreaseCourierCapacity));
			rows.Add(new SettingToggleRow(4, "IncreasePrintSpeed", "Increase Print Speed", SettingKeys.IncreasePrintSpeed));
			rows.Add(new SettingToggleRow(4, "MoreHostingDeals", "More Hosting Deals", SettingKeys.MoreHostingDeals));
			rows.Add(new SettingToggleRow(4, "ReduceInternetCost", "Reduce Internet Cost", SettingKeys.ReduceISPCost));
			rows.Add(new SettingToggleRow(4, "NoServerCost", "No Server Cost", SettingKeys.NoServerCost));
			rows.Add(new SettingToggleRow(4, "ReduceExpansionCost", "Reduce Expansion Cost", SettingKeys.ReduceExpansionCost));
			rows.Add(new SettingToggleRow(4, "DisableForcePause", "Disable Force Pause", SettingKeys.DisableForcePause));
			rows.Add(new SettingToggleRow(4, "DisableForceFreeze", "Disable Force Freeze", SettingKeys.DisableForceFreeze));
			rows.Add(new SettingToggleRow(4, "AutoAcceptHostingDeals", "Auto Accept Hosting Deals", SettingKeys.AutoAcceptHostingDeals));
			rows.Add(new SettingToggleRow(4, "AutoPorting", "Auto Porting", SettingKeys.AutoPorting));
			rows.Add(new SettingToggleRow(4, "MoreInspiration", "More Inspiration", SettingKeys.MoreInspiration));
			rows.Add(new SettingToggleRow(4, "NoMissedSupportTickets", "No Missed Support Tickets", SettingKeys.NoMissedSupportTickets));
			rows.Add(new SettingToggleRow(4, "NoInsuranceCost", "No Insurance Cost", SettingKeys.NoInsuranceCost));
			rows.Add(new SettingToggleRow(4, "NoLoanInterest", "No Loan Interest", SettingKeys.NoLoanInterest));
			rows.Add(new SettingToggleRow(4, "FreeMarketing", "Free Marketing", SettingKeys.FreeMarketing));
			rows.Add(new SettingToggleRow(4, "NoOffshoreHeat", "No Offshore Heat", SettingKeys.NoOffshoreHeat));
			rows.Add(new SettingToggleRow(4, "NoFounderDividends", "No Founder Dividends", SettingKeys.NoFounderDividends));
			return rows;
		}
	}
}
