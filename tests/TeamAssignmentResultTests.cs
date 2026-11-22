using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class TeamAssignmentResultTests
	{
		[Fact]
		public void AppliedOnlyWhenTheItemHoldsExactlyTheExpectedTeam()
		{
			Assert.True(TeamAssignmentResult.Applied(new HashSet<string> { "B" }, "B"));
			Assert.False(TeamAssignmentResult.Applied(new HashSet<string> { "A" }, "B"));
			Assert.False(TeamAssignmentResult.Applied(new HashSet<string> { "A", "B" }, "B"));
			Assert.False(TeamAssignmentResult.Applied(new HashSet<string>(), "B"));
			Assert.False(TeamAssignmentResult.Applied(null, "B"));
		}

		[Theory]
		[InlineData(0, 0, (int)ResultSeverity.None)]
		[InlineData(2, 0, (int)ResultSeverity.Success)]
		[InlineData(2, 1, (int)ResultSeverity.Partial)]
		[InlineData(0, 1, (int)ResultSeverity.Failed)]
		public void AFailureNeverReadsAsPlainSuccess(int changed, int failed, int expected)
		{
			Assert.Equal((ResultSeverity)expected, TeamAssignmentResult.Severity(changed, failed));
		}

		[Fact]
		public void ALongPreviewIsCutAndSaysHowManyLinesWereLeftOut()
		{
			var lines = new[] { "a", "b", "c" };
			int omitted;

			Assert.Equal("a\nb\nc", TeamAssignmentResult.PreviewText(lines, 3, out omitted));
			Assert.Equal(0, omitted);
			Assert.Equal("a\nb", TeamAssignmentResult.PreviewText(lines, 2, out omitted));
			Assert.Equal(1, omitted);
		}

		[Fact]
		public void OptimizerCountsFromTheItemStateAfterTheChange()
		{
			CompiledCalls.InOrder(typeof(WorkItemFeatures), "ApplyTeamRecommendations", "call WorkItem.SetDevTeams", "call TeamAssignmentResult.Applied", "call TeamAssignmentResult.Severity");
		}
	}
}
