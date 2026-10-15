using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Trainer_v5.Tests
{
	// One source file after the game's source-mod injector has rewritten it.
	internal sealed class InjectedSource
	{
		public InjectedSource(string name, string original, string stripped, string injected, int prefixLines)
		{
			Name = name;
			Original = original;
			Stripped = stripped;
			Injected = injected;
			PrefixLines = prefixLines;
		}

		public string Name { get; }

		public string Original { get; }

		public string Stripped { get; }

		public string Injected { get; }

		public int PrefixLines { get; }
	}

	// Test-only bridge to the game's injector, which cannot run outside Unity: Inject repeats its loop, VerifyMirror detects drift.
	internal static class GameSourceInjector
	{
		public const string ModName = "t5b1tests";

		private const BindingFlags Members = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly Type ModControllerType = typeof(ModController);

		// Serializes calls into the game's static helpers; tests run in parallel.
		private static readonly object Gate = new object();

		private static readonly string[] ExpectedGameCalls =
		{
			"ModController.FindNextClassOrStruct",
			"ModController.HandleObjectParsing",
			"ModController.SkipTo",
			"ModController.StripComments",
			"Options.get_InjectMods",
			"Versioning.get_AlmostExtremelySimpleVersionString",
			"Versioning.get_ExtremelySimpleVersionString",
			"Versioning.get_TypeVersionString",
		};

		public static void VerifyMirror()
		{
			Require("InjectTryBlocks", typeof(string), typeof(string));
			Require("StripComments", typeof(string));
			Require("FindNextClassOrStruct", typeof(string), typeof(int).MakeByRefType());
			Require("SkipTo", typeof(string), typeof(int).MakeByRefType(), typeof(char));
			Require("HandleObjectParsing", typeof(string), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(StringBuilder), typeof(string));

			var called = IlReader.CalledMethods(Require("InjectTryBlocks", typeof(string), typeof(string)));
			var gameCalls = new SortedSet<string>(called.Where(call => IsGameType(call)));
			var expected = new SortedSet<string>(ExpectedGameCalls);
			if (!gameCalls.SetEquals(expected))
			{
				throw new InvalidOperationException(
					"ModController.InjectTryBlocks changed, so GameSourceInjector no longer mirrors it. Re-derive the loop from the new IL."
					+ Environment.NewLine + "expected game calls: " + string.Join(", ", expected)
					+ Environment.NewLine + "actual game calls:   " + string.Join(", ", gameCalls));
			}
		}

		public static InjectedSource Inject(string name, string source)
		{
			lock (Gate)
			{
				return InjectLocked(name, source);
			}
		}

		private static InjectedSource InjectLocked(string name, string source)
		{
			var prefix = VersionDefines();
			var stripped = (string)Require("StripComments", typeof(string)).Invoke(null, new object[] { prefix + source });
			var find = Require("FindNextClassOrStruct", typeof(string), typeof(int).MakeByRefType());
			var skip = Require("SkipTo", typeof(string), typeof(int).MakeByRefType(), typeof(char));
			var parse = Require("HandleObjectParsing", typeof(string), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(StringBuilder), typeof(string));

			var builder = new StringBuilder();
			var i = 0;
			var j = 0;
			var stalled = 0;
			try
			{
				while (i < stripped.Length)
				{
					var before = i;
					var findArgs = new object[] { stripped, i };
					var found = (bool)find.Invoke(null, findArgs);
					i = (int)findArgs[1];
					if (found)
					{
						var skipArgs = new object[] { stripped, i, '{' };
						var skipped = (bool)skip.Invoke(null, skipArgs);
						i = (int)skipArgs[1];
						if (skipped)
						{
							var parseArgs = new object[] { stripped, i, j, builder, ModName };
							parse.Invoke(null, parseArgs);
							i = (int)parseArgs[1];
							j = (int)parseArgs[2];
						}
					}

					stalled = i == before ? stalled + 1 : 0;
					if (stalled > 100)
					{
						throw new InvalidOperationException("The game's injector stopped advancing at offset " + i + ".");
					}
				}

				builder.Append(stripped.Substring(j, i - j));
			}
			catch (TargetInvocationException ex)
			{
				throw new InvalidOperationException("The game's source injector threw while reading " + name + ": " + ex.InnerException, ex.InnerException);
			}

			return new InjectedSource(name, source, stripped, builder.ToString(), CountLines(prefix));
		}

		private static string VersionDefines()
		{
			var versioning = ModControllerType.Assembly.GetType("Versioning", true);
			return "#define SWINC" + Upper(versioning, "AlmostExtremelySimpleVersionString")
				+ "\n#define SWINC" + Upper(versioning, "ExtremelySimpleVersionString")
				+ "\n#define SWINC" + Upper(versioning, "TypeVersionString")
				+ "\n";
		}

		private static string Upper(Type versioning, string property)
		{
			var getter = versioning.GetMethod("get_" + property, Members);
			if (getter == null)
			{
				throw new InvalidOperationException("Versioning." + property + " is missing from the vendored game assembly.");
			}
			return ((string)getter.Invoke(null, null)).ToUpperInvariant();
		}

		private static int CountLines(string text)
		{
			return text.Count(c => c == '\n');
		}

		private static bool IsGameType(string call)
		{
			var type = call.Substring(0, call.LastIndexOf('.'));
			return ModControllerType.Assembly.GetType(type, false) != null;
		}

		private static MethodInfo Require(string name, params Type[] parameters)
		{
			var method = ModControllerType.GetMethod(name, Members, null, parameters, null);
			if (method == null)
			{
				throw new InvalidOperationException("ModController." + name + "(" + string.Join(", ", Enumerable.Select(parameters, p => p.Name)) + ") is missing from the vendored game assembly, so the source injector can no longer be exercised.");
			}
			return method;
		}
	}
}
