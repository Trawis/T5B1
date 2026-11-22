using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	// Wiring rules read from the compiled trainer (see CompiledCalls). The game objects involved cannot be built in the
	// test host, so what the code calls, and in which order, is what can be checked here.
	public class CompiledWiringTests
	{
		private static bool IsEquality(string step)
		{
			return step == "call String.op_Equality" || step == "call String.op_Inequality" || step == "call String.Equals" || step == "call String.Contains";
		}

		private static bool IsTypeText(string step)
		{
			return step == "call Object.ToString" || step == "call Enum.ToString" || step == "call Type.get_Name" || step == "call Type.get_FullName";
		}

		private static bool IsDealLiteral(string step)
		{
			return step.StartsWith("ldstr ", StringComparison.Ordinal) && step.Contains("Deal");
		}

		// Architecture: feature classes own no error boundary and do not call each other.
		[Fact]
		public void FeatureClassesNeitherGuardThemselvesNorCallOtherFeatureClasses()
		{
			var offenders = new SortedSet<string>(StringComparer.Ordinal);
			foreach (var method in CompiledCalls.AllMethods())
			{
				var owner = method.Key.Substring(0, method.Key.IndexOf('.'));
				if (!owner.EndsWith("Features", StringComparison.Ordinal))
				{
					continue;
				}

				foreach (var step in method.Value)
				{
					var callsOther = (step.StartsWith("call ", StringComparison.Ordinal) || step.StartsWith("ldftn ", StringComparison.Ordinal))
						&& step.Contains("Features.") && !step.Contains(" " + owner + ".");
					if (step == "call Helpers.TryExecute" || callsOther)
					{
						offenders.Add(method.Key + ": " + step);
					}
				}
			}

			Assert.True(offenders.Count == 0, string.Join("; ", offenders));
		}

		// Frame rows run every frame, so they must not allocate through LINQ or box an enumerator.
		[Fact]
		public void FrameRowsUseNoLinqAndNoEnumerators()
		{
			var offenders = new List<string>();
			foreach (var entry in FeatureRegistry.Create())
			{
				if (entry.Trigger != FeatureTrigger.Frame)
				{
					continue;
				}

				MethodInfo method = entry.Action.Method;
				foreach (var step in CompiledCalls.Steps(method.DeclaringType, method.Name))
				{
					if (step.StartsWith("call Enumerable.", StringComparison.Ordinal) || step.EndsWith(".GetEnumerator", StringComparison.Ordinal))
					{
						offenders.Add(entry.Id + ": " + step);
					}
				}
			}

			Assert.True(offenders.Count == 0, string.Join("; ", offenders));
		}

		// Deal types are selected by type, never by the text of ToString() or GetType().Name.
		[Fact]
		public void NoDealIsSelectedByTheTextOfItsType()
		{
			var offenders = CompiledCalls.Where(steps =>
				CompiledCalls.HasRun(steps, IsTypeText, IsDealLiteral, IsEquality) || CompiledCalls.HasRun(steps, IsDealLiteral, null, IsTypeText, IsEquality));

			Assert.True(offenders.Count == 0, string.Join(", ", offenders));
			CompiledCalls.Calls(typeof(MarketFeatures), "ApplyAutoAcceptHostingDeals", "call Enumerable.OfType");
		}

		// The game translates stage text and role names, so they must not drive logic.
		[Fact]
		public void NoTranslatedGameTextDrivesLogic()
		{
			Func<string, bool> stage = step => step.EndsWith(".CurrentStage", StringComparison.Ordinal);
			Func<string, bool> literal = step => step.StartsWith("ldstr ", StringComparison.Ordinal);
			var stageComparisons = CompiledCalls.Where(steps => CompiledCalls.HasRun(steps, stage, literal, IsEquality) || CompiledCalls.HasRun(steps, literal, stage, IsEquality));

			Assert.True(stageComparisons.Count == 0, "CurrentStage() returns translated text; test the work item's state instead: " + string.Join(", ", stageComparisons));
			CompiledCalls.OnlyIn("call Employee.get_RoleString");
		}

		// No Maintenance repairs through RepairMe with a factor and never forces the furniture state itself.
		[Fact]
		public void RepairsGoThroughRepairMeWithAFactor()
		{
			CompiledCalls.Adjacent(typeof(EnvironmentFeatures), "ApplyNoMaintenance", "ldc.r4 " + ((float)typeof(EnvironmentFeatures).GetField("RepairFactor", BindingFlags.NonPublic | BindingFlags.Static).GetRawConstantValue()).ToString(System.Globalization.CultureInfo.InvariantCulture), "call Upgradable.RepairMe");
			CompiledCalls.OnlyIn("stfld Upgradable.Broken");
			CompiledCalls.OnlyIn("stfld Upgradable.Quality");
		}

		// No Sickness never marks employees as recently sick, which would cost them the WasSick penalty.
		[Fact]
		public void NothingWritesWasSick()
		{
			var offenders = CompiledCalls.Where(steps => steps.Exists(step => step.EndsWith(".WasSick", StringComparison.Ordinal) || step.EndsWith(".set_WasSick", StringComparison.Ordinal)));

			Assert.True(offenders.Count == 0, string.Join(", ", offenders));
		}

		[Fact]
		public void LeavingOrEnteringAGameResetsThePerGameStateButNotTheSettings()
		{
			var type = typeof(TrainerBehaviour);

			CompiledCalls.Calls(type, "LeaveGame", "call TrainerBehaviour.ResetPerGameState");
			CompiledCalls.InOrder(type, "ResetPerGameState", "call SettingsWindow.Reset", "call EmployeeSkillChangeWindow.Reset", "call EmployeeFeatures.ResetSpecializations", "call MarketFeatures.ResetGameState");
			CompiledCalls.NeverCalls(type, "ResetPerGameState", "call Helpers.get_Settings");
			CompiledCalls.NeverCalls(type, "ResetPerGameState", "call Helpers.get_StoresSettings");
			CompiledCalls.InOrder(type, "OnLevelFinishedLoading", "call TrainerBehaviour.SubscribeToEvents", "call TrainerBehaviour.ResetPerGameState", "call Main.CreateUIButtons", "call EmployeesWindowTrainer.Install");
		}

		// Every save load is a cold start: leaving a game and deactivating the mod reset the toggle tracker, nothing else does.
		[Fact]
		public void OnlyLeavingAndDeactivatingResetTheToggleTracker()
		{
			CompiledCalls.Calls(typeof(TrainerBehaviour), "LeaveGame", "call ToggleStateTracker.Reset");
			CompiledCalls.Calls(typeof(TrainerBehaviour), "OnDeactivate", "call ToggleStateTracker.Reset");
			CompiledCalls.OnlyIn("call ToggleStateTracker.Reset", "TrainerBehaviour.LeaveGame", "TrainerBehaviour.OnDeactivate");
		}

		// The game's toggles always raise a change event, so a refresh sets the guard before it moves the controls.
		[Fact]
		public void AToggleChangeRaisedByARefreshDoesNotFlipTheSetting()
		{
			var type = typeof(SettingsWindow);

			CompiledCalls.InOrder(type, "OnToggleChanged", "ldsfld SettingsWindow._refreshing", "call Extensions.Toggle");
			CompiledCalls.Adjacent(type, "RefreshFromSettings", "ldc 1", "stsfld SettingsWindow._refreshing");
			CompiledCalls.InOrder(type, "RefreshFromSettings", "stsfld SettingsWindow._refreshing", "call Toggle.set_isOn", "stsfld SettingsWindow._refreshing");
			CompiledCalls.NeverCalls(type, "RefreshEfficiency", "call GUICombobox.UpdateSelection");
		}

		// Settings go through SettingKeys; a key spelled as a string literal next to a settings accessor bypasses that.
		[Fact]
		public void NoSettingIsReadOrWrittenThroughAStringLiteral()
		{
			var keys = new HashSet<string>();
			foreach (var field in typeof(SettingKeys).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				var value = field.GetValue(null);
				keys.Add(value is ToggleKey ? ((ToggleKey)value).Key : ((StoreKey)value).Key);
			}

			var accessors = new HashSet<string> { "call Helpers.GetProperty", "call Helpers.SetProperty", "call Extensions.Get", "call Extensions.Set", "call Extensions.Toggle", "call Extensions.GetIndex", "call Dictionary`2.get_Item", "call Dictionary`2.set_Item" };
			var offenders = CompiledCalls.Where(steps =>
			{
				for (var i = 0; i + 1 < steps.Count; i++)
				{
					if (steps[i].StartsWith("ldstr ", StringComparison.Ordinal) && keys.Contains(steps[i].Substring(6)) && accessors.Contains(steps[i + 1]))
					{
						return true;
					}
				}

				return false;
			}, "SettingKeys");

			Assert.True(offenders.Count == 0, "Use SettingKeys.X: " + string.Join(", ", offenders));
		}
	}
}
