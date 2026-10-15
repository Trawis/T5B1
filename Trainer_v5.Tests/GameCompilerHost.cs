using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Trainer_v5.Tests
{
	internal sealed class CompilerDiagnostic
	{
		public CompilerDiagnostic(string file, int line, int column, bool isError, string code, string message)
		{
			File = file;
			Line = line;
			Column = column;
			IsError = isError;
			Code = code;
			Message = message;
		}

		public string File { get; }

		public int Line { get; }

		public int Column { get; }

		public bool IsError { get; }

		public string Code { get; }

		public string Message { get; }
	}

	internal sealed class CompilerRun
	{
		public CompilerRun(bool succeeded, IList<CompilerDiagnostic> diagnostics, byte[] assembly)
		{
			Succeeded = succeeded;
			Diagnostics = diagnostics;
			Assembly = assembly;
		}

		public bool Succeeded { get; }

		public IList<CompilerDiagnostic> Diagnostics { get; }

		public byte[] Assembly { get; }

		public IEnumerable<CompilerDiagnostic> Errors => Enumerable.Where(Diagnostics, d => d.IsError);
	}

	// Test-only host for the game's bundled C# compiler (Trainer.Libraries/mcs.dll).
	internal static class GameCompiler
	{
		private static readonly object Gate = new object();
		private static readonly Regex Located = new Regex(@"^(?<file>.*)\((?<line>\d+),(?<column>\d+)\): (?<kind>error|warning) (?<code>CS\d+): (?<message>.*)$", RegexOptions.Compiled);
		private static readonly Regex Unlocated = new Regex(@"^(?<kind>error|warning) (?<code>CS\d+): (?<message>.*)$", RegexOptions.Compiled);
		private static MethodInfo _entryPoint;

		public static string AssemblyPath => Path.Combine(RepositoryFiles.Root, "Trainer_v5", "Trainer.Libraries", "mcs.dll");

		public static void VerifyAvailable()
		{
			lock (Gate)
			{
				EntryPoint();
			}
		}

		public static CompilerRun Run(IEnumerable<string> arguments, string outputPath)
		{
			lock (Gate)
			{
				var writer = new StringWriter(CultureInfo.InvariantCulture);
				bool succeeded;
				try
				{
					succeeded = (bool)EntryPoint().Invoke(null, new object[] { arguments.ToArray(), writer });
				}
				catch (TargetInvocationException ex)
				{
					throw new InvalidOperationException("The game's compiler threw: " + ex.InnerException, ex.InnerException);
				}
				var assembly = succeeded && outputPath != null && File.Exists(outputPath) ? File.ReadAllBytes(outputPath) : null;
				return new CompilerRun(succeeded, Parse(writer.ToString()), assembly);
			}
		}

		// Syntax check only: mcs reports syntax errors before it loads references, so a reference that cannot
		// exist leaves exactly one CS0006 when the sources parse. The vendored libraries are a subset of the
		// game's assemblies, so mcs cannot resolve types here; the SDK build does that.
		public static ParseCheck CheckSyntax(IList<InjectedSource> sources)
		{
			var directory = CreateTemporaryDirectory();
			try
			{
				var files = new Dictionary<string, InjectedSource>(StringComparer.OrdinalIgnoreCase);
				var arguments = new List<string> { "-target:library", "-out:" + Path.Combine(directory, "syntax.dll") };
				var sentinel = Path.Combine(directory, "reference-that-does-not-exist.dll");
				arguments.Add("-r:" + sentinel);
				for (var i = 0; i < sources.Count; i++)
				{
					var path = Path.Combine(directory, i.ToString("d3", CultureInfo.InvariantCulture) + "_" + Path.GetFileName(sources[i].Name));
					File.WriteAllText(path, sources[i].Injected, new UTF8Encoding(false));
					files[path] = sources[i];
					arguments.Add(path);
				}

				var run = Run(arguments, null);
				var sentinelReported = run.Errors.Any(e => e.Code == "CS0006" && e.Message.IndexOf(sentinel, StringComparison.OrdinalIgnoreCase) >= 0);
				var problems = run.Errors.Where(e => e.Code != "CS0006").ToList();
				return new ParseCheck(sentinelReported, Enumerable.Select(problems, p => Locate(p, files)).ToList());
			}
			finally
			{
				DeleteQuietly(directory);
			}
		}

		public static CompilerRun CompileStandalone(string name, params string[] sources)
		{
			var directory = CreateTemporaryDirectory();
			try
			{
				var assembly = Path.Combine(directory, name + ".dll");
				var arguments = new List<string> { "-target:library", "-out:" + assembly };
				for (var i = 0; i < sources.Length; i++)
				{
					var path = Path.Combine(directory, name + i.ToString(CultureInfo.InvariantCulture) + ".cs");
					File.WriteAllText(path, sources[i], new UTF8Encoding(false));
					arguments.Add(path);
				}
				return Run(arguments, assembly);
			}
			finally
			{
				DeleteQuietly(directory);
			}
		}

		public static Assembly LoadCompiled(CompilerRun run)
		{
			return Assembly.Load(run.Assembly);
		}

		private static MethodInfo EntryPoint()
		{
			if (_entryPoint != null)
			{
				return _entryPoint;
			}

			if (!File.Exists(AssemblyPath))
			{
				throw new FileNotFoundException("The game's C# compiler is missing. Copy mcs.dll from the game's Managed folder to Trainer_v5/Trainer.Libraries/mcs.dll.", AssemblyPath);
			}

			var type = Assembly.LoadFrom(LoadableCopy()).GetType("Mono.CSharp.CompilerCallableEntryPoint", false);
			var method = type == null ? null : type.GetMethod("InvokeCompiler", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string[]), typeof(TextWriter) }, null);
			if (method == null)
			{
				throw new InvalidOperationException("Mono.CSharp.CompilerCallableEntryPoint.InvokeCompiler(string[], TextWriter) is missing from " + AssemblyPath + ", so the game's compiler can no longer be driven the way DynamicCSharp drives it.");
			}

			_entryPoint = method;
			return method;
		}

		// mcs.dll is flagged 32-bit-only, which a 64-bit test host rejects; load a copy with that flag cleared.
		private static string LoadableCopy()
		{
			var image = PeImage.WithoutRequires32BitProcess(File.ReadAllBytes(AssemblyPath));
			byte[] hash;
			using (var sha = SHA256.Create())
			{
				hash = sha.ComputeHash(image);
			}

			var directory = Path.Combine(Path.GetTempPath(), "t5b1-game-compiler-" + BitConverter.ToString(hash, 0, 8).Replace("-", string.Empty));
			var path = Path.Combine(directory, "mcs.dll");
			if (!File.Exists(path) || new FileInfo(path).Length != image.Length)
			{
				Directory.CreateDirectory(directory);
				try
				{
					File.WriteAllBytes(path, image);
				}
				catch (IOException)
				{
					// Another process wrote the identical copy first.
					if (!File.Exists(path))
					{
						throw;
					}
				}
			}
			return path;
		}

		private static IList<CompilerDiagnostic> Parse(string output)
		{
			var diagnostics = new List<CompilerDiagnostic>();
			foreach (var raw in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var located = Located.Match(raw);
				if (located.Success)
				{
					diagnostics.Add(new CompilerDiagnostic(
						located.Groups["file"].Value,
						int.Parse(located.Groups["line"].Value, CultureInfo.InvariantCulture),
						int.Parse(located.Groups["column"].Value, CultureInfo.InvariantCulture),
						located.Groups["kind"].Value == "error",
						located.Groups["code"].Value,
						located.Groups["message"].Value));
					continue;
				}

				var unlocated = Unlocated.Match(raw);
				if (unlocated.Success)
				{
					diagnostics.Add(new CompilerDiagnostic(null, 0, 0, unlocated.Groups["kind"].Value == "error", unlocated.Groups["code"].Value, unlocated.Groups["message"].Value));
				}
			}
			return diagnostics;
		}

		private static MappedDiagnostic Locate(CompilerDiagnostic diagnostic, Dictionary<string, InjectedSource> files)
		{
			InjectedSource source = null;
			if (diagnostic.File != null && !files.TryGetValue(diagnostic.File, out source))
			{
				var fileName = Path.GetFileName(diagnostic.File);
				source = files.Where(f => string.Equals(Path.GetFileName(f.Key), fileName, StringComparison.OrdinalIgnoreCase)).Select(f => f.Value).FirstOrDefault();
			}
			return source == null
				? new MappedDiagnostic(diagnostic, null, null)
				: new MappedDiagnostic(diagnostic, source, new GameSourceMap(source).Locate(diagnostic.Line, diagnostic.Column));
		}

		private static string CreateTemporaryDirectory()
		{
			var directory = Path.Combine(Path.GetTempPath(), "t5b1-game-compiler-" + Guid.NewGuid().ToString("n"));
			Directory.CreateDirectory(directory);
			return directory;
		}

		private static void DeleteQuietly(string directory)
		{
			try
			{
				Directory.Delete(directory, true);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}

	internal sealed class ParseCheck
	{
		public ParseCheck(bool reachedReferenceLoading, IList<MappedDiagnostic> problems)
		{
			ReachedReferenceLoading = reachedReferenceLoading;
			Problems = problems;
		}

		public bool ReachedReferenceLoading { get; }

		public IList<MappedDiagnostic> Problems { get; }

		public bool Passed => ReachedReferenceLoading && Problems.Count == 0;

		public string Describe()
		{
			if (Problems.Count == 0 && !ReachedReferenceLoading)
			{
				return "The game's compiler neither accepted the sources nor reported a syntax error, so the check proves nothing.";
			}
			const int shown = 12;
			var lines = Enumerable.Select(Enumerable.Take(Problems, shown), p => p.Describe()).ToList();
			if (Problems.Count > shown)
			{
				lines.Add("... and " + (Problems.Count - shown) + " more");
			}
			lines.Add("The position is where the game's compiler stopped, mapped back to the repository file; the cause is often an earlier member. Known causes: a call in a property or field initializer (#222), a brace in a char literal, or an apostrophe in a string before a comment containing a brace.");
			return string.Join(Environment.NewLine, lines);
		}
	}

	internal sealed class MappedDiagnostic
	{
		public MappedDiagnostic(CompilerDiagnostic diagnostic, InjectedSource source, OriginalPosition position)
		{
			Diagnostic = diagnostic;
			Source = source;
			Position = position;
		}

		public CompilerDiagnostic Diagnostic { get; }

		public InjectedSource Source { get; }

		public OriginalPosition Position { get; }

		public string Describe()
		{
			if (Source == null || Position == null)
			{
				return Diagnostic.Code + ": " + Diagnostic.Message;
			}

			var text = new StringBuilder();
			text.Append(Source.Name).Append('(').Append(Position.Line).Append(',').Append(Position.Column).Append("): ")
				.Append(Diagnostic.Code).Append(": ").Append(Diagnostic.Message);
			text.Append(Environment.NewLine).Append("    ").Append(Position.Description);
			text.Append(" [injected source line ").Append(Diagnostic.Line).Append(", column ").Append(Diagnostic.Column).Append(']');
			return text.ToString();
		}
	}
}
