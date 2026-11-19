using Xunit;

namespace Trainer_v5.Tests
{
	public class SceneRoutesTests
	{
		[Theory]
		[InlineData("MainScene", "MainScene")]
		[InlineData("Customization", "Customization")]
		[InlineData("MainMenu", "LeaveGame")]
		[InlineData("SomeFutureScene", "LeaveGame")]
		[InlineData("mainscene", "LeaveGame")]
		[InlineData("", "LeaveGame")]
		[InlineData(null, "LeaveGame")]
		public void EachSceneNameRoutesTheSameAsBefore(string name, string expected)
		{
			Assert.Equal(expected, SceneRoutes.For(name).ToString());
		}
	}
}
