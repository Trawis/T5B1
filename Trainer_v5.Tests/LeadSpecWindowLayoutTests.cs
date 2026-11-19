using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	public class LeadSpecWindowLayoutTests
	{
		[Fact]
		public void TheWindowHeightComesFromTheSharedColumnHeightCalculation()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeLeadSpecChangeWindow.cs"));

			Assert.Contains("UIHelper.GetColumnWindowHeight(col1Height, col2Height, padding)", source);
		}
	}
}
