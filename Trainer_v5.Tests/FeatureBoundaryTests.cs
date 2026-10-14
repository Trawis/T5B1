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
		public void TheFeatureClassesAreFound()
		{
			var files = FeatureFiles();
			Assert.True(files.Length >= 6);
			foreach (var file in files)
			{
				Assert.EndsWith("Features.cs", file);
				Assert.Contains("static class " + Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
			}
		}

		[Fact]
		public void TheScanIgnoresCommentsAndStringsButSeesCode()
		{
			var reference = new Regex(@"\bTrainerBehaviour\b");
			Assert.DoesNotMatch(reference, SourceText.Strip("// TrainerBehaviour\n/* TrainerBehaviour */ var a = \"TrainerBehaviour\"; var b = @\"TrainerBehaviour\";", keepStrings: false));
			Assert.Matches(reference, SourceText.Strip("var a = 1; /* x */ TrainerBehaviour.Instance.Run(); // y", keepStrings: false));
		}

		[Fact]
		public void TheScanSeesReferencesInsideInterpolatedExpressions()
		{
			var reference = new Regex(@"\bTrainerBehaviour\b");
			Assert.Matches(reference, SourceText.Strip("var d = $\"{typeof(TrainerBehaviour)}\";", keepStrings: false));
			Assert.Matches(reference, SourceText.Strip("var d = $@\"text {Describe(\"x\", TrainerBehaviour.Instance)} more\";", keepStrings: false));
			Assert.Matches(reference, SourceText.Strip("var d = $\"{{literal}} {(a ? \"}\" : TrainerBehaviour.Name)}\";", keepStrings: false));
			Assert.Matches(reference, SourceText.Strip("var d = $\"a {$\"b {TrainerBehaviour.Name}\"} c\"; var e = 1;", keepStrings: false));
		}

		[Fact]
		public void TheScanIgnoresTextAroundInterpolatedExpressions()
		{
			var reference = new Regex(@"\bTrainerBehaviour\b");
			Assert.DoesNotMatch(reference, SourceText.Strip("var d = $\"TrainerBehaviour {name} TrainerBehaviour\";", keepStrings: false));
			Assert.DoesNotMatch(reference, SourceText.Strip("var d = $\"{{TrainerBehaviour}}\";", keepStrings: false));
			Assert.DoesNotMatch(reference, SourceText.Strip("var d = $\"{(a ? \"TrainerBehaviour\" : b)}\";", keepStrings: false));
			Assert.DoesNotMatch(reference, SourceText.Strip("var d = $@\"TrainerBehaviour \"\"quoted\"\" {x}\"; var c = '{'; // TrainerBehaviour", keepStrings: false));
		}

		[Fact]
		public void TheScanKeepsCodeThatFollowsAnInterpolatedString()
		{
			var reference = new Regex(@"\bTrainerBehaviour\b");
			Assert.Matches(reference, SourceText.Strip("var d = $\"{x}\"; TrainerBehaviour.Run();", keepStrings: false));
			Assert.Matches(reference, SourceText.Strip("var d = $\"{{\"; TrainerBehaviour.Run();", keepStrings: false));
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
