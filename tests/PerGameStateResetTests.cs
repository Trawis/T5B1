using System;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
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
	}
}
