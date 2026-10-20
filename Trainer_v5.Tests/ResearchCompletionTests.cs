using Xunit;

namespace Trainer_v5.Tests
{
	// Instant Research and Auto Research End must finish research the way ResearchWork.FinishNow does (#231).
	public class ResearchCompletionTests
	{
		private static string CompletionBody()
		{
			return SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "private static void CompleteResearchWork");
		}

		[Fact]
		public void ThePatentTeamKeyIsTheEnumNameNotItsNumber()
		{
			var body = CompletionBody();

			Assert.DoesNotContain("(int)legalWork.Type", body);
			Assert.Contains("legalWork.Type + \"Team\"", body);
		}

		[Fact]
		public void PatentWorkIsRegisteredThroughTheCompanyAndTheResearchListRefreshed()
		{
			var body = CompletionBody();

			Assert.Contains("MyCompany.AddWorkItem(legalWork)", body);
			Assert.DoesNotContain("WorkItems.Add(", body);
			Assert.Contains("researchWindow.UpdateLists()", body);
		}
	}
}
