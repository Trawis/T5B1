using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Stage and status strings the game returns are translated, so comparing them to English text only works in English (#197).
	public class LocalizedGameTextTests
	{
		private static readonly Regex StageComparison = new Regex(@"\bCurrentStage\(\)\s*[!=]=", RegexOptions.Compiled);

		[Fact]
		public void TheScanFindsAStageComparison()
		{
			Assert.Matches(StageComparison, "work.CurrentStage() == \"Finished\"");
			Assert.DoesNotMatch(StageComparison, "var stage = work.CurrentStage();");
		}

		[Fact]
		public void NoFeatureComparesTheGamesStageTextToAString()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				if (StageComparison.IsMatch(SourceText.Strip(File.ReadAllText(file), keepStrings: true)))
				{
					offenders.Add(Path.GetFileName(file));
				}
			}

			Assert.True(offenders.Count == 0, "CurrentStage() returns translated text; test the work item's state instead: " + string.Join(", ", offenders));
		}
	}
}
