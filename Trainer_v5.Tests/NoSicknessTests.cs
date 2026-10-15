using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The global sick list is cleared once per pass, not once per actor (#268).
	public class NoSicknessTests
	{
		[Fact]
		public void TheSickListIsClearedOutsideThePerActorWork()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs"));
			var perActor = source.Substring(source.IndexOf("private static void ApplyNoSicknessToActor", System.StringComparison.Ordinal), 500);

			Assert.DoesNotContain("Sick.Clear()", perActor);
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"Sick\.Clear\(\)"));
		}
	}
}
