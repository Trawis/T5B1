using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Time events must be subscribed before any fallible UI installation on scene load (#338).
	public class StartupSubscriptionTests
	{
		[Fact]
		public void TheMainSceneSubscribesBeforeItCreatesOrInstallsAnyUi()
		{
			var branch = SourceCode.Span(SourceCode.Read("TrainerBehaviour.cs"), "case SceneRoute.MainScene:", "break;");

			SourceCode.AssertInOrder(branch, "SubscribeToEvents()", "Main.CreateUIButtons()", "the main scene must subscribe before creating UI");
			SourceCode.AssertInOrder(branch, "SubscribeToEvents()", "DetailWindowTrainer.Install()", "the main scene must subscribe before installing UI");
		}

		[Fact]
		public void SubscribingStaysGuardedAndLeavingStillUnsubscribes()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");
			var subscribe = SourceCode.Body(source, "private void SubscribeToEvents()");
			var leave = SourceCode.Body(source, "private void LeaveGame()");

			Assert.Contains("if (_timeEventsSubscribed)", subscribe);
			Assert.Contains("_timeEventsSubscribed = true;", subscribe);
			Assert.Contains("UnsubscribeFromEvents();", leave);
			Assert.Single(Regex.Matches(source, @"TimeOfDay\.OnHourPassed \+= "));
		}

		[Fact]
		public void AMissingUiElementIsReportedNotDereferenced()
		{
			var add = SourceCode.Body(SourceCode.Read("UIHelper.cs"), "public static bool AddToElement");

			SourceCode.AssertInOrder(add, "parent == null", "parent.gameObject", "the parent is checked before it is used");
			Assert.Contains(".Log()", add);
			Assert.DoesNotContain("catch", add);
		}

		[Fact]
		public void TheDetailControlsAreOnlyMarkedInstalledWhenTheyWerePlaced()
		{
			var source = SourceCode.Read("DetailWindowTrainer.cs");

			SourceCode.AssertInOrder(source, "if (!placed)", "_installedOn = target;", "the installed marker follows the placement check");
		}
	}
}
