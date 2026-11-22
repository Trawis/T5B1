using Xunit;

namespace Trainer_v5.Tests
{
	// Which employee an editor acts on. Liveness checks need game objects and are covered in-game.
	public class EmployeeTargetTests
	{
		[Theory]
		[InlineData(1, "Selection")]
		[InlineData(2, "Ambiguous")]
		public void SeveralSelectedEmployeesAreNeverGuessed(int selected, string expected)
		{
			Assert.Equal(expected, EmployeeTarget.Decide(selected).ToString());
		}

		[Fact]
		public void OnlyExactlyOneSelectedEmployeeBecomesATarget()
		{
			CompiledCalls.InOrder(typeof(EmployeeTarget), "Resolve", "call EmployeeTarget.Decide", "ldc 0", "call List`1.get_Item", "call EmployeeTarget..ctor");
			CompiledCalls.Count(typeof(EmployeeTarget), "Resolve", "call List`1.get_Item", 1);
		}
	}
}
