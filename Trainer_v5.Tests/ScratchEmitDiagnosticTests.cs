using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Temporary: finds which construct makes the game's compiler fail on .NET Framework (SetConstant). Not to be merged.
	public class ScratchEmitDiagnosticTests
	{
		private static string Try(string name, params string[] sources)
		{
			try
			{
				var run = GameCompiler.CompileStandalone(name, sources);
				var errors = new List<string>();
				foreach (var d in run.Diagnostics)
				{
					if (d.IsError)
					{
						errors.Add(d.Code);
					}
				}

				return name + ": " + (run.Succeeded ? "OK" : "FAILED " + string.Join(",", errors));
			}
			catch (Exception ex)
			{
				return name + ": THREW " + ex.Message.Split('\n')[0];
			}
		}

		private static string File1(string name)
		{
			return File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, name));
		}

		[Fact]
		public void Diagnose()
		{
			var lines = new List<string>
			{
				Try("enumExplicit", "namespace N { internal enum E { None = 0, A = 1 } }"),
				Try("enumImplicit", "namespace N { internal enum E { None, A } }"),
				Try("enumTwo", "namespace N { internal enum E { None = 0, A = 1 } internal enum F { None = 0, B = 1 } }"),
				Try("constInt", "namespace N { internal class C { public const int X = 5; } }"),
				Try("constNeg", "namespace N { internal class C { public const int X = -3; } }"),
				Try("constFloat", "namespace N { internal class C { public const float X = 1.5f; } }"),
				Try("constFloatInt", "namespace N { internal class C { public const float X = 4f; } }"),
				Try("constString", "namespace N { internal class C { public const string X = \"a\"; } }"),
				Try("constChain", "namespace N { internal class C { public const int A = 1; public const int B = A + 2; } internal class D { public const int Z = C.B - C.A; } }"),
				Try("constEnum", "namespace N { internal enum E { None = 0, A = 1 } internal class C { private const E X = E.A; } }"),
				Try("DomainEnums.cs", File1("DomainEnums.cs")),
				Try("SkillEdit.cs", File1("SkillEdit.cs")),
				Try("SceneRoute.cs", File1("SceneRoute.cs")),
				Try("ToggleStateTracker.cs", File1("ToggleStateTracker.cs")),
				Try("Constants.cs", File1("Constants.cs")),
				Try("EmployeeTarget+...", File1("EmployeeTarget.cs"))
			};
			Assert.True(false, Environment.NewLine + string.Join(Environment.NewLine, lines));
		}
	}
}
