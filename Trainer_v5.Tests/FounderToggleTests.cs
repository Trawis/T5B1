using System.IO;
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
			Assert.Contains("Type YES to confirm", turnOn);
		}

		[Fact]
		public void TheToggleChecksAgainAfterTheConfirmationAndOnlyFlipsTheFlagOnTheExistingEmployee()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "FounderToggle.cs"));
			var apply = source.Substring(source.IndexOf("private static void Apply", System.StringComparison.Ordinal));
			apply = apply.Substring(0, apply.IndexOf("private static string FindBlocker", System.StringComparison.Ordinal));

			Assert.True(apply.IndexOf("answer != \"YES\"", System.StringComparison.Ordinal) < apply.IndexOf("FindBlocker(target)", System.StringComparison.Ordinal));
			Assert.True(apply.IndexOf("FindBlocker(target)", System.StringComparison.Ordinal) < apply.IndexOf("Founder = !shown", System.StringComparison.Ordinal));
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"\.Founder = "));
			Assert.DoesNotContain("Experimental", source);
			Assert.DoesNotContain("new Employee(", source);
			Assert.DoesNotContain(".MadeCEO =", source);
			Assert.DoesNotContain(".Salary =", source);
		}

		[Fact]
		public void TheDetailWindowOffersTheFounderToggleNextToTheOtherEditors()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "DetailWindowTrainer.cs"));

			foreach (var label in new[] { "\"Trait\"", "\"Demand\"", "\"Inspiration\"", "\"LeadSpec\"", "\"Founder\"" })
			{
				Assert.Contains("UIFactory.Button(" + label, source);
			}
			Assert.DoesNotContain("Creativity", source);
			Assert.DoesNotContain("new Employee(", source);
		}
	}
}
