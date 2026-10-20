using Xunit;

namespace Trainer_v5.Tests
{
	// The new-game screen's extended start years and loan amounts live in Constants (#279); their values must not drift.
	public class CustomizationConstantsTests
	{
		[Fact]
		public void StartYearsKeepTheOfferedValues()
		{
			Assert.Equal(new[] { 1970, 1975, 1980, 1985, 1990, 1995, 2000, 2005, 2010, 2015, 2020, 2025, 2030, 2035, 2040, 2045, 2050, 2060, 2070, 2080, 2090, 2100 }, Constants.CUSTOMIZATION_START_YEARS);
		}

		[Fact]
		public void StartLoansKeepTheOfferedValues()
		{
			Assert.Equal(new[] { 0, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000, 5000000, 10000000 }, Constants.CUSTOMIZATION_START_LOANS);
		}

		[Fact]
		public void CustomizationSceneAssignsTheConstantsAndKeepsNoInlineArrays()
		{
			var scene = SourceCode.Span(SourceCode.Read("TrainerBehaviour.cs"), "case SceneRoute.Customization:", "default:");

			Assert.Contains("ActorCustomization.StartYears = Constants.CUSTOMIZATION_START_YEARS;", scene);
			Assert.Contains("ActorCustomization.StartLoans = Constants.CUSTOMIZATION_START_LOANS;", scene);
			Assert.DoesNotContain("new[]", scene);
		}
	}
}
