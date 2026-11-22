using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Trainer source is read as text only for rules about the source itself. Wiring is checked on the compiled
	// assembly with CompiledCalls, and decisions by calling them.
	public class SourceTextAllowListTests
	{
		private static readonly Dictionary<string, string> Allowed = new Dictionary<string, string>
		{
			{ "GameSourcePipelineTests.cs", "runs the game's injector and compiler on the source the Workshop ships" },
			{ "GameCompilerCompatibilityTests.cs", "generic class declarations, which the game's compiler turns into invalid IL" },
			{ "InjectedSourceAudit.cs", "audits the injected source the game's compiler receives" },
			{ "LoggingOptionsTests.cs", "every LogDebug inside its if block, which IL order cannot show" },
			{ "RepositoryFiles.cs", "locates the source directory" },
			{ "SourceCode.cs", "the source-text reader itself" },
			{ "SourceText.cs", "the comment and string stripper itself" },
		};

		[Fact]
		public void OnlySourceShapeTestsReadTheTrainerSourceAsText()
		{
			var reads = new Regex(@"\b(?:SourceCode\.|SourceText\.|SourceDirectory\b)");
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(Path.Combine(RepositoryFiles.Root, "tests"), "*.cs"))
			{
				var name = Path.GetFileName(file);
				if (name != "SourceTextAllowListTests.cs" && !Allowed.ContainsKey(name) && reads.IsMatch(File.ReadAllText(file)))
				{
					offenders.Add(name);
				}
			}

			Assert.True(offenders.Count == 0, "These tests read trainer source as text; check wiring with CompiledCalls or call the decision instead: " + string.Join(", ", offenders));
		}
	}
}
