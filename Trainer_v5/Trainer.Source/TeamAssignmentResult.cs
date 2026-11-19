using System.Collections.Generic;

namespace Trainer_v5
{
	internal static class TeamAssignmentResult
	{
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

		public static bool ChangedNothing(int changed, int failed)
		{
			return changed == 0 && failed == 0;
		}
	}
}
