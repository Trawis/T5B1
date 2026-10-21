using System.Collections.Generic;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	// #457: an experimental toggle, shown only while the Experimental option is on. It only postpones Actor.ForgetfulETA
	// ahead of the game's own check in AI.Despawn; it never touches the specialization, the trait or the popup itself.
	public class NoForgetfulSkillLossTests
	{
		private static string Action()
		{
			return SourceCode.Body(SourceCode.Read("Features", "EmployeeFeatures.cs"), "internal static void ApplyNoForgetfulSkillLoss");
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
		public void TheToggleHasItsOwnKeyAndNotTheRetiredOne()
		{
			Assert.Equal("NoForgetfulSkillLoss", SettingKeys.NoForgetfulSkillLoss.Key);
			Assert.NotEqual(SettingKeys.DisableSkillDecay.Key, SettingKeys.NoForgetfulSkillLoss.Key);
			Assert.False(SettingKeys.NoForgetfulSkillLoss.DefaultValue);
		}

		[Fact]
		public void TheRegistryRunsItMonthlyAndOnEnablingAndNeverRestores()
		{
			var triggers = new List<FeatureTrigger>();
			foreach (var entry in FeatureRegistry.Create())
			{
				if (entry.Id == "NoForgetfulSkillLoss")
				{
					Assert.Same(SettingKeys.NoForgetfulSkillLoss, entry.Toggle);
					triggers.Add(entry.Trigger);
				}
			}

			Assert.Equal(2, triggers.Count);
			Assert.Contains(FeatureTrigger.Month, triggers);
			Assert.Contains(FeatureTrigger.Enabled, triggers);
		}

		[Fact]
		public void TheRowIsOnlyInTheExperimentalListAndTheWindowAddsItOnlyWhileTheOptionIsOn()
		{
			var keys = new List<string>();
			foreach (var row in SettingToggleRows.CreateExperimental())
			{
				keys.Add(row.Key.Key);
			}

			Assert.Equal(new[] { "NoForgetfulSkillLoss" }, keys.ToArray());
			foreach (var row in SettingToggleRows.Create())
			{
				Assert.NotEqual("NoForgetfulSkillLoss", row.Key.Key);
			}

			var build = SourceCode.Body(SourceCode.Read("SettingsWindow.cs"), "private static void BuildToggles(");
			SourceCode.AssertInOrder(build, "if (ExperimentalOptions.Enabled)", "SettingToggleRows.CreateExperimental()", "only added while the option is on");
		}

		[Fact]
		public void TurningTheOptionOffSwitchesEveryExperimentalToggleOff()
		{
			var settings = new Dictionary<string, bool>();
			settings["NoForgetfulSkillLoss"] = true;
			settings["NoSickness"] = true;

			ExperimentalToggles.TurnOffAll(settings);
			ExperimentalToggles.TurnOffAll(null);

			Assert.False(settings["NoForgetfulSkillLoss"]);
			Assert.True(settings["NoSickness"]);
			Assert.Single(ExperimentalToggles.Keys());
		}

		[Fact]
		public void TheOptionHandlerAndTheGameReadyHandlerTurnThemOff()
		{
			var change = SourceCode.Body(SourceCode.Read("Main.cs"), "private void SetExperimentalEnabled(");
			var ready = SourceCode.Body(SourceCode.Read("TrainerBehaviour.cs"), "private void OnGameReady(");

			SourceCode.AssertInOrder(change, "if (!enabled)", "ExperimentalToggles.TurnOffAll(Helpers.Settings)", "only when turned off");
			SourceCode.AssertInOrder(ready, "if (!ExperimentalOptions.Enabled)", "ExperimentalToggles.TurnOffAll(Helpers.Settings)", "a save with one on is cleared");
		}
	}
}
