namespace Trainer_v5
{
	// Creativity has no setter, so the editor replaces the Employee object (#183). Game collections keyed by or
	// holding the old object are not migrated, so the edit is refused while one of them still holds it.
	internal static class CreativityEditGuard
	{
		public static bool IsEnabled()
		{
			return Helpers.GetProperty(Helpers.Settings, SettingKeys.Experimental) || Helpers.IsDebug;
		}

		// A reason the employee must not be replaced, or null when no known reference would go stale.
		public static string FindBlockingReference(Employee employee)
		{
			var company = GameSettings.Instance != null ? GameSettings.Instance.MyCompany : null;
			if (company != null && company.WorkItems != null)
			{
				foreach (var item in company.WorkItems)
				{
					var work = item as SoftwareWorkItem;
					if (work == null)
					{
						continue;
					}

					if (work.LeadWork != null && work.LeadWork.ContainsKey(employee))
					{
						return "has lead work in progress on a work item";
					}

					if (work.NewWorking != null && work.NewWorking.ContainsKey(employee))
					{
						return "is working on a feature of a work item";
					}
				}
			}

			var market = MarketSimulation.Active;
			if (market != null && market.FreeLeads != null && market.FreeLeads.Contains(employee))
			{
				return "is listed as a free lead in the market simulation";
			}

			return null;
		}
	}
}
