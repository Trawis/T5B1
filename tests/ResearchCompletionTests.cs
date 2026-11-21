using Xunit;

namespace Trainer_v5.Tests
{
	// Instant Research and Auto Research End must finish research the way ResearchWork.FinishNow does.
	public class ResearchCompletionTests
	{
		// The enum name ("PatentTeam"), not its number: ToString on the enum, then "Team".
		[Fact]
		public void ThePatentTeamKeyIsTheEnumNameNotItsNumber()
		{
			CompiledCalls.Adjacent(typeof(WorkItemFeatures), "CompleteResearchWork", "call Object.ToString", "ldstr Team", "call String.Concat", "call GameSettings.ApplyDefaultTeams");
			CompiledCalls.NeverCalls(typeof(WorkItemFeatures), "CompleteResearchWork", "call Int32.ToString");
		}

		[Fact]
		public void PatentWorkIsRegisteredThroughTheCompanyAndTheResearchListRefreshed()
		{
			CompiledCalls.InOrder(typeof(WorkItemFeatures), "CompleteResearchWork", "call LegalWork..ctor", "call Company.AddWorkItem", "call ResearchWindow.UpdateLists");
			CompiledCalls.NeverCalls(typeof(WorkItemFeatures), "CompleteResearchWork", "call List`1.Add");
		}
	}
}
