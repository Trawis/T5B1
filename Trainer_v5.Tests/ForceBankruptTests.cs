using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Bankruptcy is one-way, unambiguous, never the player's own company (#234).
	public class ForceBankruptTests
	{
		private static string Between(string startMarker, string endMarker)
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "CompanyFeatures.cs"));
			var start = source.IndexOf(startMarker, System.StringComparison.Ordinal);
			var end = source.IndexOf(endMarker, start, System.StringComparison.Ordinal);
			return source.Substring(start, end - start);
		}

		[Fact]
		public void TheForceActionNeverTogglesTheFlagAndAsksBeforeSettingIt()
		{
			var body = Between("internal static void ForceBankruptAction", "internal static void ForceBankrupt()");

			Assert.DoesNotContain("= !", body);
			Assert.Contains("matches.Count > 1", body);
			Assert.Contains("simCompany != Settings.MyCompany", body);
			Assert.Contains("Bankrupt = true", body);
			Assert.True(body.IndexOf("SpawnInputDialog", System.StringComparison.Ordinal) < body.IndexOf("Bankrupt = true", System.StringComparison.Ordinal));
		}

		[Fact]
		public void BankruptAllLeavesThePlayersCompanyAlone()
		{
			var body = Between("internal static void AIBankrupt", "internal static void UnlockAllSpace");

			Assert.Contains("company != Settings.MyCompany", body);
		}
	}
}
