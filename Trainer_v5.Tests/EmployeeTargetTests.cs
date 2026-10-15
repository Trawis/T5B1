using Xunit;

namespace Trainer_v5.Tests
{
	// Which employee an editor acts on (#235). Liveness checks need game objects and are covered in-game.
	public class EmployeeTargetTests
	{
		[Theory]
		[InlineData(true, 0, "DetailWindow")]
		[InlineData(true, 1, "DetailWindow")]
		[InlineData(true, 5, "DetailWindow")]
		[InlineData(false, 1, "Selection")]
		[InlineData(false, 2, "Ambiguous")]
		[InlineData(false, 9, "Ambiguous")]
		[InlineData(false, 0, "None")]
		public void TheOpenDetailWindowWinsAndSeveralSelectedEmployeesAreNeverGuessed(bool detail, int selected, string expected)
		{
			Assert.Equal(expected, EmployeeTarget.Decide(detail, selected).ToString());
		}
	}
}
