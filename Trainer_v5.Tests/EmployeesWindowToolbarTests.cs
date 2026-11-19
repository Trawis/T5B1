using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	public class EmployeesWindowToolbarTests
	{
		[Fact]
		public void InstallingTwiceOnTheSameWindowOrWithoutAWindowAddsNothing()
		{
			var install = SourceCode.Body(SourceCode.Read("EmployeesWindowTrainer.cs"), "public static void Install()");

			SourceCode.AssertInOrder(install, "target == null", "UIFactory.Button(", "nothing is created without a window");
			SourceCode.AssertInOrder(install, "target == _installedOn", "UIFactory.Button(", "nothing is created twice for one window");
			Assert.Contains("target.Window == null", install);
			SourceCode.AssertInOrder(install, "if (!placed) return;", "_installedOn = target;", "the marker follows the placement check");
		}

		[Fact]
		public void TheToolbarIsRebuiltOnEachNewGameAndNoDetailWindowLauncherRemains()
		{
			var behaviour = SourceCode.Read("TrainerBehaviour.cs");

			Assert.Contains("EmployeesWindowTrainer.Reset();", SourceCode.Body(behaviour, "private void LeaveGame()"));
			Assert.Contains("EmployeesWindowTrainer.Install();", SourceCode.Body(behaviour, "private void OnGameReady("));
			Assert.False(File.Exists(Path.Combine(RepositoryFiles.SourceDirectory, "DetailWindowTrainer.cs")));
			Assert.DoesNotContain("DetailWindowTrainer", behaviour);
		}

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

		[Fact]
		public void TheEditorsResolveTheirEmployeeOnlyThroughEmployeeTarget()
		{
			foreach (var file in new[] { "EmployeeTraitChangeWindow.cs", "EmployeeDemandChangeWindow.cs", "EmployeeLeadSpecChangeWindow.cs", "EmployeeSkillChangeWindow.cs" })
			{
				var source = SourceCode.Read("Window", file);

				Assert.Contains(file == "EmployeeSkillChangeWindow.cs" ? "EmployeeTarget.ResolveSelection(" : "EmployeeTarget.TryResolveFor(", source);
				Assert.DoesNotContain("DetailWindow", source);
				Assert.DoesNotContain("HUD.Instance", source);
			}
		}

		[Theory]
		[InlineData("EmployeeTraitChangeWindow.cs", "public void Show()")]
		[InlineData("EmployeeDemandChangeWindow.cs", "public void Show()")]
		[InlineData("EmployeeLeadSpecChangeWindow.cs", "public void Show()")]
		public void TheSingleEmployeeEditorsRequireExactlyOneEmployeeBeforeAnyWindowOperation(string file, string signature)
		{
			var show = SourceCode.Body(SourceCode.Read("Window", file), signature);

			Assert.StartsWith(signature + " { if (EmployeeTarget.RequireOne() == null)", show);
			foreach (var windowOperation in new[] { "CreateWindow()", "_window.Toggle()", "Refresh()" })
			{
				SourceCode.AssertInOrder(show, "EmployeeTarget.RequireOne()", windowOperation, "the guard precedes " + windowOperation);
			}
		}

		[Fact]
		public void SkillsNeedsAtLeastOneEmployeeBeforeAnyWindowOperation()
		{
			var show = SourceCode.Body(SourceCode.Read("Window", "EmployeeSkillChangeWindow.cs"), "public static void Show()");

			Assert.StartsWith("public static void Show() { if (!EmployeeTarget.RequireAny())", show);
			foreach (var operation in new[] { "CreateWindow()", "Window.Toggle()", "Refresh()" })
			{
				SourceCode.AssertInOrder(show, "EmployeeTarget.RequireAny()", operation, "the guard precedes " + operation);
			}

			Assert.DoesNotContain("RequireOne", show);
			Assert.DoesNotContain("isOn = true", show);
		}

		[Theory]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(2, false)]
		[InlineData(9, false)]
		public void OnlyExactlyOneSelectedEmployeeLetsTheSingleEmployeeEditorsOpen(int liveSelected, bool opens)
		{
			Assert.Equal(opens, EmployeeTarget.Decide(liveSelected) == TargetSource.Selection);
		}
	}
}
