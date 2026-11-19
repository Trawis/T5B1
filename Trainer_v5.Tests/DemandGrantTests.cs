using Xunit;

namespace Trainer_v5.Tests
{
	public class DemandGrantTests
	{
		[Fact]
		public void GrantingCallsAcceptDemandWithoutTheRefreshFlagAndNeverRefundsTheCost()
		{
			var body = SourceCode.Body(SourceCode.Read("Window", "EmployeeDemandChangeWindow.cs"), "private static void GrantDemand");

			Assert.Contains("employee.AcceptDemand(choice, index, false)", body);
			Assert.DoesNotContain("MakeTransaction", body);
		}

		[Fact]
		public void AnAlreadyGrantedDemandIsNeverAcceptedTwice()
		{
			var source = SourceCode.Read("Window", "EmployeeDemandChangeWindow.cs");

			Assert.Contains("if (on && !has)", source);
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"GrantDemand\(employee, choice"));
		}
	}
}
