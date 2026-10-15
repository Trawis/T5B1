using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Creativity editor replaces the Employee object (#183); the guard needs the game's objects, so this checks it is wired in.
	public class CreativityEditGuardTests
	{
		[Fact]
		public void TheCreativityEditorChecksTheGuardBeforeAskingAndAgainBeforeReplacing()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "DetailWindowTrainer.cs"));
			var start = source.IndexOf("private static void SetCreativity", System.StringComparison.Ordinal);
			var end = source.IndexOf("private static void SetInspiration", System.StringComparison.Ordinal);
			var body = source.Substring(start, end - start);

			Assert.Equal(2, Regex.Matches(body, @"CreativityEditGuard\.IsEnabled\(\)").Count);
			Assert.True(body.LastIndexOf("CreativityEditGuard.IsEnabled()", System.StringComparison.Ordinal) > body.IndexOf("RequestFloat", System.StringComparison.Ordinal), "the deferred callback must re-check Experimental");
			Assert.Equal(2, Regex.Matches(body, @"CreativityEditGuard\.FindBlockingReference\(").Count);
			Assert.True(body.IndexOf("FindBlockingReference", System.StringComparison.Ordinal) < body.IndexOf("RequestFloat", System.StringComparison.Ordinal));
			Assert.True(body.LastIndexOf("FindBlockingReference", System.StringComparison.Ordinal) < body.IndexOf("new Employee(", System.StringComparison.Ordinal));
		}

		[Fact]
		public void TheNewEmployeeIsInstalledBeforeTheReferencesAreMigratedToIt()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "DetailWindowTrainer.cs"));
			var start = source.IndexOf("private static void SetCreativity", System.StringComparison.Ordinal);
			var body = source.Substring(start, source.IndexOf("private static void SetInspiration", System.StringComparison.Ordinal) - start);

			Assert.True(body.IndexOf("actor.employee = newEmployee", System.StringComparison.Ordinal) < body.IndexOf("EmployeeReferenceMigration.Migrate(employee, newEmployee)", System.StringComparison.Ordinal));
		}
	}
}
