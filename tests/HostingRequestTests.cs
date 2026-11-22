using Xunit;

namespace Trainer_v5.Tests
{
	public class HostingRequestTests
	{
		[Fact]
		public void OnlyARequestWithNoBidderAndTheDeveloperAsClientIsLegacy()
		{
			Assert.True(MarketFeatures.IsLegacyRequestDeal(true, false, true, false));
		}

		[Theory]
		[InlineData(false, false, true, false)]
		[InlineData(true, false, true, true)]
		public void NativeAndAmbiguousDealsAreNotLegacy(bool request, bool hasBidder, bool clientIsDeveloper, bool clientIsPlayer)
		{
			Assert.False(MarketFeatures.IsLegacyRequestDeal(request, hasBidder, clientIsDeveloper, clientIsPlayer));
		}

		[Fact]
		public void TheRepairOnlyClearsTheRequestFlagOfDealsTheDetectionAccepts()
		{
			var steps = CompiledCalls.Steps(typeof(MarketFeatures), "RepairLegacyHostingRequests");

			CompiledCalls.InOrder(typeof(MarketFeatures), "RepairLegacyHostingRequests", "call MarketFeatures.IsLegacyRequestDeal", "ldc 0", "stfld Deal.Request");
			Assert.DoesNotContain(steps, step => step.Contains("PerPower") || step.Contains("Accepted"));
		}
	}
}
