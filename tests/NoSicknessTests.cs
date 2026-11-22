using Xunit;

namespace Trainer_v5.Tests
{
	// The global sick list is cleared once per pass, not once per actor, and employees are not marked as recently sick.
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
		public void NoSicknessNeverWritesWasSickAnywhere()
		{
			var source = Source();

			Assert.DoesNotContain("WasSick", source);
		}
	}
}
