using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class StartupSubscriptionTests
	{
		[Fact]
		public void TheMainSceneSubscribesBeforeItCreatesOrInstallsAnyUi()
		{
			var branch = SourceCode.Span(SourceCode.Read("TrainerBehaviour.cs"), "case SceneRoute.MainScene:", "break;");

			SourceCode.AssertInOrder(branch, "SubscribeToEvents()", "Main.CreateUIButtons()", "the main scene must subscribe before creating UI");
			SourceCode.AssertInOrder(branch, "SubscribeToEvents()", "EmployeesWindowTrainer.Install()", "the main scene must subscribe before installing UI");
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
	}
}
