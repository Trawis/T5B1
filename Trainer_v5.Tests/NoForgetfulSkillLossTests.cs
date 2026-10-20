using Xunit;

namespace Trainer_v5.Tests
{
	// #457: experimental. It only postpones Actor.ForgetfulETA ahead of the game's own check in AI.Despawn and has no
	// Trainer Settings control; it never touches the specialization, the trait or the popup itself.
	public class NoForgetfulSkillLossTests
	{
		private static string Action()
		{
			return SourceCode.Body(SourceCode.Read("Experimental", "NoForgetfulSkillLoss.cs"), "internal static void Apply()");
		}

		[Fact]
		public void ItDoesNothingUnlessTheExperimentalOptionIsOn()
		{
			var action = Action();

			Assert.Contains("!ExperimentalOptions.Enabled", action);
			SourceCode.AssertInOrder(action, "ExperimentalOptions.Enabled", "actor.ForgetfulETA = now", "the option gates every write");
		}

		[Fact]
		public void OnlyActorsWithTheForgetfulTraitAreTouched()
		{
			var action = Action();

			Assert.Contains("!actor.employee.HasTrait(Employee.Trait.Forgetful)", action);
			SourceCode.AssertInOrder(action, "HasTrait(Employee.Trait.Forgetful)", "actor.ForgetfulETA = now", "the trait gates the write");
		}

		[Fact]
		public void WithoutTheGameClockNothingIsWritten()
		{
			var action = Action();

			Assert.Contains("TimeOfDay.Instance == null", action);
			SourceCode.AssertInOrder(action, "TimeOfDay.Instance == null", "SDateTime.Now()", "Now() is the year 0 default without a clock");
		}

		[Fact]
		public void AnUnarmedTimerIsLeftAlone()
		{
			var action = Action();

			Assert.Contains("SDateTime unarmed = new SDateTime(0);", action);
			Assert.Contains("actor.ForgetfulETA != unarmed", action);
		}

		[Fact]
		public void OnlyTheTimerIsWritten()
		{
			var action = Action();

			Assert.Single(System.Text.RegularExpressions.Regex.Matches(action, @"actor\.\w+ = [^=]"));
			Assert.Contains("actor.ForgetfulETA = now + 12;", action);
			Assert.DoesNotContain("LoseSpec", action);
			Assert.DoesNotContain("SpecializationLevels", action);
			Assert.DoesNotContain("SetTraitView", action);
			Assert.DoesNotContain("RemoveTrait", action);
			Assert.DoesNotContain("AddTrait", action);
		}

		[Fact]
		public void ItHasNoSettingKeyToggleRowOrRegistryEntry()
		{
			Assert.DoesNotContain("NoForgetfulSkillLoss", SourceCode.Read("SettingKeys.cs"));
			Assert.DoesNotContain("NoForgetfulSkillLoss", SourceCode.Read("SettingToggleRows.cs"));
			Assert.DoesNotContain("NoForgetfulSkillLoss", SourceCode.Read("FeatureRegistry.cs"));
		}

		[Fact]
		public void ItRunsMonthlyAndWhenTheGameIsReady()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");

			Assert.Contains("NoForgetfulSkillLoss.Apply", SourceCode.Body(source, "private void OnMonthPassed("));
			Assert.Contains("NoForgetfulSkillLoss.Apply", SourceCode.Body(source, "private void OnGameReady("));
		}
	}
}
