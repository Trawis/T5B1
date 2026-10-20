using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// One-shot actions say when they did nothing and refuse invalid targets (#371).
	public class OneShotActionFeedbackTests
	{
		private static string Between(string file, string startMarker, string endMarker)
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", file));
			var start = source.IndexOf(startMarker, System.StringComparison.Ordinal);
			var end = source.IndexOf(endMarker, start, System.StringComparison.Ordinal);
			return source.Substring(start, end - start);
		}

		[Fact]
		public void SellingStockReportsOnlyAfterTheSaleWasCounted()
		{
			var body = Between("ProductFeatures.cs", "internal static void SellProductStock", "internal static void SetProductPriceAction");

			Assert.True(body.IndexOf("ChangeAllPhysicalStock", System.StringComparison.Ordinal) < body.IndexOf("SpawnDialog", System.StringComparison.Ordinal));
			Assert.True(body.IndexOf("MakeTransaction", System.StringComparison.Ordinal) < body.IndexOf("SpawnDialog", System.StringComparison.Ordinal));
			Assert.Contains("StockSale.Summary(", body);
		}

		[Fact]
		public void SubsidiaryReportsAnUnknownNameAndRefusesThePlayersOwnCompany()
		{
			var body = Between("CompanyFeatures.cs", "internal static void SubDCompanyAction", "internal static void SubDCompany()");

			Assert.True(body.IndexOf("Company == null", System.StringComparison.Ordinal) < body.IndexOf("SpawnDialog", System.StringComparison.Ordinal));
			Assert.True(body.IndexOf("!IsBankruptTarget(Company)", System.StringComparison.Ordinal) < body.IndexOf("MakeSubsidiary", System.StringComparison.Ordinal));
			Assert.Equal(2, body.Split(new[] { "SpawnDialog" }, System.StringSplitOptions.None).Length - 1);
		}

		[Fact]
		public void FixBugsReportsAProductThatIsNotInBeta()
		{
			var body = Between("WorkItemFeatures.cs", "internal static void FixBugsAction", "internal static void FixBugs()");

			Assert.True(body.IndexOf("WorkItem == null", System.StringComparison.Ordinal) < body.IndexOf("SpawnDialog", System.StringComparison.Ordinal));
			Assert.True(body.IndexOf("SpawnDialog", System.StringComparison.Ordinal) < body.IndexOf("FixedBugs =", System.StringComparison.Ordinal));
		}
	}
}
