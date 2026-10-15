using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Trainer_v5.Tests
{
	// Runs the game's injector and compiler over the real sources (#222, #225): parsing and try placement only.
	public class GameSourcePipelineTests
	{
		private static readonly Lazy<IList<InjectedSource>> Injected = new Lazy<IList<InjectedSource>>(InjectEverySourceFile);
		private static readonly Lazy<ParseCheck> Syntax = new Lazy<ParseCheck>(() => GameCompiler.CheckSyntax(Injected.Value));

		[Fact]
		public void TheHarnessStillMirrorsTheGamesInjector()
		{
			GameSourceInjector.VerifyMirror();
		}

		[Fact]
		public void TheCopyOfTheGamesCompilerDiffersByTheBitnessFlagOnly()
		{
			var original = File.ReadAllBytes(GameCompiler.AssemblyPath);
			var copy = PeImage.WithoutRequires32BitProcess(original);

			Assert.False(PeImage.Requires32BitProcess(copy));
			Assert.Equal(original.Length, copy.Length);
			var differing = 0;
			for (var i = 0; i < original.Length; i++)
			{
				if (original[i] != copy[i])
				{
					differing++;
				}
			}
			Assert.True(differing <= 1, "Only the 32-bit flag may change, but " + differing + " bytes differ.");
		}

		[Fact]
		public void TheGamesCompilerIsAvailable()
		{
			GameCompiler.VerifyAvailable();
		}

		[Fact]
		public void TheGamesInjectorOnlyAddsText()
		{
			var offenders = Enumerable.Where(Injected.Value, file => !IsSubsequence(WithoutWhitespace(file.Stripped), WithoutWhitespace(file.Injected))).Select(file => file.Name).ToList();
			Assert.True(offenders.Count == 0, "The injector dropped or reordered original text in: " + string.Join(", ", offenders));
		}

		[Fact]
		public void EverySourceFileParsesWithTheGamesCompilerAfterInjection()
		{
			var check = Syntax.Value;
			Assert.True(check.Passed, "The game's compiler rejects the injected source (as it did for v5.3.2, #222):" + Environment.NewLine + check.Describe());
		}

		[Fact]
		public void EveryInjectedTryBlockWrapsAMethodOrConstructorBody()
		{
			var misplaced = new List<string>();
			var wrapped = 0;
			foreach (var file in Injected.Value)
			{
				var audit = InjectedSourceAudit.Audit(file);
				wrapped += audit.TryBlocks;
				misplaced.AddRange(audit.Misplaced);
			}

			Assert.True(misplaced.Count == 0, "The injector wrapped something that is not a method or constructor body:" + Environment.NewLine + string.Join(Environment.NewLine, misplaced));
			Assert.True(wrapped > 100, "Expected the injector to wrap well over a hundred bodies in the trainer, found " + wrapped + ". The injector or this check has changed.");
		}

		[Fact]
		public void NoMethodBodyEndsInAnUnconditionalThrow()
		{
			var offenders = new List<string>();
			foreach (var file in Injected.Value)
			{
				offenders.AddRange(InjectedSourceAudit.FindBodiesEndingInThrow(file.Name, file.Original));
			}

			Assert.True(offenders.Count == 0, "The game's compiler emits invalid IL for a wrapped body that has no return and ends in a throw; guard the throw with a condition:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
		}

		private static string WithoutWhitespace(string text)
		{
			return new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
		}

		private static bool IsSubsequence(string part, string whole)
		{
			var next = 0;
			foreach (var c in whole)
			{
				if (next < part.Length && part[next] == c)
				{
					next++;
				}
			}
			return next == part.Length;
		}

		private static IList<InjectedSource> InjectEverySourceFile()
		{
			var root = RepositoryFiles.SourceDirectory;
			return Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
				.OrderBy(path => path, StringComparer.Ordinal)
				.Select(path => GameSourceInjector.Inject(path.Substring(root.Length).TrimStart('\\', '/').Replace('\\', '/'), File.ReadAllText(path)))
				.ToList();
		}
	}
}
