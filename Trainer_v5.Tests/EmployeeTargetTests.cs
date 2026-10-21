using Xunit;

namespace Trainer_v5.Tests
{
	// Which employee an editor acts on (#235, #478). Liveness checks need game objects and are covered in-game.
	public class EmployeeTargetTests
	{
		[Theory]
		[InlineData(0, "None")]
		[InlineData(1, "Selection")]
		[InlineData(2, "Ambiguous")]
		[InlineData(9, "Ambiguous")]
		public void SeveralSelectedEmployeesAreNeverGuessed(int selected, string expected)
		{
			Assert.Equal(expected, EmployeeTarget.Decide(selected).ToString());
		}

		[Fact]
		public void OnlyTheEmployeesWindowListDecidesAndNoOtherWindowOrWorldSelectionCanOverrideIt()
		{
			var source = SourceCode.Read("EmployeeTarget.cs");
			var selected = SourceCode.Body(source, "private static List<Actor> LiveSelectedActors()");

			Assert.DoesNotContain("DetailWindow", source);
			Assert.DoesNotContain("SelectorController", source);
			Assert.DoesNotContain("CurrentEmployee", source);
			Assert.Contains("HUD.Instance.employeeWindow", selected);
			Assert.Contains("EmployeeList.GetSelected<Actor>()", selected);
			SourceCode.AssertInOrder(selected, "Window.Shown", "GetSelected<Actor>()", "a closed Employees window has no selection");
			SourceCode.AssertInOrder(selected, "IsLive(actor)", "selected.Add(actor)", "dismissed employees are never targets");
			Assert.Contains("IsInActiveGame(actor)", selected);
		}

		[Fact]
		public void OnlyExactlyOneSelectedEmployeeBecomesATarget()
		{
			var resolve = SourceCode.Body(SourceCode.Read("EmployeeTarget.cs"), "public static EmployeeTarget Resolve(");

			Assert.Contains("source == TargetSource.Selection ? new EmployeeTarget(selected[0]) : null", resolve);
			Assert.DoesNotContain("selected[1]", resolve);
		}

		[Fact]
		public void ANoSelectionOrSeveralSelectedActionTellsThePlayerAndDoesNothing()
		{
			var require = SourceCode.Body(SourceCode.Read("EmployeeTarget.cs"), "public static EmployeeTarget RequireOne()");

			Assert.Contains("SharedMessages.SelectOneEmployee()", require);
			SourceCode.AssertInOrder(require, "Notification.ShowError(", "return target;", "the player is told before the empty target is returned");
		}
	}
}
