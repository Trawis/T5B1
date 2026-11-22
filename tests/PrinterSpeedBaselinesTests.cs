using Xunit;

namespace Trainer_v5.Tests
{
	public class PrinterSpeedBaselinesTests
	{
		[Fact]
		public void TheFirstCapturedSpeedIsTheBaseline()
		{
			var baselines = new PrinterSpeedBaselines();

			Assert.Equal(1.5f, baselines.Capture(10, 1.5f));
			Assert.Equal(1.5f, baselines.Capture(10, 2f));
		}

		[Fact]
		public void ATakenBaselineIsGoneSoTheNextEnableCapturesAgain()
		{
			var baselines = new PrinterSpeedBaselines();
			baselines.Capture(1, 1f);

			float baseline;
			Assert.True(baselines.TryTake(1, out baseline));
			Assert.False(baselines.TryTake(1, out baseline));
			Assert.Equal(4f, baselines.Capture(1, 4f));
		}

		[Fact]
		public void RetainDropsPrintersThatAreNoLongerLive()
		{
			var baselines = new PrinterSpeedBaselines();
			baselines.Capture(1, 1f);
			baselines.Capture(2, 2f);
			baselines.Capture(3, 3f);

			baselines.Retain(new System.Collections.Generic.HashSet<int> { 2 });

			float baseline;
			Assert.Equal(1, baselines.Count);
			Assert.False(baselines.TryTake(1, out baseline));
			Assert.True(baselines.TryTake(2, out baseline));
			Assert.False(baselines.TryTake(3, out baseline));
		}
	}
}
