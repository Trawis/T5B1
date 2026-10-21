using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// A retried UI install leaves exactly one set of controls (#371).
	public class UiInstallRetryTests
	{
		private static string Read(string file)
		{
			return File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, file));
		}

		private static int Index(string text, string marker)
		{
			return text.IndexOf(marker, System.StringComparison.Ordinal);
		}

		[Fact]
		public void TheEmployeesToolbarControlsAreDestroyedInAFinallyBlockWhenPlacementDidNotComplete()
		{
			var source = Read("EmployeesWindowTrainer.cs");
			var install = source.Substring(Index(source, "public static void Install()"));
			install = install.Substring(0, Index(install, "private static void SetInspiration"));

			Assert.True(Index(install, "try") < Index(install, "PlaceInElement"));
			Assert.True(Index(install, "PlaceInElement") < Index(install, "finally"));
			Assert.True(Index(install, "finally") < Index(install, "Object.Destroy"));
			Assert.True(Index(install, "Object.Destroy") < Index(install, "_installedOn = target;"));
			Assert.DoesNotContain("catch", install);
		}

		[Fact]
		public void CreatingTheMainButtonsFirstRemovesWhateverAnEarlierAttemptLeft()
		{
			var source = Read("Main.cs");
			var create = source.Substring(Index(source, "public static void CreateUIButtons()"));
			create = create.Substring(0, Index(create, "public override void ConstructOptionsScreen"));

			Assert.True(Index(create, "DestroyUIButtons();") < Index(create, "TrainerButton = UIFactory.Button"));
			Assert.Contains("Object.Destroy(button.gameObject)", source);
		}

		[Fact]
		public void AButtonWithoutItsParentElementIsNotLeftLoose()
		{
			var source = Read("Main.cs");
			var create = source.Substring(Index(source, "public static void CreateUIButtons()"));
			create = create.Substring(0, Index(create, "public override void ConstructOptionsScreen"));

			Assert.Contains("if (!TrainerButton.gameObject.AddToElement(", create);
			Assert.Contains("TrainerButton = null;", create);
			Assert.DoesNotContain("SkillChangeButton", source);
			Assert.DoesNotContain("ActorWindow", source);
		}
	}
}
