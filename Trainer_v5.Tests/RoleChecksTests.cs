using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Employee.RoleString is display text built from translated role names, so it must not drive logic (#269).
	public class RoleChecksTests
	{
		[Fact]
		public void NoFeatureReadsTheTranslatedRoleString()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				if (SourceText.Strip(File.ReadAllText(file), keepStrings: true).Contains(".RoleString"))
				{
					offenders.Add(Path.GetFileName(file));
				}
			}

			Assert.True(offenders.Count == 0, "Use Employee.IsRole(Employee.RoleBit...) instead of RoleString: " + string.Join(", ", offenders));
		}
	}
}
