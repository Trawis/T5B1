using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	public class GameVersionCheckTests
	{
		[Fact]
		public void TargetMatchesTheVendoredAssemblyConstants()
		{
			Assert.Equal(GameVersionCheck.TargetTypeInt, ConstantOf("Type"));
			Assert.Equal(GameVersionCheck.TargetMajor, ConstantOf("Major"));
			Assert.Equal(GameVersionCheck.TargetMinor, ConstantOf("Minor"));
			Assert.Equal(GameVersionCheck.TargetRevision, ConstantOf("Revision"));
			Assert.Equal(GameVersionCheck.TargetText,
				"Beta " + GameVersionCheck.TargetMajor + "." + GameVersionCheck.TargetMinor + "." + GameVersionCheck.TargetRevision);
		}

		[Fact]
		public void OnlyTheExactTargetMatches()
		{
			Assert.True(GameVersionCheck.IsTarget(2, 1, 8, 42));
			Assert.False(GameVersionCheck.IsTarget(2, 1, 8, 43));
			Assert.False(GameVersionCheck.IsTarget(2, 1, 9, 42));
			Assert.False(GameVersionCheck.IsTarget(2, 2, 8, 42));
			Assert.False(GameVersionCheck.IsTarget(3, 1, 8, 42));
		}

		[Fact]
		public void MismatchMessageNamesBothVersionsAndStaysNonBlocking()
		{
			var text = GameVersionCheck.Describe("Beta 1.9.0", false);

			Assert.Contains("Beta 1.9.0", text);
			Assert.Contains(GameVersionCheck.TargetText, text);
			Assert.Contains("keeps running", text);
			Assert.Contains("matches", GameVersionCheck.Describe(GameVersionCheck.TargetText, true));
		}

		[Fact]
		public void DiagnosticReadsTheRuntimeVersionAndOnlyLogs()
		{
			var source = SourceCode.Read("GameVersionCheck.cs");

			Assert.Contains("Versioning.CurrentVersion", SourceCode.Body(source, "public static string Running"));
			Assert.DoesNotContain("throw", source);
			Assert.Contains("GameVersionCheck.Running()", SourceCode.Span(SourceCode.Read("Helpers.cs"), "public static string GetGameVersion"));
			Assert.Contains("GameVersionCheck.LogRunningVersion()", SourceCode.Body(SourceCode.Read("Main.cs"), "public override void Initialize"));
		}

		private static int ConstantOf(string name)
		{
			var field = typeof(Versioning).GetField(name, BindingFlags.Public | BindingFlags.Static);
			return (int)field.GetRawConstantValue();
		}
	}
}
