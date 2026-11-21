using System.IO;
using System.Text.RegularExpressions;
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

		// Source shape: a new path that files patents or finishes research early must go through the guarded methods.
		[Theory]
		[InlineData(@"\.PatentNow\(\)", "private static PatentOutcome TryPatentNow")]
		[InlineData(@"\bFinished = true", "private static bool TryFinishResearchNow", "private static bool AdvanceSoftwarePort")]
		[InlineData(@"\bCompleteResearchWork\b", "private static bool TryFinishResearchNow", "internal static void ApplyAutoEndResearch", "private static void CompleteResearchWork")]
		public void GuardedCallsAppearOnlyInTheirSharedPath(string pattern, params string[] allowedMembers)
		{
			var guarded = SourceCode.Read("Features", "WorkItemFeatures.cs");
			var expected = 0;
			foreach (var member in allowedMembers)
			{
				var body = SourceCode.Body(guarded, member);
				Assert.Matches(pattern, body);
				expected += Regex.Matches(body, pattern).Count;
			}

			var found = 0;
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				found += Regex.Matches(SourceCode.Normalize(File.ReadAllText(file)), pattern).Count;
			}

			Assert.True(expected == found, "Only " + string.Join(", ", allowedMembers) + " may match " + pattern + "; found " + found + " matches, " + expected + " allowed.");
		}
	}
}
