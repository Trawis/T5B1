using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// The six employee editors are reached from the native Employees window (#478).
	public class EmployeesWindowToolbarTests
	{
		private static readonly string[] Labels = { "Trait", "Demand", "Inspiration", "LeadSpec", "Founder", "Skills" };

		[Fact]
		public void TheToolbarHasTheSixExistingActionsWithTheirExistingKeys()
		{
			var install = SourceCode.Body(SourceCode.Read("EmployeesWindowTrainer.cs"), "public static void Install()");

			foreach (var label in Labels)
			{
				Assert.Contains("UIFactory.Button(\"Detail" + label + "Button\".LocDef(\"" + label + "\")", install);
			}

			Assert.Equal(Labels.Length, Regex.Matches(install, @"UIFactory\.Button\(").Count);
		}

		[Fact]
		public void TheToolbarIsPlacedOnTheEmployeesWindowAboveItsTopEdgeNotOnTheDetailWindow()
		{
			var source = SourceCode.Read("EmployeesWindowTrainer.cs");
			var install = SourceCode.Body(source, "public static void Install()");

			Assert.Contains("HUD.Instance?.employeeWindow", install);
			Assert.Contains("DetailButtonLayout.Place(", install);
			Assert.Contains("PlaceInElement(components[i].gameObject, target.Window.gameObject, slots[i])", install);
			Assert.DoesNotContain("DetailWindow", source);
			Assert.DoesNotContain("ActorWindow", source);
			Assert.DoesNotContain("AddToElement", source);
		}

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

				Assert.Contains("EmployeeTarget.TryResolveFor(", source);
				Assert.DoesNotContain("DetailWindow", source);
				Assert.DoesNotContain("HUD.Instance", source);
			}
		}

		[Fact]
		public void TheNewMessageIsLocalizedInEnglish()
		{
			var english = File.ReadAllText(Path.Combine(RepositoryFiles.Root, "Trainer_v5", "Trainer.Localization", "English", "Trainer.tyd"));

			Assert.Contains("SelectOneEmployee\t", english);
		}
	}
}
