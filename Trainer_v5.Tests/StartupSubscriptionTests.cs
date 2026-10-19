using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Time events must be subscribed before any fallible UI installation on scene load (#338).
	public class StartupSubscriptionTests
	{
		private static string Read(string file)
		{
			return File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, file));
		}

		[Fact]
		public void TheMainSceneSubscribesBeforeItCreatesOrInstallsAnyUi()
		{
			var source = Read("TrainerBehaviour.cs");
			var start = source.IndexOf("case SceneRoute.MainScene:", System.StringComparison.Ordinal);
			var branch = source.Substring(start, source.IndexOf("break;", start, System.StringComparison.Ordinal) - start);

			var subscribe = branch.IndexOf("SubscribeToEvents()", System.StringComparison.Ordinal);
			Assert.True(subscribe >= 0, "the main scene must subscribe");
			Assert.True(subscribe < branch.IndexOf("Main.CreateUIButtons()", System.StringComparison.Ordinal));
			Assert.True(subscribe < branch.IndexOf("DetailWindowTrainer.Install()", System.StringComparison.Ordinal));
		}

		[Fact]
		public void SubscribingStaysGuardedAndLeavingStillUnsubscribes()
		{
			var source = Read("TrainerBehaviour.cs");
			var subscribe = source.Substring(source.IndexOf("private void SubscribeToEvents()", System.StringComparison.Ordinal));
			subscribe = subscribe.Substring(0, subscribe.IndexOf("private void UnsubscribeFromEvents()", System.StringComparison.Ordinal));
			var leave = source.Substring(source.IndexOf("private void LeaveGame()", System.StringComparison.Ordinal));
			leave = leave.Substring(0, leave.IndexOf("private void SubscribeToSceneEvents()", System.StringComparison.Ordinal));

			Assert.Contains("if (_timeEventsSubscribed)", subscribe);
			Assert.Contains("_timeEventsSubscribed = true;", subscribe);
			Assert.Contains("UnsubscribeFromEvents();", leave);
			Assert.Single(Regex.Matches(source, @"TimeOfDay\.OnHourPassed \+= "));
		}

		[Fact]
		public void AMissingUiElementIsReportedNotDereferenced()
		{
			var source = Read("UIHelper.cs");
			var add = source.Substring(source.IndexOf("public static bool AddToElement", System.StringComparison.Ordinal));
			add = add.Substring(0, add.IndexOf("public static void AddToWindow", System.StringComparison.Ordinal));

			Assert.True(add.IndexOf("parent == null", System.StringComparison.Ordinal) < add.IndexOf("parent.gameObject", System.StringComparison.Ordinal));
			Assert.Contains(".Log()", add);
			Assert.DoesNotContain("catch", add);
		}

		[Fact]
		public void TheDetailControlsAreOnlyMarkedInstalledWhenTheyWerePlaced()
		{
			var source = Read("DetailWindowTrainer.cs");

			Assert.True(source.IndexOf("if (!placed)", System.StringComparison.Ordinal) < source.IndexOf("_installedOn = target;", System.StringComparison.Ordinal));
		}
	}
}
