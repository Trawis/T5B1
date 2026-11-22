using Xunit;

namespace Trainer_v5.Tests
{
	public class ForceBankruptTests
	{

		[Fact]
		public void TheForceActionNeverTogglesTheFlagAndAsksBeforeSettingIt()
		{
			var type = typeof(CompanyFeatures);

			CompiledCalls.Calls(type, "ForceBankruptAction", "call CompanyFeatures.IsBankruptTarget");
			CompiledCalls.InOrder(type, "ForceBankruptAction", "call SharedMessages.CompanyNameAmbiguous", "call CompanyFeatures.AskForceBankrupt");
			CompiledCalls.NeverCalls(type, "ForceBankruptAction", "stfld Company.Bankrupt");
			CompiledCalls.InOrder(type, "AskForceBankrupt", "call CompanyFeatures.AICompanyChange", "call CompanyFeatures.ConfirmForceBankrupt", "call Confirmation.AskAndApply");
		}

		[Fact]
		public void BankruptAllLeavesThePlayersCompanyAlone()
		{
			var type = typeof(CompanyFeatures);

			CompiledCalls.InOrder(type, "FindAllBankruptTargets", "call CompanyFeatures.IsBankruptTarget", "ldfld Company.Bankrupt");
			CompiledCalls.InOrder(type, "AIBankrupt", "call CompanyFeatures.FindAllBankruptTargets", "call CompanyFeatures.FindAllBankruptTargets", "call Confirmation.IsSameTargets");
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
			var type = typeof(CompanyFeatures);

			CompiledCalls.InOrder(type, "AICompanyChange", "call CompanyFeatures.IsLiveAICompany", "ldfld Company.Bankrupt");
			CompiledCalls.InOrder(type, "IsLiveAICompany", "ldfld Company.ID", "call Dictionary`2.TryGetValue", "call CompanyFeatures.IsBankruptTarget");
			CompiledCalls.Adjacent(type, "ConfirmForceBankrupt", "ldc 1", "stfld Company.Bankrupt");
		}
	}
}
