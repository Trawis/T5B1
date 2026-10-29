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

		// #506: the guard runs first, so an invalid launch creates, toggles, focuses, refreshes and resets nothing.
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
		public void SkillsNeedsAtLeastOneEmployeeBeforeAnyWindowOperationOrBulkReset()
		{
			var show = SourceCode.Body(SourceCode.Read("Window", "EmployeeSkillChangeWindow.cs"), "public static void Show()");

			Assert.StartsWith("public static void Show() { if (!EmployeeTarget.RequireAny())", show);
			foreach (var operation in new[] { "CreateWindow()", "Window.Toggle()", "_allSelected.isOn = false", "Refresh()" })
			{
				SourceCode.AssertInOrder(show, "EmployeeTarget.RequireAny()", operation, "the guard precedes " + operation);
			}

			Assert.DoesNotContain("RequireOne", show);
			Assert.DoesNotContain("isOn = true", show);
		}

		[Fact]
		public void SkillsKeepsItsFreshBulkChoiceOnEveryValidOpening()
		{
			var show = SourceCode.Body(SourceCode.Read("Window", "EmployeeSkillChangeWindow.cs"), "public static void Show()");

			SourceCode.AssertInOrder(show, "Window.Toggle()", "_allSelected.isOn = false", "every valid opening resets bulk to off");
			SourceCode.AssertInOrder(show, "_allSelected.isOn = false", "Refresh()", "then the scope follows the selection");
		}

		[Theory]
		[InlineData(0, false, false)]
		[InlineData(1, true, true)]
		[InlineData(2, false, true)]
		[InlineData(9, false, true)]
		public void ZeroOneAndSeveralSelectedEmployeesDecideWhichEditorsMayOpen(int liveSelected, bool singleEditorsOpen, bool skillsOpens)
		{
			Assert.Equal(singleEditorsOpen, EmployeeTarget.Decide(liveSelected) == TargetSource.Selection);
			Assert.Equal(skillsOpens, EmployeeTarget.HasAny(liveSelected));
		}

		[Fact]
		public void TheLaunchGuardsUseTheMessagesAlreadyDefinedForThem()
		{
			var target = SourceCode.Read("EmployeeTarget.cs");

			Assert.Contains("SharedMessages.SelectOneEmployee()", SourceCode.Body(target, "public static EmployeeTarget RequireOne()"));
			Assert.Contains("SharedMessages.SelectEmployeesError()", SourceCode.Body(target, "public static bool RequireAny()"));
			Assert.Contains("ResolveSelection()", SourceCode.Body(target, "public static bool RequireAny()"));
			Assert.DoesNotContain("SelectEmployeesError\".LocDef", SourceCode.Read("Window", "EmployeeSkillChangeWindow.cs"));
		}

		[Fact]
		public void TheLaunchFixLeavesTheRefreshResolverAndTheInspirationAndFounderGuardsAlone()
		{
			var target = SourceCode.Read("EmployeeTarget.cs");
			var tryResolve = SourceCode.Body(target, "public static bool TryResolveFor(");

			Assert.Contains("window == null || !window.Shown", tryResolve);
			Assert.DoesNotContain("RequireOne", tryResolve);
			Assert.DoesNotContain("RequireAny", tryResolve);
			Assert.Contains("EmployeeTarget.RequireOne()", SourceCode.Body(SourceCode.Read("EmployeesWindowTrainer.cs"), "private static void SetInspiration()"));
			Assert.Contains("EmployeeTarget.RequireOne()", SourceCode.Body(SourceCode.Read("FounderToggle.cs"), "public static void Start()"));
		}
	}
}
