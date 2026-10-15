using Xunit;

namespace Trainer_v5.Tests
{
	// Which scenes install the trainer and which tear it down (#272); the Unity calls stay in TrainerBehaviour.
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
