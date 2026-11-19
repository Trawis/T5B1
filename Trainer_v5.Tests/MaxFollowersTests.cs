using Xunit;

namespace Trainer_v5.Tests
{
	// Max Followers fills the game's own cap once; it adds no arbitrary amounts.
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

		[Theory]
		[InlineData(0f, 0u, false)]
		[InlineData(0f, 500u, true)]
		[InlineData(499.5f, 500u, true)]
		[InlineData(500f, 500u, false)]
		[InlineData(800f, 500u, false)]
		public void AutoMaxFollowersFillsOnlyBelowTheCap(float followers, uint maxFollowers, bool expected)
		{
			Assert.Equal(expected, WorkItemFeatures.IsBelowFollowerCap(followers, maxFollowers));
		}
	}
}
