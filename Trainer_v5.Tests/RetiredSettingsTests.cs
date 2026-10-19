using System.Linq;
using Xunit;

namespace Trainer_v5.Tests
{
	// Retired toggles keep their save keys but must stay unavailable and inert (#270).
	public class RetiredSettingsTests
	{
		private static readonly string[] Retired = { "ReduceBoxPrice", "AutoMaxMarketShare", "MoreCreativity" };

		[Fact]
		public void RetiredKeysAreStillDeclaredWithTheirOriginalNamesAndDefaultOff()
		{
			Assert.Equal("ReduceBoxPrice", SettingKeys.ReduceBoxPrice.Key);
			Assert.Equal("AutoMaxMarketShare", SettingKeys.AutoMaxMarketShare.Key);
			Assert.False(SettingKeys.ReduceBoxPrice.DefaultValue);
			Assert.False(SettingKeys.AutoMaxMarketShare.DefaultValue);
			Assert.Equal("MoreCreativity", SettingKeys.MoreCreativity.Key);
			Assert.False(SettingKeys.MoreCreativity.DefaultValue);
			Assert.True(Helpers.Settings.ContainsKey("MoreCreativity"));
			Assert.True(Helpers.Settings.ContainsKey("ReduceBoxPrice"));
			Assert.True(Helpers.Settings.ContainsKey("AutoMaxMarketShare"));
		}

		[Fact]
		public void RetiredKeysHaveNoScheduledBehavior()
		{
			var scheduled = Enumerable.ToList(Enumerable.Select(Enumerable.Where(FeatureRegistry.Create(), entry => entry.Toggle != null), entry => entry.Toggle.Key));

			Assert.DoesNotContain(scheduled, key => System.Array.IndexOf(Retired, key) >= 0);
		}

		[Fact]
		public void RetiredKeysHaveNoSettingsWindowToggle()
		{
			var shown = Enumerable.ToList(Enumerable.Select(SettingToggleRows.Create(), row => row.Key.Key));

			Assert.DoesNotContain(shown, key => System.Array.IndexOf(Retired, key) >= 0);
		}
	}
}
