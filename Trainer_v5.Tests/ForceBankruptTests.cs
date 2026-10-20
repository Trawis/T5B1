using Xunit;

namespace Trainer_v5.Tests
{
	// Bankruptcy is one-way, unambiguous, never the player's own company (#234).
	public class ForceBankruptTests
	{
		private static string Member(string signature)
		{
			return SourceCode.Body(SourceCode.Read("Features", "CompanyFeatures.cs"), signature);
		}

		[Fact]
		public void TheForceActionNeverTogglesTheFlagAndAsksBeforeSettingIt()
		{
			var body = Member("internal static void ForceBankruptAction");

			Assert.DoesNotContain("= !", body);
			Assert.Contains("matches.Count > 1", body);
			Assert.Contains("IsBankruptTarget(simCompany)", body);
			Assert.DoesNotContain("Bankrupt = true", body);
			SourceCode.AssertInOrder(body, "SpawnInputDialog", "ConfirmForceBankrupt", "the player is asked before the flag is set");
		}

		[Fact]
		public void BankruptAllLeavesThePlayersCompanyAlone()
		{
			var body = Member("internal static void AIBankrupt");

			Assert.Contains("Where(IsBankruptTarget)", body);
		}

		[Fact]
		public void TheEligibilityRuleSparesThePlayersCompanyAndItsSubsidiaries()
		{
			var body = Member("private static bool IsBankruptTarget");

			Assert.Contains("company != Settings.MyCompany", body);
			Assert.Contains("!company.IsPlayerOwned()", body);
		}

		[Fact]
		public void TheConfirmationChecksTheCompanyAgainBeforeSettingTheFlag()
		{
			var body = Member("private static void ConfirmForceBankrupt");

			Assert.Contains("Companies.TryGetValue(company.ID", body);
			Assert.Contains("IsBankruptTarget(company)", body);
			Assert.Contains("!company.Bankrupt", body);
			Assert.DoesNotContain("= !", body);
		}
	}
}
