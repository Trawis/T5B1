using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The window object is cached after the first open, so the Test controls must follow the Experimental setting
	// through the toggle callback instead of only at build time.
	public class SettingsWindowExperimentalTests
	{
		private static string ReadSource()
		{
			return File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "SettingsWindow.cs"));
		}

		[Fact]
		public void TheExperimentalToggleShowsAndHidesTheTestControls()
		{
			var source = ReadSource();

			Assert.Contains("settings.Toggle(SettingKeys.Experimental);", source);
			Assert.Contains("SetTestControlsActive(settings, testButton);", source);
			Assert.Contains("testButton.SetActive(experimental);", source);
			Assert.Contains("\"ExperimentalHint\".LocDef(\"Shows the Test button below\")", source);
		}

		[Fact]
		public void TheDummyTestToggleIsGone()
		{
			var source = ReadSource();

			Assert.DoesNotContain("testToggle", source);
			Assert.DoesNotContain("\"TestToggle\"", source);
		}

		[Fact]
		public void TheLabelNoLongerAsksForTheWindowToBeReopened()
		{
			Assert.DoesNotContain("re-open the window", ReadSource());
		}
	}
}
