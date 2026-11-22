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
			Assert.Contains("() => AICompanyChange(company), () => ConfirmForceBankrupt(company));", Member("internal static void AskForceBankrupt"));
		}

		[Fact]
		public void BankruptAllLeavesThePlayersCompanyAlone()
		{
			var body = Member("private static List<SimulatedCompany> FindAllBankruptTargets");

			Assert.Contains("IsBankruptTarget(company)", body);
			Assert.Contains("!company.Bankrupt", body);
			Assert.Contains("IsSameTargets(companies, FindAllBankruptTargets())", Member("internal static void AIBankrupt"));
		}

		[Theory]
		[InlineData(false, false, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		public void TheEligibilityRuleSparesThePlayersCompanyAndItsSubsidiaries(bool isPlayersCompany, bool isPlayerOwned, bool expected)
		{
			Assert.Equal(expected, CompanyFeatures.IsBankruptEligible(isPlayersCompany, isPlayerOwned));
			CompiledCalls.InOrder(typeof(CompanyFeatures), "IsBankruptTarget", "ldfld GameSettings.MyCompany", "call Company.IsPlayerOwned", "call CompanyFeatures.IsBankruptEligible");
		}

		[Fact]
		public void TheConfirmationChecksTheCompanyAgainBeforeSettingTheFlag()
		{
			var change = Member("private static string AICompanyChange(");
			var live = Member("internal static bool IsLiveAICompany(");

			Assert.Contains("IsLiveAICompany(company)", change);
			Assert.Contains("company.Bankrupt ?", change);
			Assert.Contains("Companies.TryGetValue(company.ID", live);
			Assert.Contains("IsBankruptTarget(company)", live);
			Assert.DoesNotContain("= !", Member("private static string ConfirmForceBankrupt("));
		}
	}
}
