using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Time events must be subscribed before any fallible UI installation on scene load.
	public class StartupSubscriptionTests
	{
		[Fact]
		public void TheMainSceneSubscribesBeforeItCreatesOrInstallsAnyUi()
		{
			var branch = SourceCode.Span(SourceCode.Read("TrainerBehaviour.cs"), "case SceneRoute.MainScene:", "break;");

			SourceCode.AssertInOrder(branch, "SubscribeToEvents()", "Main.CreateUIButtons()", "the main scene must subscribe before creating UI");
			SourceCode.AssertInOrder(branch, "SubscribeToEvents()", "EmployeesWindowTrainer.Install()", "the main scene must subscribe before installing UI");
		}
	}
}
