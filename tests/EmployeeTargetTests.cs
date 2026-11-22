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
			var resolve = SourceCode.Body(SourceCode.Read("EmployeeTarget.cs"), "public static EmployeeTarget Resolve(");

			Assert.Contains("source == TargetSource.Selection ? new EmployeeTarget(selected[0]) : null", resolve);
			Assert.DoesNotContain("selected[1]", resolve);
		}
	}
}
