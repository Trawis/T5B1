using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	// The team optimizer reports the post-action state instead of a generic success.
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

		[Fact]
		public void ZeroChangesWithoutFailuresIsANoOp()
		{
			Assert.True(TeamAssignmentResult.ChangedNothing(0, 0));
			Assert.False(TeamAssignmentResult.ChangedNothing(1, 0));
			Assert.False(TeamAssignmentResult.ChangedNothing(0, 2));
			Assert.False(TeamAssignmentResult.ChangedNothing(3, 1));
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
		public void OptimizerCountsFromTheItemStateAndNeverReportsPlainSuccessAfterAFailure()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "private static void ApplyTeamRecommendations");

			Assert.Contains("TeamAssignmentResult.Applied(item.DevTeams, bestTeam.Name)", body);
			SourceCode.AssertInOrder(body, "item.SetDevTeams(", "TeamAssignmentResult.Applied(", "state is read after the change");
			SourceCode.AssertInOrder(body, "TeamAssignmentResult.ChangedNothing(", "Notification.Popup(", "a no-op returns before the success popup");
			Assert.Contains("failed == 0", body);
			Assert.Contains("Notification.ShowWarning(summary)", body);
			Assert.Contains("Notification.ShowError(summary)", body);
			Assert.Equal(1, body.Split(new[] { "Notification.Popup(" }, System.StringSplitOptions.None).Length - 1);
		}
	}
}
