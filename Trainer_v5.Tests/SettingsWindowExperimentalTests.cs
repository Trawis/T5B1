using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Test Button and its debug-only quality action are retired (#341). The Experimental toggle stays in the
	// window until it moves to the native mod Options (#245); its saved key is preserved.
	public class SettingsWindowExperimentalTests
	{
		[Fact]
		public void TheExperimentalToggleStillPersistsItsSetting()
		{
			var source = SourceCode.Read("SettingsWindow.cs");

			Assert.Contains("\"Experimental\".LocDef(\"Experimental\")", source);
			Assert.Contains("settings.Get(SettingKeys.Experimental)", source);
			Assert.Contains("settings.Toggle(SettingKeys.Experimental)", source);
		}

		[Fact]
		public void TheTestControlsAndTheirHintAreGone()
		{
			var source = SourceCode.Read("SettingsWindow.cs");

			Assert.DoesNotContain("testButton", source);
			Assert.DoesNotContain("TestButton", source);
			Assert.DoesNotContain("SetTestControlsActive", source);
			Assert.DoesNotContain("ExperimentalHint", source);
			Assert.DoesNotContain("testToggle", source);
			Assert.DoesNotContain("\"TestToggle\"", source);
		}

		[Fact]
		public void TheTestActionsAreGone()
		{
			var source = SourceCode.Read("Features", "WorkItemFeatures.cs");

			Assert.DoesNotContain("TestButton", source);
			Assert.DoesNotContain("TestAction", source);
			Assert.DoesNotContain("static void Test(", source);
		}

		[Fact]
		public void TheEnglishFileHasNoTestEntriesButKeepsTheExperimentalLabel()
		{
			var english = File.ReadAllText(Path.Combine(RepositoryFiles.Root, "Trainer_v5", "Trainer.Localization", "English", "Trainer.tyd"));

			Assert.DoesNotContain("TestButton", english);
			Assert.DoesNotContain("TestToggle", english);
			Assert.DoesNotContain("ExperimentalHint", english);
			Assert.Contains("Experimental\t", english);
		}

		[Fact]
		public void TheLabelNoLongerAsksForTheWindowToBeReopened()
		{
			Assert.DoesNotContain("re-open the window", SourceCode.Read("SettingsWindow.cs"));
		}
	}
}
