using System;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Disable Burglars / Disable Fire Inspection remove the actors with the game's own destroy call instead of only setting the employee-only Despawned flag (#362).
	public class ActorRemovalTests
	{
		private static string Source()
		{
			return File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs"));
		}

		private static string Method(string source, string signature)
		{
			var start = source.IndexOf(signature, StringComparison.Ordinal);
			Assert.True(start >= 0, signature);
			var end = source.IndexOf("\t\t}", start, StringComparison.Ordinal);
			return source.Substring(start, end - start);
		}

		[Fact]
		public void BothTogglesRemoveTheirActorGroup()
		{
			var source = Source();

			Assert.Contains("RemoveActorsOfGroup(\"Burglars\");", Method(source, "internal static void ApplyDisableBurglars"));
			Assert.Contains("RemoveActorsOfGroup(\"FireInspector\");", Method(source, "internal static void ApplyDisableFireInspection"));
		}

		[Fact]
		public void ActorsAreDestroyedThroughTheGameCallFromASnapshot()
		{
			var removal = Method(Source(), "private static void RemoveActorsOfGroup");

			Assert.Contains("new List<Actor>(Settings.sActorManager.Others[group])", removal);
			Assert.Contains("RemoveFromAwaiting(actors[i]);", removal);
			Assert.Contains("actors[i].DestroyGO();", removal);
			Assert.DoesNotContain("Despawned", removal);
		}

		[Fact]
		public void FireInspectionStillResetsTheReportAndPassesTheInspection()
		{
			var method = Method(Source(), "internal static void ApplyDisableFireInspection");

			Assert.Contains("Settings.ActiveFireReport.Reset();", method);
			Assert.Contains("Settings.PassedFireInspection = true;", method);
		}
	}
}
