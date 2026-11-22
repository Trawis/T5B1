using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	public class EmployeesWindowToolbarTests
	{
		// The input callback is a lambda; its body is read where it is created, before RequestFloat.
		[Fact]
		public void SingleEmployeeActionsNeedExactlyOneSelectedEmployeeAndCheckAgainAfterTheirDialog()
		{
			CompiledCalls.InOrder(typeof(EmployeesWindowTrainer), "SetInspiration", "call EmployeeTarget.RequireOne", "call EmployeeTarget.IsStillValid", "stfld Employee.Inspiration", "call InputHelper.RequestFloat");
			CompiledCalls.Calls(typeof(FounderToggle), "Start", "call EmployeeTarget.RequireOne");
			CompiledCalls.NeverCalls(typeof(FounderToggle), "Start", "call EmployeeTarget.Resolve");
		}

		[Theory]
		[InlineData(1, true)]
		[InlineData(2, false)]
		public void OnlyExactlyOneSelectedEmployeeLetsTheSingleEmployeeEditorsOpen(int liveSelected, bool opens)
		{
			Assert.Equal(opens, EmployeeTarget.Decide(liveSelected) == TargetSource.Selection);
		}
	}
}
