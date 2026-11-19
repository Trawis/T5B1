namespace Trainer_v5
{
	internal static class SharedMessages
	{
		public static string TeamAssignmentsNoChange(int skipped)
		{
			return string.Format("TeamAssignmentsNoChange".LocDef("Trainer: No team assignment was changed. {0} skipped."), skipped);
		}

		public static string CompanyNotFound(string name)
		{
			return string.Format("CompanyNotFound".LocDef("Trainer: Company {0} not found!"), name);
		}

		public static string CompanyNameAmbiguous(string name)
		{
			return string.Format("CompanyNameAmbiguous".LocDef("Trainer: Company name {0} is ambiguous. Rename the company or use a unique name."), name);
		}

		public static string CompanyNamePrompt()
		{
			return "CompanyNamePrompt".LocDef("Type company name:");
		}

		public static string ProductNamePrompt()
		{
			return "ProductNamePrompt".LocDef("Type product name:");
		}

		public static string CompanyAlreadyBankrupt(string name)
		{
			return string.Format("CompanyAlreadyBankrupt".LocDef("Trainer: {0} is already going bankrupt."), name);
		}

		public static string AdjustAIFundsTitle()
		{
			return "AdjustAIFundsTitle".LocDef("Adjust AI Funds");
		}

		public static string EditorRefreshButton()
		{
			return "EditorRefreshButton".LocDef("Refresh");
		}

		public static string EditSkillsTitle(string target)
		{
			return string.Format("EditSkillsTitle".LocDef("Edit skills for {0}"), target);
		}

		public static string EditTraitsTitle(string target)
		{
			return string.Format("EditTraitsTitle".LocDef("Edit traits for {0}"), target);
		}

		public static string EditDemandsTitle(string target)
		{
			return string.Format("EditDemandsTitle".LocDef("Edit demands for {0}"), target);
		}

		public static string EditLeadSpecTitle(string target)
		{
			return string.Format("EditLeadSpecTitle".LocDef("Edit lead specialization for {0}"), target);
		}

		public static string SelectEmployeeFirst()
		{
			return "SelectEmployeeFirst".LocDef("Select an employee first (select one employee in the Employees window, then close and reopen this window).");
		}

		public static string SelectEmployeesError()
		{
			return "SelectEmployeesError".LocDef("Select one or more employees.");
		}

		public static string SelectOneEmployee()
		{
			return "SelectOneEmployee".LocDef("Select exactly one employee in the Employees window for this action.");
		}

		public static string EmployeeChangedNothingEdited()
		{
			return "EmployeeChangedNothingEdited".LocDef("The employee changed or is not available. Nothing was edited.");
		}

		public static string ProductChangedNothingEdited()
		{
			return "ProductChangedNothingEdited".LocDef("The product is no longer available. Nothing was changed.");
		}

		public static string NotAvailableInMultiplayer()
		{
			return "NotAvailableInMultiplayer".LocDef("Not available in multiplayer.");
		}

		public static string EmployeeChangedPressRefresh()
		{
			return "EmployeeChangedPressRefresh".LocDef("The employee changed or is not available. Press Refresh.");
		}
	}
}
