using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The global sick list is cleared once per pass, not once per actor (#268), and employees are not marked as recently sick (#360).
	public class NoSicknessTests
	{
		private static string PerActorWork()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs"));
			var start = source.IndexOf("private static void ApplyNoSicknessToActor", System.StringComparison.Ordinal);
			var end = source.IndexOf("SickDays = 0;", start, System.StringComparison.Ordinal) + "SickDays = 0;".Length;
			return source.Substring(start, end - start);
		}

		[Fact]
		public void TheSickListIsClearedOutsideThePerActorWork()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs"));
			var perActor = PerActorWork();

			Assert.DoesNotContain("Sick.Clear()", perActor);
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"Sick\.Clear\(\)"));
		}

		[Fact]
		public void ASickEmployeeIsSentBackToWorkWithoutTheWasSickPenalty()
		{
			var perActor = PerActorWork();

			Assert.Contains("actor.SpecialState = Actor.HomeState.Default;", perActor);
			Assert.Contains("actor.GermAdd = 0f;", perActor);
			Assert.Contains("actor.GermCount = 0f;", perActor);
			Assert.Contains("actor.SickDays = 0;", perActor);
			Assert.DoesNotContain("WasSick", perActor);
		}

		[Fact]
		public void NoSicknessNeverWritesWasSickAnywhere()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs"));

			Assert.DoesNotContain("WasSick", source);
		}
	}
}
