using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class VersionTests
	{
		[Fact]
		public void Version_IsMajorMinorPatch()
		{
			Assert.Matches(@"^\d+\.\d+\.\d+$", Helpers.Version);
		}

		[Fact]
		public void TrainerVersion_PrefixesTheVersion()
		{
			Assert.Equal("Trainer v" + Helpers.Version, Helpers.TrainerVersion);
		}

		[Fact]
		public void Changelog_HasAnEntryForTheCurrentVersion()
		{
			var changelog = File.ReadAllText(Path.Combine(RepositoryFiles.Root, "CHANGELOG.md"));

			Assert.Matches("(?m)^## \\[" + Regex.Escape(Helpers.Version) + "\\] - \\d{4}-\\d{2}-\\d{2}\\s*$", changelog);
		}

		[Theory]
		[InlineData("Programmer", Employee.EmployeeRole.Programmer)]
		[InlineData("Lead", Employee.EmployeeRole.Lead)]
		[InlineData("Artist", Employee.EmployeeRole.Artist)]
		public void ToEmployeeRole_ParsesKnownRoleNames(string name, Employee.EmployeeRole expected)
		{
			Assert.Equal(expected, name.ToEmployeeRole());
		}

		[Fact]
		public void ToEmployeeRole_FallsBackToProgrammerForUnknownNames()
		{
			Assert.Equal(Employee.EmployeeRole.Programmer, "NotARole".ToEmployeeRole());
		}
	}
}
