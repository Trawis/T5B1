using Xunit;

namespace Trainer_v5.Tests
{
	public class ToggleStateTrackerTests
	{
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
		public void AToggleAlreadyEnabledOnTheFirstObservationAppliesOnce()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransitionEnum.JustEnabled, tracker.Observe("NoStress", true));
			Assert.Equal(ToggleTransitionEnum.Unchanged, tracker.Observe("NoStress", true));
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
	}
}
