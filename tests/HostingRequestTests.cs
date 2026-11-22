using Xunit;

namespace Trainer_v5.Tests
{
	public class HostingRequestTests
	{
		private static string PushDealBody()
		{
			return SourceCode.Body(SourceCode.Read("Features", "MarketFeatures.cs"), "internal static void PushDeal");
		}

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
			var body = SourceCode.Body(SourceCode.Read("Features", "MarketFeatures.cs"), "internal static int RepairLegacyHostingRequests");

			Assert.Contains("IsLegacyRequestDeal(", body);
			Assert.Contains("deal.Request = false", body);
			Assert.DoesNotContain("PerPower", body);
			Assert.DoesNotContain("Accepted", body);
		}
	}
}
