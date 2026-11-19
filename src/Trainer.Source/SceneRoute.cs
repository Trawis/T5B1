namespace Trainer_v5
{
	internal enum SceneRoute
	{
		// The main menu and every scene the trainer does not know: UI and event subscriptions are torn down.
		LeaveGame = 0,
		MainScene = 1,
		Customization = 2
	}

	internal static class SceneRoutes
	{
		public static SceneRoute For(string sceneName)
		{
			switch (sceneName)
			{
				case "MainScene":
					return SceneRoute.MainScene;
				case "Customization":
					return SceneRoute.Customization;
				default:
					return SceneRoute.LeaveGame;
			}
		}
	}
}
