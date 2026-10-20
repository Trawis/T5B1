using System;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Per-game UI and runtime caches are reset at game boundaries and the named employee actions check readiness first (#344).
	public class PerGameStateResetTests
	{
		private static string Read(params string[] parts)
		{
			var path = RepositoryFiles.SourceDirectory;
			foreach (var part in parts)
			{
				path = Path.Combine(path, part);
			}
			return File.ReadAllText(path);
		}

		private static string Between(string source, string from, string to)
		{
			var start = source.IndexOf(from, StringComparison.Ordinal);
			Assert.True(start >= 0, from);
			var end = source.IndexOf(to, start, StringComparison.Ordinal);
			Assert.True(end > start, to);
			return source.Substring(start, end - start);
		}

		[Fact]
		public void LeavingTheGameResetsThePerGameCaches()
		{
			var source = Read("TrainerBehaviour.cs");

			Assert.Contains("ResetPerGameState();", Between(source, "private void LeaveGame", "private void ResetPerGameState"));
			var reset = Between(source, "private void ResetPerGameState", "private void SubscribeToSceneEvents");
			Assert.Contains("SettingsWindow.Reset();", reset);
			Assert.Contains("EmployeeSkillChangeWindow.Reset();", reset);
			Assert.Contains("EmployeeFeatures.ResetSpecializations();", reset);
			Assert.Contains("MarketFeatures.ResetGameState();", reset);
		}

		[Fact]
		public void EnteringTheMainSceneResetsThePerGameCachesBeforeBuildingTheUi()
		{
			var source = Read("TrainerBehaviour.cs");
			var mainScene = Between(source, "case SceneRoute.MainScene:", "case SceneRoute.Customization:");

			Assert.True(mainScene.IndexOf("ResetPerGameState();", StringComparison.Ordinal) < mainScene.IndexOf("Main.CreateUIButtons();", StringComparison.Ordinal));
		}

		[Fact]
		public void ThePerGameResetDoesNotTouchPersistedSettings()
		{
			var source = Read("TrainerBehaviour.cs");
			var reset = Between(source, "private void ResetPerGameState", "private void SubscribeToSceneEvents");

			Assert.DoesNotContain("TrainerSettings", reset);
			Assert.DoesNotContain("StoresSettings", reset);
			Assert.DoesNotContain("Helpers.Settings", reset);
		}

		[Fact]
		public void TheSettingsWindowResetClosesItsStaleState()
		{
			var reset = Between(Read("SettingsWindow.cs"), "public static void Reset()", "private static void Init()");

			Assert.Contains("Window = null;", reset);
			Assert.Contains("Shown = false;", reset);
		}

		[Fact]
		public void TheHostingTimingAndWarningStateAreReset()
		{
			var reset = Between(Read("Features", "MarketFeatures.cs"), "internal static void ResetGameState()", "private static void WarnOnce");

			Assert.Contains("Helpers.DealIsPushed = false;", reset);
			Assert.Contains("Helpers.RewardIsGained = false;", reset);
			Assert.Contains("_warnedAboutPendingList = false;", reset);
		}

		[Fact]
		public void SpecializationsAreRecomputedOnGameReadyWithoutRepeatingTheFirstLoadPopup()
		{
			var employee = Read("Features", "EmployeeFeatures.cs");
			var reset = Between(employee, "internal static void ResetSpecializations()", "internal static void RestoreSpecializationsAfterReset()");
			var restore = Between(employee, "internal static void RestoreSpecializationsAfterReset()", "internal static void LoadSpecializations()");
			var ready = Between(Read("TrainerBehaviour.cs"), "private void OnGameReady", "private void OnHourPassed");

			Assert.Contains("SpecializationsList = null;", reset);
			Assert.DoesNotContain("_specializationsLoaded", reset);
			Assert.Contains("_specializationsLoaded && Helpers.SpecializationsList == null", restore);
			Assert.Contains("RestoreSpecializationsAfterReset", ready);
		}

		[Fact]
		public void EmployeesToMaxChecksReadinessBeforeTouchingTheMarketSimulation()
		{
			var employee = Read("Features", "EmployeeFeatures.cs");
			var body = Between(employee, "internal static void EmployeesToMax()", "foreach (Actor actor");

			Assert.True(body.IndexOf("RequireGameReady(", StringComparison.Ordinal) < body.IndexOf("MarketSimulation.Active", StringComparison.Ordinal));
			Assert.Contains("requireSelector: true", body);
		}

		[Fact]
		public void ResetAgeChecksReadinessBeforeTouchingTheActorManager()
		{
			var employee = Read("Features", "EmployeeFeatures.cs");
			var body = Between(employee, "internal static void ResetAgeOfEmployees()", "internal static void EmployeesToMax()");

			Assert.True(body.IndexOf("RequireGameReady(", StringComparison.Ordinal) < body.IndexOf("sActorManager", StringComparison.Ordinal));
		}

		[Fact]
		public void ANotReadyActionIsLoggedAndShownWhenThereIsAHud()
		{
			var body = Between(Read("Helpers.cs"), "internal static bool RequireGameReady", "#endregion");

			Assert.Contains(".Log();", body);
			Assert.Contains("Notification.ShowError(", body);
			Assert.Contains("HUD.Instance != null", body);
		}
	}
}
