using Xunit;

namespace Trainer_v5.Tests
{
	public class FounderToggleTests
	{
		[Fact]
		public void TheToggleChecksAgainAfterTheConfirmationAndOnlyFlipsTheFlagOnTheExistingEmployee()
		{
			var source = SourceCode.Read("FounderToggle.cs");
			var apply = SourceCode.Body(source, "private static void Apply(");

			SourceCode.AssertInOrder(source, "Confirmation.Ask(", "Apply(target, shown)", "the player is asked before the write");
			SourceCode.AssertInOrder(apply, "FindBlocker(target)", "Founder = !shown", "the target is checked again before the write");
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"\.Founder = "));
		}
	}
}
