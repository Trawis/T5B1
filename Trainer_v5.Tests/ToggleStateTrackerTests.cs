using Xunit;

namespace Trainer_v5.Tests
{
	public class ToggleStateTrackerTests
	{
		[Fact]
		public void ADisabledToggleThatStaysDisabledNeverTransitions()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("FreePrint", false));
			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("FreePrint", false));
		}

		[Fact]
		public void EnablingReportsJustEnabledOnce()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FreePrint", false);

			Assert.Equal(ToggleTransition.JustEnabled, tracker.Observe("FreePrint", true));
			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("FreePrint", true));
			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("FreePrint", true));
		}

		[Fact]
		public void DisablingReportsJustDisabledOnceSoRestoreRunsOnce()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FreePrint", true);

			Assert.Equal(ToggleTransition.JustDisabled, tracker.Observe("FreePrint", false));
			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("FreePrint", false));
		}

		[Fact]
		public void ReEnablingAfterDisablingReportsJustEnabledAgain()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransition.JustEnabled, tracker.Observe("FreePrint", true));
			Assert.Equal(ToggleTransition.JustDisabled, tracker.Observe("FreePrint", false));
			Assert.Equal(ToggleTransition.JustEnabled, tracker.Observe("FreePrint", true));
		}

		[Fact]
		public void AToggleAlreadyEnabledOnTheFirstObservationAppliesOnce()
		{
			var tracker = new ToggleStateTracker();

			Assert.Equal(ToggleTransition.JustEnabled, tracker.Observe("NoStress", true));
			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("NoStress", true));
		}

		[Fact]
		public void TogglesAreTrackedIndependently()
		{
			var tracker = new ToggleStateTracker();
			tracker.Observe("FullEnvironment", true);
			tracker.Observe("FullRoomBrightness", true);

			Assert.Equal(ToggleTransition.JustDisabled, tracker.Observe("FullEnvironment", false));
			Assert.Equal(ToggleTransition.Unchanged, tracker.Observe("FullRoomBrightness", true));
		}

		[Fact]
		public void SeparateTrackersDoNotShareState()
		{
			var first = new ToggleStateTracker();
			var second = new ToggleStateTracker();
			first.Observe("FreePrint", true);

			Assert.Equal(ToggleTransition.JustEnabled, second.Observe("FreePrint", true));
		}
	}
}
