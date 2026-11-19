using Xunit;

namespace Trainer_v5.Tests
{
	// Game UI types cannot be built in the test host, so these guard the window's event wiring by source shape.
	public class SettingsWindowTabsTests
	{
		private static string Window()
		{
			return SourceCode.Read("SettingsWindow.cs");
		}

		[Fact]
		public void AToggleChangeRaisedByARefreshDoesNotFlipTheSetting()
		{
			var source = Window();
			var handler = SourceCode.Body(source, "private static void OnToggleChanged(");
			var refresh = SourceCode.Body(source, "private static void RefreshFromSettings()");

			SourceCode.AssertInOrder(handler, "if (_refreshing) { return; }", "settings.Toggle(key);", "the guard comes before the flip");
			SourceCode.AssertInOrder(refresh, "_refreshing = true;", ".isOn =", "set while the controls change");
			SourceCode.AssertInOrder(refresh, ".isOn =", "finally { _refreshing = false; }", "cleared even when a control throws");
			Assert.DoesNotContain(".UpdateSelection(", SourceCode.Body(source, "private static void RefreshEfficiency("));
		}

		[Fact]
		public void ListenersAreAddedOnlyWhileTheWindowIsBuilt()
		{
			var source = Window();
			var inBuild = Count(SourceCode.Body(source, "private static void Build()"), "AddListener")
				+ Count(SourceCode.Body(source, "private static GUICombobox AddEfficiencyRow("), "AddListener");

			Assert.True(inBuild >= 1);
			Assert.Equal(Count(source, "AddListener"), inBuild);
		}

		[Fact]
		public void TheImmediateTogglesStillApplyTheirEffectAtOnce()
		{
			var handler = SourceCode.Body(Window(), "private static void OnToggleChanged(");

			foreach (var row in SettingToggleRows.CreateImmediate())
			{
				Assert.Contains("key == SettingKeys." + row.Key.Key, handler);
			}
		}

		private static int Count(string source, string text)
		{
			var count = 0;
			var index = 0;
			while ((index = source.IndexOf(text, index, System.StringComparison.Ordinal)) >= 0)
			{
				count++;
				index += text.Length;
			}

			return count;
		}
	}
}
