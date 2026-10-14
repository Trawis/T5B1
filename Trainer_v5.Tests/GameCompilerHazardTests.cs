using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	// Each failure the Workshop build has produced, or that the game's own tools are known to produce, is
	// kept here as a small fixture run through the game's injector and compiler. They show that the checks
	// in GameSourcePipelineTests would catch the problem, and they fail when a game update changes how the
	// injector or compiler behaves, which is the cue to re-read the game's code.
	public class GameCompilerHazardTests
	{
		// The shape of Helpers.cs in v5.3.2: a call in a property initializer (#222, CS1014 at Helpers.cs(39,2) in game).
		private const string CallInPropertyInitializer =
			"using System.Collections.Generic;\n" +
			"\n" +
			"namespace Fixture\n" +
			"{\n" +
			"\tpublic static class Helpers\n" +
			"\t{\n" +
			"\t\tpublic static Dictionary<string, bool> Settings { get; } = Create();\n" +
			"\n" +
			"\t\tpublic static Dictionary<string, bool> RolesList { get; } = new Dictionary<string, bool>\n" +
			"\t\t{\n" +
			"\t\t\t{\"Lead\", false},\n" +
			"\t\t\t{\"Service\", false}\n" +
			"\t\t};\n" +
			"\n" +
			"\t\tprivate static Dictionary<string, bool> Create()\n" +
			"\t\t{\n" +
			"\t\t\treturn new Dictionary<string, bool>();\n" +
			"\t\t}\n" +
			"\t}\n" +
			"}\n";

		// The same type after the fix: the call moved into a static constructor.
		private const string CallInStaticConstructor =
			"using System.Collections.Generic;\n" +
			"\n" +
			"namespace Fixture\n" +
			"{\n" +
			"\tpublic static class Helpers\n" +
			"\t{\n" +
			"\t\tpublic static Dictionary<string, bool> Settings { get; }\n" +
			"\n" +
			"\t\tpublic static Dictionary<string, bool> RolesList { get; } = new Dictionary<string, bool>\n" +
			"\t\t{\n" +
			"\t\t\t{\"Lead\", false},\n" +
			"\t\t\t{\"Service\", false}\n" +
			"\t\t};\n" +
			"\n" +
			"\t\tstatic Helpers()\n" +
			"\t\t{\n" +
			"\t\t\tSettings = Create();\n" +
			"\t\t}\n" +
			"\n" +
			"\t\tprivate static Dictionary<string, bool> Create()\n" +
			"\t\t{\n" +
			"\t\t\treturn new Dictionary<string, bool>();\n" +
			"\t\t}\n" +
			"\t}\n" +
			"}\n";

		private const string StubTypes =
			"public class ModException : System.Exception { }\n" +
			"public static class ModController { public static void HandleException(string mod, System.Exception e) { } }\n";

		private const string ThrowingBodies =
			"public static class Bodies\n" +
			"{\n" +
			"\tpublic static int Count;\n" +
			"\tpublic static void OnlyThrows() { throw new System.InvalidOperationException(\"x\"); }\n" +
			"\tpublic static void ThrowsAfterWork() { Count++; throw new System.InvalidOperationException(\"x\"); }\n" +
			"\tpublic static void ReturnsThenThrows(bool b) { if (b) { return; } throw new System.InvalidOperationException(\"x\"); }\n" +
			"\tpublic static void ThrowsConditionally(bool b) { if (b) { throw new System.InvalidOperationException(\"x\"); } }\n" +
			"\tpublic static void ThrowsInOneBranch(bool b) { if (b) throw new System.InvalidOperationException(\"x\"); else Count++; }\n" +
			"\tpublic static int ReturnsValue() { return 1; }\n" +
			"}\n";

		[Fact]
		public void ACallInAPropertyInitializerIsRejected()
		{
			var check = Check("Helpers.cs", CallInPropertyInitializer);

			Assert.False(check.Passed);
			var problem = Enumerable.First(check.Problems, p => p.Diagnostic.Code == "CS1014");
			// The game reports the broken accessor; the message maps it back to the property whose braces were rewritten.
			Assert.Equal("Helpers.cs", problem.Source.Name);
			Assert.InRange(problem.Position.Line, 7, 9);
			Assert.Contains("Dictionary<string, bool>", problem.Position.Description);
			Assert.Contains("Helpers.cs(", problem.Describe());
		}

		[Fact]
		public void TheSameTypeIsAcceptedOnceTheCallMovesIntoAStaticConstructor()
		{
			var check = Check("Helpers.cs", CallInStaticConstructor);

			Assert.True(check.Passed, check.Describe());
		}

		[Fact]
		public void ABraceInACharLiteralIsRejected()
		{
			var check = Check("Brace.cs", MethodSource("char open = '{';"));

			Assert.False(check.Passed);
		}

		[Fact]
		public void AnApostropheInAStringBeforeACommentWithABraceIsRejected()
		{
			var check = Check("Apostrophe.cs", MethodSource("string text = \"it's\"; // {"));

			Assert.False(check.Passed);
		}

		[Fact]
		public void BracesAndApostrophesInTheirOwnStringsAndCommentsAreAccepted()
		{
			var check = Check("Strings.cs", MethodSource("string open = \"{\"; string close = \"}\"; // braces in a comment, { and }, are stripped first\n\t\tstring text = \"it's\"; // an apostrophe on its own is fine"));

			Assert.True(check.Passed, check.Describe());
		}

		[Fact]
		public void AParameterNamedExCollidesWithTheInjectedCatchVariable()
		{
			var source =
				"using System;\n" +
				"class Reporter\n" +
				"{\n" +
				"\tpublic void Report(string id, Exception ex)\n" +
				"\t{\n" +
				"\t\tConsole.WriteLine(ex.Message);\n" +
				"\t}\n" +
				"}\n";

			var check = Check("Reporter.cs", source);

			Assert.False(check.Passed);
			Assert.Contains(check.Problems, p => p.Diagnostic.Code == "CS0136");
		}

		[Fact]
		public void TheGamesCompilerIsACSharpSixCompiler()
		{
			var supported =
				"using System;\n" +
				"using static System.Math;\n" +
				"class Six\n" +
				"{\n" +
				"\tpublic int Auto { get; } = 3;\n" +
				"\tpublic int Body => Auto;\n" +
				"\tpublic string Text(string s) { return $\"{nameof(Six)}:{s?.Length}\"; }\n" +
				"\tpublic double Abs2(double d) { return Abs(d); }\n" +
				"\tpublic void Filter() { try { } catch (Exception e) when (e != null) { } }\n" +
				"}\n";
			var unsupported =
				"class Seven\n" +
				"{\n" +
				"\tpublic void Parse(string s) { int.TryParse(s, out var number); }\n" +
				"}\n";

			Assert.True(Check("Six.cs", supported).Passed);
			var seven = Check("Seven.cs", unsupported);
			Assert.False(seven.Passed);
			Assert.Contains(seven.Problems, p => p.Diagnostic.Code == "CS1644");
		}

		[Fact]
		public void ADiagnosticIsReportedAgainstTheOriginalLineEvenAfterEarlierBodiesWereWrapped()
		{
			var source =
				"class Many\n" +                                    // 1
				"{\n" +                                             // 2
				"\tint a;\n" +                                      // 3
				"\tvoid One()\n" +                                  // 4
				"\t{\n" +                                           // 5
				"\t\ta++;\n" +                                      // 6
				"\t}\n" +                                           // 7
				"\n" +                                              // 8
				"\tvoid Two()\n" +                                  // 9
				"\t{\n" +                                           // 10
				"\t\ta--;\n" +                                      // 11
				"\t}\n" +                                           // 12
				"\n" +                                              // 13
				"\tvoid Three()\n" +                                // 14
				"\t{\n" +                                           // 15
				"\t\tchar open = '{';\n" +                          // 16
				"\t}\n" +                                           // 17
				"\n" +                                              // 18
				"\tvoid Four()\n" +                                 // 19
				"\t{\n" +                                           // 20
				"\t}\n" +                                           // 21
				"}\n";                                              // 22

			var check = Check("Many.cs", source);

			Assert.False(check.Passed);
			// Three bodies' worth of injected lines sit above the failure; the report still points into the
			// method that holds the brace literal or the one the game then mangled, never at the injected offset.
			var problem = check.Problems[0];
			Assert.InRange(problem.Position.Line, 16, 19);
			Assert.True(problem.Diagnostic.Line > 22, "the injected file is longer than the original");
		}

		[Fact]
		public void ADiagnosticOnALineTheInjectorLeftAloneIsExact()
		{
			var source =
				"class Simple\n" +
				"{\n" +
				"\tvoid One()\n" +
				"\t{\n" +
				"\t\tint x = ;\n" +
				"\t}\n" +
				"}\n";

			var check = Check("Simple.cs", source);

			var problem = Enumerable.First(check.Problems, p => p.Diagnostic.Code == "CS1525");
			Assert.Equal(5, problem.Position.Line);
			Assert.Equal(problem.Diagnostic.Column, problem.Position.Column);
			Assert.True(problem.Position.Exact);
		}

		[Fact]
		public void TheAuditFlagsATryBlockInsideAnAccessor()
		{
			// Nothing in the game's output is known to parse with a misplaced try, so the detector is shown on a hand-written case.
			var injected =
				"class A\n" +
				"{\n" +
				"\tint P { get {\n" +
				"try {\n" +
				"\t\treturn 1;\n" +
				"\t}\n" +
				"catch(ModException) {throw;}\n" +
				"catch(System.Exception ex)\n" +
				"\t{\n" +
				"\tModController.HandleException(\"x\", ex);\n" +
				"\tthrow new ModException();\n" +
				"\t}\n" +
				"} }\n" +
				"\tvoid M() {\n" +
				"try {\n" +
				"\t}\n" +
				"catch(ModException) {throw;}\n" +
				"}\n" +
				"}\n";

			var audit = InjectedSourceAudit.Audit(new InjectedSource("A.cs", injected, injected, injected, 0));

			Assert.Equal(2, audit.TryBlocks);
			Assert.Single(audit.Misplaced);
			Assert.Contains("get", audit.Misplaced[0]);
		}

		[Fact]
		public void ABodyThatOnlyThrowsCompilesToABranchPastTheEndOfTheMethod()
		{
			var injected = GameSourceInjector.Inject("Bodies.cs", ThrowingBodies);
			var run = GameCompiler.CompileStandalone("bodies", injected.Injected, StubTypes);
			Assert.True(run.Succeeded, string.Join(Environment.NewLine, Enumerable.Select(run.Diagnostics, d => d.Code + ": " + d.Message)));

			var invalid = new List<string>();
			var type = GameCompiler.LoadCompiled(run).GetType("Bodies", true);
			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
			{
				if (IlReader.InvalidBranchTargets(method.GetMethodBody()).Count > 0)
				{
					invalid.Add(method.Name);
				}
			}

			// Only the bodies with no way out other than a throw: the injected leave has nowhere to land.
			invalid.Sort(StringComparer.Ordinal);
			Assert.Equal(new[] { "OnlyThrows", "ThrowsAfterWork" }, invalid);

			// And the source check names exactly those.
			var flagged = InjectedSourceAudit.FindBodiesEndingInThrow("Bodies.cs", ThrowingBodies);
			Assert.Equal(2, flagged.Count);
			Assert.Contains("OnlyThrows", flagged[0]);
			Assert.Contains("ThrowsAfterWork", flagged[1]);
		}

		[Fact]
		public void TheIlReaderFindsNothingWrongWithOrdinaryCode()
		{
			var problems = new List<string>();
			foreach (var type in typeof(IlReader).Assembly.GetTypes())
			{
				foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					var body = method.GetMethodBody();
					if (body != null && IlReader.InvalidBranchTargets(body).Count > 0)
					{
						problems.Add(type.FullName + "." + method.Name);
					}
				}
			}

			Assert.True(problems.Count == 0, "The IL walker disagrees with the C# compiler about: " + string.Join(", ", problems));
		}

		private static ParseCheck Check(string name, string source)
		{
			return GameCompiler.CheckSyntax(new[] { GameSourceInjector.Inject(name, source) });
		}

		private static string MethodSource(string statement)
		{
			return "class Fixture\n{\n\tpublic int F;\n\tpublic void First()\n\t{\n\t\t" + statement + "\n\t\tF++;\n\t}\n\tpublic void After()\n\t{\n\t\tF--;\n\t}\n}\n";
		}
	}
}
