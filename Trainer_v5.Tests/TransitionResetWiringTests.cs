using Xunit;

namespace Trainer_v5.Tests
{
	// Leaving the game or deactivating the mod must reset the transition tracker (#266).
	public class TransitionResetWiringTests
	{
		[Fact]
		public void LeavingTheGameAndDeactivatingResetTheToggleTracker()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");

			Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(source, @"_toggleStates\.Reset\(\)").Count);
		}
	}
}
