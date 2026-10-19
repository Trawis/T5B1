using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Deal types are selected by type, never by the text of ToString() (#246).
	public class HostingRewardTypeTests
	{
		private const string TypeText = @"(?:ToString\(\)|GetType\(\)\s*\.\s*(?:Name|FullName|ToString\(\)))";
		private const string DealLiteral = "@?\"[^\"]*Deal[^\"]*\"";

		// Any "...Deal..." literal compared with the text of a type or value, in either operand order.
		private static readonly Regex DealSelectedByText = new Regex(
			TypeText + @"\s*(?:==|!=|\.\s*Equals\s*\(|\.\s*Contains\s*\()\s*" + DealLiteral +
			"|" + DealLiteral + @"\s*(?:==|!=)\s*[\w.\s()]*?" + TypeText +
			"|" + DealLiteral + @"\s*\.\s*Equals\s*\([\w.\s()]*?" + TypeText);

		[Theory]
		[InlineData("x.ToString() == \"ServerDeal\"")]
		[InlineData("x.ToString() != \"ServerDeal\"")]
		[InlineData("\"ServerDeal\" == deal.ToString()")]
		[InlineData("deal.GetType().Name == \"ServerDeal\"")]
		[InlineData("deal.GetType().FullName == \"ServerDeal\"")]
		[InlineData("\"ServerDeal\".Equals(deal.GetType().Name)")]
		[InlineData("deal.ToString().Equals(\"ServerDeal\")")]
		public void TheScanRecognisesEverySpellingOfSelectingADealByText(string code)
		{
			Assert.Matches(DealSelectedByText, code);
		}

		[Fact]
		public void TheScanIgnoresSelectionByTypeAndComments()
		{
			var code = SourceText.Strip("deals.OfType<ServerDeal>(); // x.ToString() == \"ServerDeal\"", true);

			Assert.DoesNotMatch(DealSelectedByText, code);
		}

		[Fact]
		public void NoFeatureSelectsDealsByToString()
		{
			var scanned = 0;
			foreach (var path in Directory.GetFiles(Path.Combine(RepositoryFiles.SourceDirectory, "Features"), "*.cs"))
			{
				var source = SourceText.Strip(File.ReadAllText(path), true);
				scanned++;

				Assert.False(DealSelectedByText.IsMatch(source), Path.GetFileName(path) + " selects a deal by the text of its type.");
			}

			Assert.True(scanned > 0);
		}

		[Fact]
		public void HostingFeaturesSelectDealsByType()
		{
			var source = SourceText.Strip(File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "MarketFeatures.cs")), true);

			Assert.Contains("OfType<ServerDeal>()", source);
		}
	}
}
