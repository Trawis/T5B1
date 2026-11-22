using System.Collections.Generic;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	// It only postpones Actor.ForgetfulETA ahead of the game's own check in AI.Despawn; it never touches the specialization, the trait or the popup itself.
	public class NoForgetfulSkillLossTests
	{
		private static string Action()
		{
			return SourceCode.Body(SourceCode.Read("Features", "EmployeeFeatures.cs"), "internal static void ApplyNoForgetfulSkillLoss");
		}

		[Fact]
		public void OnlyActorsWithTheForgetfulTraitAreTouched()
		{
			var action = Action();

			Assert.Contains("!actor.employee.HasTrait(Employee.Trait.Forgetful)", action);
			SourceCode.AssertInOrder(action, "HasTrait(Employee.Trait.Forgetful)", "actor.ForgetfulETA = now", "the trait gates the write");
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
		public void TurningTheOptionOffSwitchesEveryExperimentalToggleOff()
		{
			var settings = new Dictionary<string, bool>();
			settings["NoForgetfulSkillLoss"] = true;
			settings["AutoInstantPatent"] = true;
			settings["AutoInstantResearch"] = true;
			settings["AutoMaxFollowers"] = true;
			settings["NoSickness"] = true;

			ExperimentalToggles.TurnOffAll(settings);
			ExperimentalToggles.TurnOffAll(null);

			Assert.False(settings["NoForgetfulSkillLoss"]);
			Assert.False(settings["AutoInstantPatent"]);
			Assert.False(settings["AutoInstantResearch"]);
			Assert.False(settings["AutoMaxFollowers"]);
			Assert.True(settings["NoSickness"]);
		}
	}
}
