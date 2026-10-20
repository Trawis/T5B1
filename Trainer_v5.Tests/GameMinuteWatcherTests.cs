using System;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	public class GameMinuteWatcherTests
	{
		private static int CountEvents(GameMinuteWatcher watcher, params int[] minutes)
		{
			var count = 0;
			watcher.OnMinutePassed += (sender, args) => count++;
			foreach (var minute in minutes)
			{
				watcher.Observe(minute);
			}
			return count;
		}

		[Fact]
		public void TheFirstObservationFires()
		{
			Assert.Equal(1, CountEvents(new GameMinuteWatcher(), 30));
		}

		[Fact]
		public void RepeatedObservationsOfTheSameMinuteFireOnce()
		{
			Assert.Equal(1, CountEvents(new GameMinuteWatcher(), 30, 30, 30));
		}

		[Fact]
		public void EachNewMinuteFires()
		{
			Assert.Equal(3, CountEvents(new GameMinuteWatcher(), 30, 31, 32));
		}

		[Fact]
		public void TheHourRolloverFiresOnceWhenTheMinuteWrapsAround()
		{
			Assert.Equal(3, CountEvents(new GameMinuteWatcher(), 58, 59, 0));
		}

		[Fact]
		public void ResetMakesTheNextObservationFireEvenForTheSameMinute()
		{
			var watcher = new GameMinuteWatcher();
			var count = 0;
			watcher.OnMinutePassed += (sender, args) => count++;

			watcher.Observe(12);
			watcher.Observe(12);
			watcher.Reset();
			watcher.Observe(12);

			Assert.Equal(2, count);
		}

		// TimeOfDay cannot be touched in the test host, so the readiness check is verified on the source (#356).
		[Fact]
		public void PollTreatsAMissingTimeOfDayAsNotReadyBeforeReadingIt()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "GameMinuteWatcher.cs"));
			var poll = source.Substring(source.IndexOf("public void Poll()", StringComparison.Ordinal));
			poll = poll.Substring(0, poll.IndexOf("public void Reset()", StringComparison.Ordinal));

			var check = poll.IndexOf("TimeOfDay.Instance == null", StringComparison.Ordinal);
			Assert.True(check >= 0);
			Assert.True(check < poll.IndexOf("TimeOfDay.Instance.Minute", StringComparison.Ordinal));
			Assert.True(poll.IndexOf("return;", StringComparison.Ordinal) < poll.IndexOf("Observe(", StringComparison.Ordinal));
		}

		// TrainerBehaviour needs Unity, so the frame boundaries are checked on the source (#356).
		[Fact]
		public void TheSharedFrameStepsAreEachGuardedAndPrecedeTheFrameRun()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "TrainerBehaviour.cs"));
			var update = source.Substring(source.IndexOf("private void Update()", StringComparison.Ordinal));
			update = update.Substring(0, update.IndexOf("private void ApplyProcessStaticSettings()", StringComparison.Ordinal));

			var settings = update.IndexOf("Helpers.TryExecute(\"ProcessStaticSettings\", () => ApplyProcessStaticSettings())", StringComparison.Ordinal);
			var transitions = update.IndexOf("Helpers.TryExecute(\"ToggleTransitions\", () => GetDispatcher().RunTransitions())", StringComparison.Ordinal);
			var poll = update.IndexOf("Helpers.TryExecute(\"MinutePoll\", () => _minuteWatcher.Poll())", StringComparison.Ordinal);
			var frame = update.IndexOf("GetDispatcher().Run(FeatureTrigger.Frame)", StringComparison.Ordinal);

			Assert.True(settings >= 0 && settings < transitions);
			Assert.True(transitions < poll);
			Assert.True(poll < frame);
		}
	}
}
