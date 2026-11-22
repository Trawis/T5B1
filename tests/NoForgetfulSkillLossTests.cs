using System.Collections.Generic;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	// It only postpones Actor.ForgetfulETA ahead of the game's own check in AI.Despawn; it never touches the specialization, the trait or the popup itself.
	public class NoForgetfulSkillLossTests
	{
		[Fact]
		public void OnlyActorsWithTheForgetfulTraitAreTouched()
		{
			CompiledCalls.InOrder(typeof(EmployeeFeatures), "ApplyNoForgetfulSkillLoss", "ldc " + (int)Employee.Trait.Forgetful, "call Employee.HasTrait", "stfld Actor.ForgetfulETA");
		}

		[Fact]
		public void OnlyTheTimerIsWritten()
		{
			var steps = CompiledCalls.Steps(typeof(EmployeeFeatures), "ApplyNoForgetfulSkillLoss");

			CompiledCalls.Adjacent(typeof(EmployeeFeatures), "ApplyNoForgetfulSkillLoss", "ldc 12", "call SDateTime.op_Addition", "stfld Actor.ForgetfulETA");
			Assert.Single(steps, step => step.StartsWith("stfld ", System.StringComparison.Ordinal));
			Assert.DoesNotContain(steps, step => step.StartsWith("call Employee.", System.StringComparison.Ordinal) && step != "call Employee.HasTrait");
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
