using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	// What the game's injected try/catch does to a FeatureGuard-style boundary (#241). The real
	// ModController.HandleException shows the error window and unloads the mod (verified in the game assembly,
	// not run); the stub here only records that it was called. These tests show which exceptions reach it.
	public class WorkshopExceptionBoundaryTests
	{
		private const string Stubs =
			"public class ModException : System.Exception { }\n" +
			"public static class ModController\n" +
			"{\n" +
			"\tpublic static int Handled;\n" +
			"\tpublic static bool ModUnloaded;\n" +
			"\tpublic static void HandleException(string mod, System.Exception failure) { Handled++; ModUnloaded = true; }\n" +
			"}\n";

		private const string Fixture =
			"using System;\n" +
			"public static class Feature\n" +
			"{\n" +
			"\tpublic static int Runs;\n" +
			"\tpublic static void Fails()\n" +
			"\t{\n" +
			"\t\tRuns++;\n" +
			"\t\tif (Runs > 0)\n" +
			"\t\t{\n" +
			"\t\t\tthrow new InvalidOperationException(\"boom\");\n" +
			"\t\t}\n" +
			"\t}\n" +
			"\tpublic static void CallsFails()\n" +
			"\t{\n" +
			"\t\tFails();\n" +
			"\t}\n" +
			"\tpublic static void Works()\n" +
			"\t{\n" +
			"\t\tRuns++;\n" +
			"\t}\n" +
			"}\n" +
			"public static class Guard\n" +
			"{\n" +
			"\tpublic static int Caught;\n" +
			"\tpublic static string LastCaught;\n" +
			"\tpublic static bool TryExecute(string id, Action action)\n" +
			"\t{\n" +
			"\t\ttry\n" +
			"\t\t{\n" +
			"\t\t\taction();\n" +
			"\t\t\treturn true;\n" +
			"\t\t}\n" +
			"\t\tcatch (Exception failure)\n" +
			"\t\t{\n" +
			"\t\t\tCaught++;\n" +
			"\t\t\tLastCaught = failure.GetType().Name;\n" +
			"\t\t\treturn false;\n" +
			"\t\t}\n" +
			"\t}\n" +
			"}\n" +
			"public static class Scenarios\n" +
			"{\n" +
			"\tpublic static bool MethodGroup()\n" +
			"\t{\n" +
			"\t\tvar first = Guard.TryExecute(\"a\", Feature.Fails);\n" +
			"\t\tGuard.TryExecute(\"b\", Feature.Works);\n" +
			"\t\treturn first;\n" +
			"\t}\n" +
			"\tpublic static bool InlineLambdaThatThrows()\n" +
			"\t{\n" +
			"\t\treturn Guard.TryExecute(\"a\", () => { if (Feature.Runs >= 0) { throw new InvalidOperationException(\"x\"); } });\n" +
			"\t}\n" +
			"\tpublic static bool LambdaThatCallsAMethod()\n" +
			"\t{\n" +
			"\t\treturn Guard.TryExecute(\"a\", () => Feature.Fails());\n" +
			"\t}\n" +
			"\tpublic static bool NestedGuards()\n" +
			"\t{\n" +
			"\t\treturn Guard.TryExecute(\"outer\", () => { Guard.TryExecute(\"inner\", Feature.Fails); });\n" +
			"\t}\n" +
			"\tpublic static void DirectCall()\n" +
			"\t{\n" +
			"\t\tFeature.CallsFails();\n" +
			"\t}\n" +
			"}\n";

		private static readonly Lazy<Assembly> Compiled = new Lazy<Assembly>(Compile);

		[Fact]
		public void AMethodGroupFailureIsCaughtByTheGuardButTheGameHandlerHasAlreadyRun()
		{
			Reset();

			var result = (bool)Call("MethodGroup");

			Assert.False(result);
			Assert.Equal("ModException", Read("Guard", "LastCaught"));
			Assert.Equal(1, Read("ModController", "Handled"));
			Assert.Equal(true, Read("ModController", "ModUnloaded"));
			Assert.Equal(2, Read("Feature", "Runs"));
		}

		[Fact]
		public void AnInlineLambdaThatThrowsIsNotInstrumentedSoOnlyTheGuardSeesIt()
		{
			Reset();

			var result = (bool)Call("InlineLambdaThatThrows");

			Assert.False(result);
			Assert.Equal("InvalidOperationException", Read("Guard", "LastCaught"));
			Assert.Equal(0, Read("ModController", "Handled"));
			Assert.Equal(false, Read("ModController", "ModUnloaded"));
		}

		[Fact]
		public void AnInlineLambdaThatCallsAMethodReachesTheGameHandlerThroughThatMethod()
		{
			Reset();

			var result = (bool)Call("LambdaThatCallsAMethod");

			Assert.False(result);
			Assert.Equal("ModException", Read("Guard", "LastCaught"));
			Assert.Equal(1, Read("ModController", "Handled"));
		}

		[Fact]
		public void AnInnerGuardSwallowsTheFailureForTheOuterOneButTheHandlerStillRanOnce()
		{
			Reset();

			var result = (bool)Call("NestedGuards");

			Assert.True(result);
			Assert.Equal(1, Read("Guard", "Caught"));
			Assert.Equal(1, Read("ModController", "Handled"));
		}

		[Fact]
		public void WithoutAGuardTheFailureEscapesEveryFrameAndTheHandlerRunsOnlyOnce()
		{
			Reset();

			var failure = Assert.Throws<TargetInvocationException>(() => Call("DirectCall"));

			Assert.Equal("ModException", failure.InnerException.GetType().Name);
			Assert.Equal(1, Read("ModController", "Handled"));
		}

		private static Assembly Compile()
		{
			var injected = GameSourceInjector.Inject("Fixture.cs", Fixture);
			var run = GameCompiler.CompileStandalone("boundary", injected.Injected, Stubs);
			Assert.True(run.Succeeded, string.Join(Environment.NewLine, Enumerable.Select(run.Diagnostics, d => d.Code + ": " + d.Message)));
			return GameCompiler.LoadCompiled(run);
		}

		private static object Call(string scenario)
		{
			return Compiled.Value.GetType("Scenarios", true).GetMethod(scenario).Invoke(null, null);
		}

		private static object Read(string type, string field)
		{
			return Compiled.Value.GetType(type, true).GetField(field).GetValue(null);
		}

		private static void Reset()
		{
			Compiled.Value.GetType("ModController", true).GetField("Handled").SetValue(null, 0);
			Compiled.Value.GetType("ModController", true).GetField("ModUnloaded").SetValue(null, false);
			Compiled.Value.GetType("Feature", true).GetField("Runs").SetValue(null, 0);
			Compiled.Value.GetType("Guard", true).GetField("Caught").SetValue(null, 0);
			Compiled.Value.GetType("Guard", true).GetField("LastCaught").SetValue(null, null);
		}
	}
}
