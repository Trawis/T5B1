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
	}
}
