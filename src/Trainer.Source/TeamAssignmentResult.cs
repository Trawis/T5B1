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

		public static string PreviewText(IList<string> lines, int maxLines, out int omitted)
		{
			omitted = System.Math.Max(0, lines.Count - maxLines);
			var shown = new List<string>();
			for (int i = 0; i < lines.Count - omitted; i++)
			{
				shown.Add(lines[i]);
			}

			return string.Join("\n", shown);
		}

		// None: nothing changed and nothing failed. A failure never reads as plain success.
		public static ResultSeverity Severity(int changed, int failed)
		{
			if (failed == 0)
			{
				return changed == 0 ? ResultSeverity.None : ResultSeverity.Success;
			}

			return changed > 0 ? ResultSeverity.Partial : ResultSeverity.Failed;
		}
	}
}
