using Xunit;

namespace Trainer_v5.Tests
{
	public class PatentResearchGuardTests
	{
		[Theory]
		[InlineData(false, false, (int)PatentOutcome.NotPatentable)]
		[InlineData(false, true, (int)PatentOutcome.NotPatentable)]
		[InlineData(true, false, (int)PatentOutcome.Unaffordable)]
		[InlineData(true, true, (int)PatentOutcome.Patented)]
		public void APatentIsFiledOnlyWhenPatentableAndAffordable(bool patentable, bool affordable, int expected)
		{
			Assert.Equal((PatentOutcome)expected, WorkItemFeatures.DecidePatent(patentable, affordable));
		}

		[Theory]
		[InlineData(2000, 2000, 5, false)]
		[InlineData(2005, 2000, 5, false)]
		[InlineData(2006, 2000, 5, true)]
		public void APatentExpiresAfterTheExpirationYears(int year, int patentYear, int expiration, bool expected)
		{
			Assert.Equal(expected, WorkItemFeatures.IsPatentExpired(year, patentYear, expiration));
		}

		[Theory]
		[InlineData(1990, -1, 1990, false)]
		[InlineData(1985, 1980, 1990, false)]
		[InlineData(1995, -1, 1990, true)]
		[InlineData(1990, 1990, 1990, true)]
		[InlineData(1990, 1995, 1990, true)]
		public void ResearchIsObsoleteWhenTheMarketOrTheCompanyIsPastItsYear(int latestMarketYear, int latestCompanyYear, int researchYear, bool expected)
		{
			Assert.Equal(expected, WorkItemFeatures.IsResearchObsolete(latestMarketYear, latestCompanyYear, researchYear));
		}

		// A new path that files patents or finishes research early must go through the guarded methods.
		[Fact]
		public void GuardedCallsAppearOnlyInTheirSharedPath()
		{
			CompiledCalls.OnlyIn("call LegalWork.PatentNow", "WorkItemFeatures.TryPatentNow");
			CompiledCalls.OnlyIn("stfld ResearchWork.Finished", "WorkItemFeatures.TryFinishResearchNow");
			CompiledCalls.OnlyIn("call WorkItemFeatures.CompleteResearchWork", "WorkItemFeatures.TryFinishResearchNow");
			CompiledCalls.OnlyIn("ldftn WorkItemFeatures.CompleteResearchWork", "WorkItemFeatures.ApplyAutoEndResearch");
		}
	}
}
