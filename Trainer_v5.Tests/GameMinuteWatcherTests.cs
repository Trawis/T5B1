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

		[Fact]
		public void ObservingWithoutSubscribersDoesNotThrow()
		{
			var watcher = new GameMinuteWatcher();

			watcher.Observe(5);
			watcher.Observe(6);
		}
	}
}
