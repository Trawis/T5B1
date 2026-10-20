using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Trainer_v5.Tests
{
	// Needs the game's Managed folder: set T5B1_GAME_MANAGED, or install Software Inc in a default Steam folder.
	public sealed class GameManagedFactAttribute : FactAttribute
	{
		public GameManagedFactAttribute()
		{
			if (FullGameCompileTests.ManagedDirectory() == null)
			{
				Skip = "The game's Managed folder was not found; set T5B1_GAME_MANAGED to run the full compile.";
			}
		}
	}

	// Compiles the injected Trainer.Source with the game's own compiler against the game's assemblies and writes the assembly,
	// the stage the parse-only pipeline tests skip (e.g. CS0016 for a field of an enum declared in a later file).
	public class FullGameCompileTests
	{
		private static readonly string[] DefaultFolders =
		{
			@"C:\Program Files (x86)\Steam\steamapps\common\Software Inc\Software Inc_Data\Managed",
			@"C:\Program Files\Steam\steamapps\common\Software Inc\Software Inc_Data\Managed",
			@"D:\SteamLibrary\steamapps\common\Software Inc\Software Inc_Data\Managed"
		};

		internal static string ManagedDirectory()
		{
			var configured = Environment.GetEnvironmentVariable("T5B1_GAME_MANAGED");
			var candidates = new List<string>();
			if (!string.IsNullOrEmpty(configured))
			{
				candidates.Add(configured);
			}

			candidates.AddRange(DefaultFolders);
			foreach (var folder in candidates)
			{
				if (Directory.Exists(folder) && File.Exists(Path.Combine(folder, "Assembly-CSharp.dll")))
				{
					return folder;
				}
			}

			return null;
		}

		[GameManagedFact]
		public void TheInjectedSourcesCompileAndTheAssemblyIsWritten()
		{
			var managed = ManagedDirectory();
			var root = RepositoryFiles.SourceDirectory;
			var directory = Path.Combine(Path.GetTempPath(), "t5b1-full-compile-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);
			try
			{
				var output = Path.Combine(directory, "Trainer.dll");
				var arguments = new List<string> { "-target:library", "-out:" + output, "-r:System.dll", "-r:System.Core.dll", "-r:System.Xml.dll" };
				foreach (var assembly in Directory.GetFiles(managed, "*.dll"))
				{
					if (IsReference(Path.GetFileName(assembly)))
					{
						arguments.Add("-r:" + assembly);
					}
				}

				var index = 0;
				foreach (var path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
				{
					var name = path.Substring(root.Length).TrimStart('\\', '/').Replace('\\', '/');
					var copy = Path.Combine(directory, index.ToString("d4") + "_" + Path.GetFileName(path));
					File.WriteAllText(copy, GameSourceInjector.Inject(name, File.ReadAllText(path)).Injected, new UTF8Encoding(false));
					arguments.Add(copy);
					index++;
				}

				var run = GameCompiler.Run(arguments, output);
				var errors = Enumerable.ToList(Enumerable.Select(Enumerable.Where(run.Diagnostics, d => d.IsError), d => d.Code + " " + d.Message));
				Assert.True(run.Succeeded && errors.Count == 0, "The game's compiler failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
				Assert.NotNull(run.Assembly);
			}
			finally
			{
				try
				{
					Directory.Delete(directory, true);
				}
				catch (IOException)
				{
				}
			}
		}

		private static bool IsReference(string file)
		{
			return file.StartsWith("UnityEngine", StringComparison.Ordinal)
				|| file.StartsWith("Assembly-CSharp", StringComparison.Ordinal)
				|| file.StartsWith("com.rlabrecque", StringComparison.Ordinal)
				|| file.StartsWith("Whinarn", StringComparison.Ordinal)
				|| file == "DOTween.dll" || file == "DOTween46.dll" || file == "Mono.Cecil.dll" || file == "SSAA.dll" || file == "Unity.TextMeshPro.dll";
		}
	}
}
