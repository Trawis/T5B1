using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Scheduling, error boundaries and toggle transitions belong to TrainerBehaviour; the domain
	// classes under Features/ only mutate game state, and none of them depends on another.
	public class FeatureBoundaryTests
	{
		private static string[] FeatureFiles()
		{
			return Directory.GetFiles(Path.Combine(RepositoryFiles.SourceDirectory, "Features"), "*.cs");
		}

		private static List<string> FilesMatching(Regex pattern)
		{
			var offenders = new List<string>();
			foreach (var file in FeatureFiles())
			{
				if (pattern.IsMatch(File.ReadAllText(file)))
				{
					offenders.Add(Path.GetFileName(file));
				}
			}
			return offenders;
		}

		[Fact]
		public void TheFeatureClassesAreFound()
		{
			Assert.True(FeatureFiles().Length >= 6);
		}

		[Fact]
		public void FeatureClassesDoNotOwnTheRuntimeErrorBoundary()
		{
			Assert.Empty(FilesMatching(new Regex(@"\bTryExecute\b")));
		}

		[Fact]
		public void FeatureClassesDoNotDependOnTheOrchestrationClass()
		{
			Assert.Empty(FilesMatching(new Regex(@"\bTrainerBehaviour\b")));
		}

		[Fact]
		public void FeatureClassesDoNotDependOnEachOther()
		{
			var offenders = new List<string>();
			foreach (var file in FeatureFiles())
			{
				var own = Path.GetFileNameWithoutExtension(file);
				var text = File.ReadAllText(file);
				foreach (Match match in Regex.Matches(text, @"\b(\w+Features)\."))
				{
					if (match.Groups[1].Value != own)
					{
						offenders.Add(own + " -> " + match.Groups[1].Value);
					}
				}
			}

			Assert.Empty(offenders);
		}
	}
}
