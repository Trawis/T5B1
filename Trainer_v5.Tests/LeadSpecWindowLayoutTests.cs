using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Lead Spec window must be tall enough for its footer buttons: the title bar needs one extra row.
	public class LeadSpecWindowLayoutTests
	{
		[Fact]
		public void TheWindowHeightComesFromTheSharedColumnHeightCalculation()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeLeadSpecChangeWindow.cs"));

			Assert.Contains("UIHelper.GetColumnWindowHeight(col1Height, col2Height, padding)", source);
		}

		[Fact]
		public void TheNoEmployeeMessageOnlyMentionsControlsTheWindowHas()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeLeadSpecChangeWindow.cs"));

			Assert.DoesNotContain("press Refresh", source);
			Assert.DoesNotContain("UIFactory.Button(\"Refresh\"", source);
			Assert.Contains("close and reopen this window", source);
		}
	}
}
