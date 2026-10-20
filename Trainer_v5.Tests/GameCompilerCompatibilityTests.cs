using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// The game's compiler emits invalid IL for fields of user-defined generic types; generic methods and delegates are fine.
	public class GameCompilerCompatibilityTests
	{
		private static readonly Regex GenericType = new Regex(@"\b(class|struct)\s+\w+\s*<", RegexOptions.Compiled);

		[Fact]
		public void TheScanFindsGenericTypeDeclarations()
		{
			Assert.Matches(GenericType, "public sealed class Key<T> { }");
			Assert.DoesNotMatch(GenericType, "public sealed class Key { T Get<T>() { } }");
			Assert.DoesNotMatch(GenericType, SourceText.Strip("// class Key<T>\nvar s = \"struct Key<T>\";", keepStrings: false));
		}

		[Fact]
		public void SourceDeclaresNoGenericClassesOrStructs()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				if (GenericType.IsMatch(SourceText.Strip(File.ReadAllText(file), keepStrings: false)))
				{
					offenders.Add(Path.GetFileName(file));
				}
			}

			Assert.Empty(offenders);
		}
	}
}
