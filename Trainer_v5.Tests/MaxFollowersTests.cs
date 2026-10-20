using Xunit;

namespace Trainer_v5.Tests
{
	// Max Followers fills the game's own cap once; it adds no arbitrary amounts (#232).
	public class MaxFollowersTests
	{
		[Fact]
		public void TheActionRecomputesTheCapAndFillsItWithoutAddingBillions()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "internal static void MaxFollowersAction");

			Assert.Contains("ReEvaluateMaxFollowers()", body);
			Assert.Contains("Followers = WorkItem.MaxFollowers", body);
			Assert.DoesNotContain("1000000000", body);
			Assert.DoesNotContain("FollowerChange", body);
			Assert.DoesNotContain("+=", body);
		}
	}
}
