using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Creativity editor replaces the Employee object (#183); the guard needs the game's objects, so this checks it is wired in.
	public class CreativityEditGuardTests
	{
		[Fact]
		public void TheCreativityEditorChecksTheGuardBeforeAskingAndAgainBeforeReplacing()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "DetailWindowTrainer.cs"));
			var start = source.IndexOf("private static void SetCreativity", System.StringComparison.Ordinal);
			var end = source.IndexOf("private static void SetInspiration", System.StringComparison.Ordinal);
			var body = source.Substring(start, end - start);

			Assert.Contains("CreativityEditGuard.IsEnabled()", body);
			Assert.Equal(2, Regex.Matches(body, @"CreativityEditGuard\.FindBlockingReference\(").Count);
			Assert.True(body.IndexOf("FindBlockingReference", System.StringComparison.Ordinal) < body.IndexOf("RequestFloat", System.StringComparison.Ordinal));
			Assert.True(body.LastIndexOf("FindBlockingReference", System.StringComparison.Ordinal) < body.IndexOf("new Employee(", System.StringComparison.Ordinal));
		}
	}
}
