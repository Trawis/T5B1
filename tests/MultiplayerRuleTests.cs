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
	}
}
