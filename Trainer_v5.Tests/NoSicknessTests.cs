using Xunit;

namespace Trainer_v5.Tests
{
	// The global sick list is cleared once per pass, not once per actor (#268), and employees are not marked as recently sick (#360).
	public class NoSicknessTests
	{
		private static string Source()
		{
			return SourceCode.Read("Features", "EnvironmentFeatures.cs");
		}

		private static string PerActorWork()
		{
			return SourceCode.Body(Source(), "private static void ApplyNoSicknessToActor");
		}

		[Fact]
		public void TheSickListIsClearedOutsideThePerActorWork()
		{
			var source = Source();
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
			var source = Source();

			Assert.DoesNotContain("WasSick", source);
		}
	}
}
