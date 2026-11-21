using Xunit;

namespace Trainer_v5.Tests
{
	public class FounderToggleTests
	{
		[Fact]
		public void TheToggleChecksAgainAfterTheConfirmationAndOnlyFlipsTheFlagOnTheExistingEmployee()
		{
			var source = SourceCode.Read("FounderToggle.cs");

			Assert.Contains("Confirmation.AskAndApply(\"Founder\", string.Format(", source);
			Assert.Contains("() => FindChange(target, shown), () => Apply(target, shown));", source);
			Assert.Contains("FindBlocker(target)", SourceCode.Body(source, "private static string FindChange("));
			Assert.Contains("Founder = !shown", SourceCode.Body(source, "private static string Apply("));
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"\.Founder = "));
		}
	}
}
