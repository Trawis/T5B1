using Xunit;

namespace Trainer_v5.Tests
{
	// Granting a demand uses the game's AcceptDemand so its effect runs, without the follow-up demand request.
	public class DemandGrantTests
	{
		[Fact]
		public void GrantingCallsAcceptDemandWithoutTheRefreshFlagAndNeverRefundsTheCost()
		{
			var type = typeof(Trainer_v5.Window.EmployeeDemandChangeWindow);

			CompiledCalls.Adjacent(type, "GrantDemand", "ldc 0", "call Employee.AcceptDemand");
			CompiledCalls.NeverCalls(type, "GrantDemand", "call Company.MakeTransaction");
		}
	}
}
