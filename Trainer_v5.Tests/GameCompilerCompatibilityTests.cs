using System;
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
		private static readonly Regex EnumDeclaration = new Regex(@"\benum\s+(?<name>\w+)", RegexOptions.Compiled);

		[Fact]
		public void TheScanFindsGenericTypeDeclarations()
		{
			Assert.Matches(GenericType, "public sealed class Key<T> { }");
			Assert.DoesNotMatch(GenericType, "public sealed class Key { T Get<T>() { } }");
			Assert.DoesNotMatch(GenericType, SourceText.Strip("// class Key<T>\nvar s = \"struct Key<T>\";", keepStrings: false));
		}

		[Fact]
		public void EveryEnumIsDeclaredBeforeTheTypesThatHaveAFieldOfIt()
		{
			var files = CompileOrder(RepositoryFiles.SourceDirectory);
			var sources = new Dictionary<string, string>();
			var enums = new Dictionary<string, string>();
			foreach (var file in files)
			{
				sources[file] = SourceText.Strip(File.ReadAllText(file), keepStrings: false);
				foreach (Match match in EnumDeclaration.Matches(sources[file]))
				{
					enums[match.Groups["name"].Value] = file;
				}
			}

			var late = new List<string>();
			foreach (var pair in enums)
			{
				var enumIndex = files.IndexOf(pair.Value);
				var enumPosition = sources[pair.Value].IndexOf("enum " + pair.Key, StringComparison.Ordinal);
				var member = new Regex(@"^\s*(?:(?:public|private|internal|protected|static|readonly|const)\s+)*" + pair.Key + @"\s+\w+\s*(?:=|;|\{\s*get)", RegexOptions.Multiline);
				foreach (var file in files)
				{
					foreach (Match use in member.Matches(sources[file]))
					{
						var isTypeMember = BraceDepth(sources[file], use.Index) == 2;
						var isLate = files.IndexOf(file) < enumIndex || (file == pair.Value && use.Index < enumPosition);
						if (isTypeMember && isLate)
						{
							late.Add(pair.Key + " (" + Path.GetFileName(pair.Value) + ") is used by a field in " + Path.GetFileName(file));
						}
					}
				}
			}

			Assert.True(late.Count == 0, "Declare the enum in a file that sorts before its users: " + string.Join("; ", late));
		}

		[Fact]
		public void TheCompileOrderListsTopLevelFilesBeforeFolders()
		{
			var root = Path.Combine(Path.GetTempPath(), "t5b1-order-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path.Combine(root, "Window"));
			File.WriteAllText(Path.Combine(root, "b.cs"), "");
			File.WriteAllText(Path.Combine(root, "A.cs"), "");
			File.WriteAllText(Path.Combine(root, "Window", "a.cs"), "");
			try
			{
				var names = new List<string>();
				foreach (var file in CompileOrder(root))
				{
					names.Add(file.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar));
				}

				Assert.Equal("A.cs, b.cs, " + Path.Combine("Window", "a.cs"), string.Join(", ", names));
			}
			finally
			{
				Directory.Delete(root, true);
			}
		}

		private static List<string> CompileOrder(string directory)
		{
			var order = new List<string>();
			var files = new List<string>(Directory.GetFiles(directory, "*.cs"));
			files.Sort(StringComparer.OrdinalIgnoreCase);
			order.AddRange(files);
			var folders = new List<string>(Directory.GetDirectories(directory));
			folders.Sort(StringComparer.OrdinalIgnoreCase);
			foreach (var folder in folders)
			{
				order.AddRange(CompileOrder(folder));
			}

			return order;
		}

		private static int BraceDepth(string text, int index)
		{
			var depth = 0;
			for (var i = 0; i < index; i++)
			{
				if (text[i] == (char)123)
				{
					depth++;
				}
				else if (text[i] == (char)125)
				{
					depth--;
				}
			}

			return depth;
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
