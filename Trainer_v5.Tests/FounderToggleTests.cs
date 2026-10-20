using Xunit;

namespace Trainer_v5.Tests
{
	// The Founder toggle on the employee detail window (#190).
	public class FounderToggleTests
	{
		[Fact]
		public void ThePromptNamesTheEmployeeAndBothStates()
		{
			var turnOn = FounderToggle.BuildPrompt("Ann", true);
			var turnOff = FounderToggle.BuildPrompt("Ann", false);

			Assert.Contains("Ann: Founder off -> on", turnOn);
			Assert.Contains("Ann: Founder on -> off", turnOff);
			Assert.DoesNotContain("YES", turnOn);
		}

		[Fact]
		public void TheToggleChecksAgainAfterTheConfirmationAndOnlyFlipsTheFlagOnTheExistingEmployee()
		{
			var source = SourceCode.Read("FounderToggle.cs");
			var apply = SourceCode.Body(source, "private static void Apply(");

			Assert.DoesNotContain("YES", apply);
			SourceCode.AssertInOrder(source, "Confirmation.Ask(", "Apply(target, shown)", "the player is asked before the write");
			SourceCode.AssertInOrder(apply, "FindBlocker(target)", "Founder = !shown", "the target is checked again before the write");
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"\.Founder = "));
			Assert.DoesNotContain("Experimental", source);
			Assert.DoesNotContain("new Employee(", source);
			Assert.DoesNotContain(".MadeCEO =", source);
			Assert.DoesNotContain(".Salary =", source);
		}

		[Fact]
		public void TheDetailWindowOffersTheFounderToggleNextToTheOtherEditors()
		{
			var source = SourceCode.Read("DetailWindowTrainer.cs");

			foreach (var label in new[] { "Trait", "Demand", "Inspiration", "LeadSpec", "Founder", "Skills" })
			{
				Assert.Contains("UIFactory.Button(\"Detail" + label + "Button\".LocDef(\"" + label + "\")", source);
			}
		}
	}
}
