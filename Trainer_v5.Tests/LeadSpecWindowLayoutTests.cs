using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Lead Spec window must be tall enough for its footer buttons: the title bar needs one extra row.
	public class LeadSpecWindowLayoutTests
	{
		[Fact]
		public void TheWindowHeightIncludesTheTitleBarRow()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeLeadSpecChangeWindow.cs"));

			Assert.Contains("Mathf.Max(col1Height, col2Height) + padding + Constants.ELEMENT_HEIGHT", source);
		}
	}
}
