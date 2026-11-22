using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class MultiplayerRuleTests
	{
		[Theory]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		public void TogglesAndSettingsActAsOffInMultiplayer(bool saved, bool multiplayer, bool expected)
		{
			Assert.Equal(expected, MultiplayerRule.ActsOn(saved, multiplayer));
		}

		// Source shape: the rule is enforced where every button and every toggle read passes, so a new action or
		// toggle cannot skip it.
		[Theory]
		[InlineData(@"\bMultiplayerMode\b", "MultiplayerRule.cs")]
		[InlineData(@"\bSpawnButton\(", "UIFactory.cs")]
		[InlineData(@"GetProperty\(Helpers\.Settings, (key|SettingKeys)", "MultiplayerRule.cs")]
		public void OnlyTheRuleOrItsSingleCallSiteUsesThis(string pattern, string allowedFile)
		{
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var matches = Regex.Matches(SourceCode.Normalize(File.ReadAllText(file)), pattern).Count;
				if (Path.GetFileName(file) == allowedFile)
				{
					Assert.True(matches > 0, allowedFile + " no longer matches " + pattern);
				}
				else
				{
					Assert.True(matches == 0, Path.GetFileName(file) + " matches " + pattern + "; go through MultiplayerRule.");
				}
			}
		}

		[Fact]
		public void EveryButtonAndTheHotkeyRefuseInMultiplayer()
		{
			var button = SourceCode.Body(SourceCode.Read("UIFactory.cs"), "public static Button Button(");
			SourceCode.AssertInOrder(button, "if (!MultiplayerRule.Refuse())", "action();", "the click is refused before the action");

			var open = SourceCode.Body(SourceCode.Read("Main.cs"), "public static void OpenSettingsWindow(");
			SourceCode.AssertInOrder(open, "if (MultiplayerRule.Refuse())", "SettingsWindow.Open();", "F1 is refused before the window opens");
		}
	}
}
