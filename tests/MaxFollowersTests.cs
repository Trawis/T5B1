using Xunit;

namespace Trainer_v5.Tests
{
	public class MaxFollowersTests
	{
		[Theory]
		[InlineData(499.5f, 500u, true)]
		[InlineData(500f, 500u, false)]
		[InlineData(800f, 500u, false)]
		public void AutoMaxFollowersFillsOnlyBelowTheCap(float followers, uint maxFollowers, bool expected)
		{
			Assert.Equal(expected, WorkItemFeatures.IsBelowFollowerCap(followers, maxFollowers));
		}
	}
}
