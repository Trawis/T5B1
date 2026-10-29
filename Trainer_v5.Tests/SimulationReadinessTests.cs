using Xunit;

namespace Trainer_v5.Tests
{
	// #494: scheduled work waits for loading and pre-simulation; GameReady initialization does not.
	public class SimulationReadinessTests
	{
		[Fact]
		public void OnlyAFullyLoadedRunningGameIsReady()
		{
			Assert.True(Helpers.SimulationReady(true, true, true, true, false, false, true));
		}

		[Theory]
		[InlineData(false, true, true, true, false, false, true)]
		[InlineData(true, false, true, true, false, false, true)]
		[InlineData(true, true, false, true, false, false, true)]
		[InlineData(true, true, true, false, false, false, true)]
		[InlineData(true, true, true, true, true, false, true)]
		[InlineData(true, true, true, true, false, true, true)]
		[InlineData(true, true, true, true, false, false, false)]
		[InlineData(false, false, false, false, false, false, false)]
		public void AnyMissingDependencyLoadingOrPreSimulationMeansNotReady(bool game, bool hud, bool selector, bool done, bool preSim, bool preSimFinished, bool clock)
		{
			Assert.False(Helpers.SimulationReady(game, hud, selector, done, preSim, preSimFinished, clock));
		}

		[Fact]
		public void TheReadinessReadsTheGamesOwnLoadingAndPreSimulationFlags()
		{
			var body = SourceCode.Body(SourceCode.Read("Helpers.cs"), "internal static bool IsSimulationReady(");

			Assert.Contains("selector.DoneLoading", body);
			Assert.Contains("game.PreSimActive", body);
			Assert.Contains("game.PreSimFinished", body);
			Assert.Contains("TimeOfDay.Instance != null", body);
			Assert.Contains("HUD.Instance != null", body);
		}

		[Fact]
		public void EveryScheduledEntryPointChecksReadinessBeforeItDispatches()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");

			foreach (var pair in new[] { new[] { "private void OnHourPassed(", "FeatureTrigger.Hour" }, new[] { "private void OnMinutePassed(", "FeatureTrigger.Minute" },
				new[] { "private void OnDayPassed(", "FeatureTrigger.Day" }, new[] { "private void OnMonthPassed(", "FeatureTrigger.Month" } })
			{
				var body = SourceCode.Body(source, pair[0]);
				SourceCode.AssertInOrder(body, "if (!Helpers.IsSimulationReady())", "GetDispatcher().Run(" + pair[1] + ");", pair[0]);
			}
		}

		[Fact]
		public void UpdateKeepsTheHotkeysButGatesEverythingThatMutatesTheGame()
		{
			var body = SourceCode.Body(SourceCode.Read("TrainerBehaviour.cs"), "private void Update(");

			SourceCode.AssertInOrder(body, "KeyCode.F1", "if (!Helpers.IsSimulationReady())", "hotkeys stay available while loading");
			SourceCode.AssertInOrder(body, "KeyCode.F2", "if (!Helpers.IsSimulationReady())", "hotkeys stay available while loading");
			foreach (var gated in new[] { "\"LoadSpecializations\"", "\"ProcessStaticSettings\"", "RunTransitions()", "_minuteWatcher.Poll()", "GetDispatcher().Run(FeatureTrigger.Frame);" })
			{
				SourceCode.AssertInOrder(body, "if (!Helpers.IsSimulationReady())", gated, "gated: " + gated);
			}
		}

		// The game rebuilds room meshes for floors -1..MaxFloor late in LoadGame; the trainer's raised limit must already be set.
		[Fact]
		public void MaxFloorIsSetOnceAndBeforeTheReadinessGate()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");
			var update = SourceCode.Body(source, "private void Update(");

			SourceCode.AssertInOrder(update, "!Helpers.IsGameLoaded", "Helpers.TryExecute(\"MaxFloor\", ApplyMaxFloorOnce);", "only once a game exists");
			SourceCode.AssertInOrder(update, "Helpers.TryExecute(\"MaxFloor\", ApplyMaxFloorOnce);", "if (!Helpers.IsSimulationReady())", "before the gate");

			var once = SourceCode.Body(source, "private void ApplyMaxFloorOnce(");
			SourceCode.AssertInOrder(once, "if (!_oneTimeSettingsApplied)", "GameSettings.MaxFloor = Constants.MAX_FLOOR;", "guarded by the one-time flag");
			SourceCode.AssertInOrder(once, "GameSettings.MaxFloor = Constants.MAX_FLOOR;", "_oneTimeSettingsApplied = true;", "flag set after the push");
			Assert.DoesNotContain("MaxFloor", SourceCode.Body(source, "private void ApplyProcessStaticSettings("));
		}

		[Fact]
		public void OnlyTheMaxFloorStepRunsBeforeTheGate()
		{
			var update = SourceCode.Body(SourceCode.Read("TrainerBehaviour.cs"), "private void Update(");
			var beforeGate = update.Substring(0, update.IndexOf("if (!Helpers.IsSimulationReady())", System.StringComparison.Ordinal));

			foreach (var gated in new[] { "ProcessStaticSettings", "RunTransitions", "Poll()", "FeatureTrigger.", "LoadSpecializations", "ShowDiscordInvite" })
			{
				Assert.DoesNotContain(gated, beforeGate);
			}
		}

		[Fact]
		public void GameReadyInitializationIsNotGatedOnTheSimulationBeingReady()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");
			var ready = SourceCode.Body(source, "private void OnGameReady(");

			Assert.DoesNotContain("IsSimulationReady", ready);
			Assert.DoesNotContain("DoneLoading", ready);
			Assert.Contains("EmployeesWindowTrainer.Install();", ready);
			Assert.Contains("\"LoadSpecializations\"", ready);
		}
	}
}
