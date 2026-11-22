using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SkipTimeTests
	{
		[Theory]
		[InlineData(22, 8, 10)]
		[InlineData(23, 0, 1)]
		[InlineData(14, 14, 0)]
		public void HoursUntilCountsForwardToTheNextMatchingHour(int current, int target, int expected)
		{
			Assert.Equal(expected, SkipTime.HoursUntil(current, target));
		}
	}
}
