using Xunit;

namespace Trainer_v5.Tests
{
	// MarketingPlan.Progress has one slot per enabled press option, so all seven option subsets matter (#196).
	public class PressReleaseProgressTests
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public void EverySlotIsCompletedWhateverTheOptionCombination(int slots)
		{
			var progress = new float[slots];

			MarketFeatures.MarkAllProgressComplete(progress);

			Assert.All(progress, value => Assert.Equal(1f, value));
		}

		[Fact]
		public void AnEmptyOrMissingArrayIsLeftAlone()
		{
			MarketFeatures.MarkAllProgressComplete(new float[0]);
			MarketFeatures.MarkAllProgressComplete(null);
		}
	}
}
