using Xunit;

namespace Trainer_v5.Tests
{
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
			Assert.DoesNotContain("YES", body);
			Assert.Contains("AskForceBankrupt(matches[0])", body);
			SourceCode.AssertInOrder(Member("internal static void AskForceBankrupt"), "Confirmation.Ask(", "ConfirmForceBankrupt", "the player is asked before the flag is set");
		}

		[Fact]
		public void BankruptAllLeavesThePlayersCompanyAlone()
		{
			var body = Member("private static List<SimulatedCompany> FindAllBankruptTargets");

			Assert.Contains("IsBankruptTarget(company)", body);
			Assert.Contains("!company.Bankrupt", body);
			Assert.Contains("FindAllBankruptTargets()", Member("private static void ApplyAIBankrupt"));
			Assert.Contains("FindAllBankruptTargets()", Member("internal static void AIBankrupt"));
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
			var body = Member("private static void ConfirmForceBankrupt(");
			var live = Member("internal static bool IsLiveAICompany(");

			Assert.Contains("IsLiveAICompany(company)", body);
			Assert.Contains("Companies.TryGetValue(company.ID", live);
			Assert.Contains("IsBankruptTarget(company)", live);
			Assert.Contains("!company.Bankrupt", body);
			Assert.DoesNotContain("= !", body);
			Assert.DoesNotContain("answer", body);
		}
	}
}
