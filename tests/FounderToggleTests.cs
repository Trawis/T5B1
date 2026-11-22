using Xunit;

namespace Trainer_v5.Tests
{
	public class FounderToggleTests
	{
		[Fact]
		public void TheToggleChecksAgainAfterTheConfirmationAndOnlyFlipsTheFlagOnTheExistingEmployee()
		{
			CompiledCalls.InOrder(typeof(FounderToggle), "Start", "call FounderToggle.FindChange", "call FounderToggle.Apply", "call Confirmation.AskAndApply");
			CompiledCalls.Calls(typeof(FounderToggle), "FindChange", "call FounderToggle.FindBlocker");
			CompiledCalls.OnlyIn("stfld Employee.Founder", "FounderToggle.Apply");
		}
	}
}
