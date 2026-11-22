using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	public class QuietFireTests
	{
		[Theory]
		[InlineData(12f, 5f, true)]
		[InlineData(24f, 1f, true)]
		[InlineData(12.5f, 1.01f, false)]
		public void ALawsuitIsImpossibleUnlessEmployedOverAYearWithComplaintsAboveOne(float months, float complaintLevel, bool expected)
		{
			Assert.Equal(expected, QuietFire.LawsuitImpossible(months, complaintLevel));
		}
	}
}
