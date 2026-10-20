using System.Collections.Generic;
using UnityEngine.Events;

namespace Trainer_v5
{
	// Not generic: the game's source-mod compiler emits invalid IL for fields of user-defined generic types.
	internal sealed class SettingActionRow
	{
		public readonly SettingsTab Tab;
		public readonly TrainerDomain Domain;
		public readonly string LocKey;
		public readonly string DefaultText;
		public readonly UnityAction Action;

		public SettingActionRow(SettingsTab tab, TrainerDomain domain, string locKey, string defaultText, UnityAction action)
		{
			Tab = tab;
			Domain = domain;
			LocKey = locKey;
			DefaultText = defaultText;
			Action = action;
		}
	}

	// Global actions only, in display order per domain. Built in a method (see Workshop Source Build).
	internal static class SettingActionRows
	{
		public static List<SettingActionRow> Create()
		{
			var rows = new List<SettingActionRow>();
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "AddMoney", "Add Money", CompanyFeatures.IncreaseMoney));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "AddAIFunds", "Add AI Funds", CompanyFeatures.AddAIFunds));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "TakeoverCompany", "Takeover Company", CompanyFeatures.TakeoverCompany));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "SubsidiaryCompany", "Subsidiary Company", CompanyFeatures.SubDCompany));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "Bankrupt", "Bankrupt", CompanyFeatures.ForceBankrupt));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "BankruptAll", "AI Bankrupt All", CompanyFeatures.AIBankrupt));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "ClearAllLoans", "Clear all loans", CompanyFeatures.ClearLoans));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "TransferOffshoreFunds", "Transfer Offshore Funds", CompanyFeatures.TransferOffshoreFunds));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "MaxReputation", "Max Reputation", CompanyFeatures.MaxReputation));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "DaysPerMonth", "Days per month", CompanyFeatures.MonthDays));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Company, "UnlockAndClaimRewards", "Unlock and Claim Rewards", CompanyFeatures.UnlockAndClaimAllRewards));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Employee, "MaxSkill", "Max skill of employees", EmployeeFeatures.EmployeesToMax));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Employee, "ResetAge", "Reset age of employees", EmployeeFeatures.ResetAgeOfEmployees));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "SetProductPrice", "Set Product Price", ProductFeatures.SetProductPrice));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "SetProductStock", "Set Product Stock", ProductFeatures.SetProductStock));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "SetActiveUsers", "Set Active Users", ProductFeatures.AddActiveUsers));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "MaxFollowers", "Max Followers", WorkItemFeatures.MaxFollowers));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "MaxMarketRecognition", "Max Market Recognition", CompanyFeatures.MaxMarketRecognition));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "InstantMarketing", "Instant Marketing", MarketFeatures.InstantMarketing));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Product, "SellProductsStock", "Sell products stock", ProductFeatures.SellProductStock));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Work, "FixBugs", "Fix Bugs", WorkItemFeatures.FixBugs));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Work, "InstantResearch", "Instant Research", WorkItemFeatures.InstantResearch));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Work, "InstantPorting", "Instant Porting", WorkItemFeatures.InstantPorting));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Work, "OptimizeTeamAssignments", "Optimize Team Assignments", WorkItemFeatures.OptimizeTeamAssignments));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Work, "ExtendDeadline", "Extend Deadline", WorkItemFeatures.ExtendDeadline));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Environment, "UnlockAllFurniture", "Unlock all furniture", CompanyFeatures.UnlockFurniture));
			rows.Add(new SettingActionRow(SettingsTab.Actions, TrainerDomain.Environment, "UnlockAllSpace", "Unlock all space", CompanyFeatures.UnlockAllSpace));
			rows.Add(new SettingActionRow(SettingsTab.Settings, TrainerDomain.None, "Discord", "DISCORD", () => Helpers.ShowDiscordInvite()));
			return rows;
		}
	}
}
