using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class FeatureBoundaryTests
	{
		private static string[] FeatureFiles()
		{
			return Directory.GetFiles(Path.Combine(RepositoryFiles.SourceDirectory, "Features"), "*.cs", SearchOption.AllDirectories);
		}

		private static string CodeOf(string file)
		{
			return SourceText.Strip(File.ReadAllText(file), keepStrings: false);
		}

		private static List<string> FilesMatching(Regex pattern)
		{
			var offenders = new List<string>();
			foreach (var file in FeatureFiles())
			{
				if (pattern.IsMatch(CodeOf(file)))
				{
					offenders.Add(Path.GetFileName(file));
				}
			}
			return offenders;
		}

		[Fact]
		public void FeatureClassesDoNotOwnTheRuntimeErrorBoundary()
		{
			Assert.Empty(FilesMatching(new Regex(@"\bTryExecute\b")));
		}

		[Fact]
		public void FeatureClassesDoNotDependOnEachOther()
		{
			var offenders = new List<string>();
			foreach (var file in FeatureFiles())
			{
				var own = Path.GetFileNameWithoutExtension(file);
				var text = CodeOf(file);
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
