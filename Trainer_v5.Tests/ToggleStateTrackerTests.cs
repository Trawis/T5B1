using Xunit;

namespace Trainer_v5.Tests
{
	public class ToggleStateTrackerTests
	{
		[Fact]
		public void ADisabledToggleThatStaysDisabledNeverTransitions()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FreePrint", false));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FreePrint", false));
		}

		[Fact]
		public void EnablingReportsJustEnabledOnce()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FreePrint", false);

			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("FreePrint", true));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FreePrint", true));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FreePrint", true));
		}

		[Fact]
		public void DisablingReportsJustDisabledOnceSoRestoreRunsOnce()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FreePrint", true);

			Assert.Equal(ToggleTransitionEnum.JustDisabled, tracker.Observe("FreePrint", false));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FreePrint", false));
		}

		[Fact]
		public void ReEnablingAfterDisablingReportsJustEnabledAgain()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("FreePrint", true));
			Assert.Equal(ToggleTransitionEnum.JustDisabled, tracker.Observe("FreePrint", false));
			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("FreePrint", true));
		}

		[Fact]
		public void AToggleAlreadyEnabledOnTheFirstObservationAppliesOnce()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("NoStress", true));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("NoStress", true));
		}

		[Fact]
		public void TogglesAreTrackedIndependently()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FullEnvironment", true);
			tracker.Observe("FullRoomBrightness", true);

			Assert.Equal(ToggleTransitionEnum.JustDisabled, tracker.Observe("FullEnvironment", false));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FullRoomBrightness", true));
		}

		[Fact]
		public void ResettingMakesTheNextSaveAColdStart()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FreePrint", true);
			tracker.Observe("NoMaintenance", false);

			tracker.Reset();

			// The next save has FreePrint on again: the enable hook must run for it, not be skipped as unchanged.
			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("FreePrint", true));
			// A toggle that was on in the previous save and is off here must not run its restore hook.
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("NoMaintenance", false));
		}

		[Fact]
		public void AToggleOnBeforeAResetAndOffAfterItReportsJustDisabled()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("ReduceISPCost", true);

			tracker.Reset();

			Assert.Equal(ToggleTransitionEnum.JustDisabled, tracker.Observe("ReduceISPCost", false));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("ReduceISPCost", false));
		}

		[Fact]
		public void AToggleOnBeforeAResetAndOnAfterItAppliesAgain()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("ReduceISPCost", true);

			tracker.Reset();

			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("ReduceISPCost", true));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("ReduceISPCost", true));
		}

		[Fact]
		public void AToggleOffBeforeAResetStaysQuietAfterIt()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("ReduceISPCost", false);
			tracker.Observe("FreePrint", true);
			tracker.Observe("FreePrint", false);

			tracker.Reset();

			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("ReduceISPCost", false));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("FreePrint", false));
		}

		[Fact]
		public void TheCarriedOnStateIsUsedOnlyForTheFirstObservationAfterTheReset()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FullEnvironment", true);
			tracker.Reset();
			tracker.Observe("FullEnvironment", true);
			tracker.Reset();

			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("FullEnvironment", true));
		}
	}
}
