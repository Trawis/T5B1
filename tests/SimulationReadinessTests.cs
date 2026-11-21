using Xunit;

namespace Trainer_v5.Tests
{
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

		[Theory]
		[InlineData("OnHourPassed", (int)FeatureTrigger.Hour)]
		[InlineData("OnMinutePassed", (int)FeatureTrigger.Minute)]
		[InlineData("OnDayPassed", (int)FeatureTrigger.Day)]
		[InlineData("OnMonthPassed", (int)FeatureTrigger.Month)]
		public void EveryScheduledEntryPointChecksReadinessBeforeItDispatches(string handler, int trigger)
		{
			CompiledCalls.InOrder(typeof(TrainerBehaviour), handler, "call Helpers.IsSimulationReady", "ldc " + trigger, "call FeatureDispatcher.Run");
		}

		[Fact]
		public void UpdateKeepsTheHotkeysButGatesEverythingThatMutatesTheGame()
		{
			var type = typeof(TrainerBehaviour);

			CompiledCalls.InOrder(type, "Update", "call Main.OpenSettingsWindow", "call Main.CloseSettingsWindow", "call Helpers.IsSimulationReady");
			foreach (var gated in new[] { "ldstr LoadSpecializations", "ldstr ProcessStaticSettings", "call FeatureDispatcher.RunTransitions", "call GameMinuteWatcher.Poll", "call FeatureDispatcher.Run" })
			{
				CompiledCalls.InOrder(type, "Update", "call Helpers.IsSimulationReady", gated);
			}

			CompiledCalls.Adjacent(type, "Update", "ldc " + (int)FeatureTrigger.Frame, "call FeatureDispatcher.Run");
		}

		// The game rebuilds room meshes for floors -1..MaxFloor late in LoadGame; the trainer's raised limit must already be set.
		[Fact]
		public void MaxFloorIsSetOnceAndBeforeTheReadinessGate()
		{
			var type = typeof(TrainerBehaviour);

			CompiledCalls.InOrder(type, "Update", "call Helpers.get_IsGameLoaded", "ldstr MaxFloor", "ldftn TrainerBehaviour.ApplyMaxFloorOnce", "call Helpers.TryExecute", "call Helpers.IsSimulationReady");
			CompiledCalls.InOrder(type, "ApplyMaxFloorOnce", "ldfld TrainerBehaviour._oneTimeSettingsApplied", "call MultiplayerRule.get_Active", "stsfld GameSettings.MaxFloor", "stfld TrainerBehaviour._oneTimeSettingsApplied");
			CompiledCalls.NeverCalls(type, "ApplyProcessStaticSettings", "stsfld GameSettings.MaxFloor");
		}

		[Fact]
		public void GameReadyInitializationIsNotGatedOnTheSimulationBeingReady()
		{
			var type = typeof(TrainerBehaviour);

			CompiledCalls.NeverCalls(type, "OnGameReady", "call Helpers.IsSimulationReady");
			CompiledCalls.NeverCalls(type, "OnGameReady", "ldsfld SelectorController.DoneLoading");
			CompiledCalls.Calls(type, "OnGameReady", "call EmployeesWindowTrainer.Install");
			CompiledCalls.Calls(type, "OnGameReady", "ldstr LoadSpecializations");
		}
	}
}
