using System.Collections.Generic;

namespace Trainer_v5
{
	internal static class TeamAssignmentResult
	{
		// SetDevTeams returns nothing and silently ignores finished work, so the work item itself is the evidence.
		public static bool Applied(ICollection<string> teamsAfter, string expectedTeam)
		{
			return teamsAfter != null && teamsAfter.Count == 1 && teamsAfter.Contains(expectedTeam);
		}

		public static bool ChangedNothing(int changed, int failed)
		{
			return changed == 0 && failed == 0;
		}
	}
}
