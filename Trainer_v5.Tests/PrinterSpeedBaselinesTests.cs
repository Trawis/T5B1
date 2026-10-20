using Xunit;

namespace Trainer_v5.Tests
{
	// Increase Print Speed restores the original per-printer speed from a baseline keyed by instance id (#339).
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
		public void EachPrinterKeepsItsOwnBaseline()
		{
			var baselines = new PrinterSpeedBaselines();
			baselines.Capture(1, 1f);
			baselines.Capture(2, 3f);

			float first, second;
			Assert.True(baselines.TryTake(1, out first));
			Assert.True(baselines.TryTake(2, out second));
			Assert.Equal(1f, first);
			Assert.Equal(3f, second);
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
		public void APrinterWithoutABaselineIsLeftAlone()
		{
			var baselines = new PrinterSpeedBaselines();

			float baseline;
			Assert.False(baselines.TryTake(99, out baseline));
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

		[Fact]
		public void ClearForgetsEveryPrinter()
		{
			var baselines = new PrinterSpeedBaselines();
			baselines.Capture(1, 1f);

			baselines.Clear();

			Assert.Equal(0, baselines.Count);
		}

		[Fact]
		public void TheToggleHasADisabledRowAndTheBaselinesAreResetPerGame()
		{
			var registry = SourceCode.Read("FeatureRegistry.cs");
			var behaviour = SourceCode.Read("TrainerBehaviour.cs");
			var features = SourceCode.Read("Features", "ProductFeatures.cs");

			Assert.Contains("FeatureTrigger.Disabled, \"IncreasePrintSpeed\", SettingKeys.IncreasePrintSpeed, ProductFeatures.RestoreIncreasePrintSpeed", registry);
			Assert.Contains("ProductFeatures.ResetGameState();", behaviour);
			Assert.Contains("GetInstanceID()", features);
			Assert.DoesNotContain("Dictionary<ProductPrinter", features);
		}
	}
}
