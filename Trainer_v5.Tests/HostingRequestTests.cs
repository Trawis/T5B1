using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// More Hosting Deals builds offers the way the game's own simulation does (#199).
	public class HostingRequestTests
	{
		private static string PushDealBody()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "MarketFeatures.cs"));
			var start = source.IndexOf("internal static void PushDeal", System.StringComparison.Ordinal);
			return source.Substring(start, source.IndexOf("internal static void ApplyHostingDealTiming", start, System.StringComparison.Ordinal) - start);
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
