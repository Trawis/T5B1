using Xunit;

namespace Trainer_v5.Tests
{
	// More Hosting Deals builds offers the way the game's own simulation does (#199).
	public class HostingRequestTests
	{
		private static string PushDealBody()
		{
			return SourceCode.Body(SourceCode.Read("Features", "MarketFeatures.cs"), "internal static void PushDeal");
		}

		[Fact]
		public void TheDealIsNotMarkedAsARequestAfterConstruction()
		{
			var body = PushDealBody();

			Assert.Contains("new ServerDeal(Products[index])", body);
			Assert.DoesNotContain("Request = true", body);
		}

		[Fact]
		public void CandidatesFollowTheGamesOwnEligibilityRules()
		{
			var body = PushDealBody();

			Assert.Contains("IsSubsidiary()", body);
			Assert.Contains("MaxHostedProductAgeMonths", body);
		}
	}
}
