using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class VersionTests
	{
		[Fact]
		public void Changelog_HasAnEntryForTheCurrentVersion()
		{
			var changelog = File.ReadAllText(Path.Combine(RepositoryFiles.Root, "CHANGELOG.md"));

			Assert.Matches("(?m)^## \\[" + Regex.Escape(Helpers.Version) + "\\] - \\d{4}-\\d{2}-\\d{2}\\s*$", changelog);
		}
	}
}
