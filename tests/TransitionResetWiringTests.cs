using Xunit;

namespace Trainer_v5.Tests
{
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
