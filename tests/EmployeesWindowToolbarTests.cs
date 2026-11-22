using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	public class EmployeesWindowToolbarTests
	{
		[Fact]
		public void SingleEmployeeActionsNeedExactlyOneSelectedEmployeeAndCheckAgainAfterTheirDialog()
		{
			var inspiration = SourceCode.Body(SourceCode.Read("EmployeesWindowTrainer.cs"), "private static void SetInspiration()");
			var founder = SourceCode.Body(SourceCode.Read("FounderToggle.cs"), "public static void Start()");

			Assert.Contains("EmployeeTarget.RequireOne()", inspiration);
			SourceCode.AssertInOrder(inspiration, "RequestFloat(", "target.IsStillValid()", "the target is checked again in the callback");
			SourceCode.AssertInOrder(inspiration, "target.IsStillValid()", "Inspiration = val", "the target is checked before the write");
			Assert.Contains("EmployeeTarget.RequireOne()", founder);
			Assert.DoesNotContain("EmployeeTarget.Resolve(", founder);
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
