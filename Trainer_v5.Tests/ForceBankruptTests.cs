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
			Assert.Contains("IsBankruptTarget(simCompany)", body);
			Assert.DoesNotContain("Bankrupt = true", body);
			Assert.True(body.IndexOf("SpawnInputDialog", System.StringComparison.Ordinal) < body.IndexOf("ConfirmForceBankrupt", System.StringComparison.Ordinal));
		}

		[Fact]
		public void BankruptAllLeavesThePlayersCompanyAlone()
		{
			var body = Between("internal static void AIBankrupt", "internal static void UnlockAllSpace");

			Assert.Contains("Where(IsBankruptTarget)", body);
		}

		[Fact]
		public void TheEligibilityRuleSparesThePlayersCompanyAndItsSubsidiaries()
		{
			var body = Between("private static bool IsBankruptTarget", "private static void ConfirmForceBankrupt");

			Assert.Contains("company != Settings.MyCompany", body);
			Assert.Contains("!company.IsPlayerOwned()", body);
		}

		[Fact]
		public void TheConfirmationChecksTheCompanyAgainBeforeSettingTheFlag()
		{
			var body = Between("private static void ConfirmForceBankrupt", "private static float GetDefaultISPCostFactor");

			Assert.Contains("Companies.TryGetValue(company.ID", body);
			Assert.Contains("IsBankruptTarget(company)", body);
			Assert.Contains("!company.Bankrupt", body);
			Assert.DoesNotContain("= !", body);
		}
	}
}
