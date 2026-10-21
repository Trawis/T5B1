namespace Trainer_v5
{
	// Texts looked up from more than one place (guarded by LocalizationTests). The key and its English fallback stay
	// together as one literal pair so the localization scans see them. Single-use texts stay inline at their call site.
	internal static class SharedMessages
	{
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

		public static string AddAIFundsTitle()
		{
			return "AddAIFundsTitle".LocDef("Add AI Funds");
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

		public static string SelectOneEmployee()
		{
			return "SelectOneEmployee".LocDef("Select exactly one employee in the Employees window for this action.");
		}

		public static string EmployeeChangedNothingEdited()
		{
			return "EmployeeChangedNothingEdited".LocDef("The employee changed or is not available. Nothing was edited.");
		}

		public static string EmployeeChangedPressRefresh()
		{
			return "EmployeeChangedPressRefresh".LocDef("The employee changed or is not available. Press Refresh.");
		}
	}
}
