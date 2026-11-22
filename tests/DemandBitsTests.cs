using System.Linq;
using Trainer_v5.SDK;
using Xunit;

namespace Trainer_v5.Tests
{
	// Lead demands: the first demand must also mark the choice as met so the native window shows it.
	public class DemandBitsTests
	{
		private const uint Rate = 1, NonBinding = 2, Choice = 0x10;

		[Fact]
		public void TheFirstDemandSetsTheResultBitAndMarksTheChoiceMet()
		{
			var next = DemandBits.Apply(new DemandBits(0, 0, Choice), Rate, NonBinding, Choice, true);

			Assert.Equal(Rate, next.Results);
			Assert.Equal(Choice, next.Met);
			Assert.Equal(0u, next.Requested);
		}

		[Fact]
		public void GrantingTheOtherChoiceOfAPairReplacesTheFirst()
		{
			var next = DemandBits.Apply(new DemandBits(Rate, Choice, 0), NonBinding, Rate, Choice, true);

			Assert.Equal(NonBinding, next.Results);
			Assert.Equal(Choice, next.Met);
		}

		[Fact]
		public void RemovingTheDemandClearsTheChoiceUnlessTheOtherChoiceIsSet()
		{
			var removed = DemandBits.Apply(new DemandBits(Rate, Choice | 0x20, 0), Rate, NonBinding, Choice, false);
			Assert.Equal(0u, removed.Results);
			Assert.Equal(0x20u, removed.Met);

			var otherStays = DemandBits.Apply(new DemandBits(Rate | NonBinding, Choice, 0), Rate, NonBinding, Choice, false);
			Assert.Equal(NonBinding, otherStays.Results);
			Assert.Equal(Choice, otherStays.Met);
		}
	}
}
