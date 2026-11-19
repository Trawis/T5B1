using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SkillEditTests
	{
		private static string EditorSource()
		{
			return SourceCode.Read("Window", "EmployeeSkillChangeWindow.cs");
		}

		[Theory]
		[InlineData(-4, false)]
		[InlineData(-3, true)]
		[InlineData(-1, true)]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(3, true)]
		[InlineData(4, false)]
		public void StarsAreAChangeOfOneToThreeInEitherDirection(int stars, bool valid)
		{
			Assert.Equal(valid, SkillEdit.IsValidStars(stars));
		}

		[Theory]
		[InlineData("3", true, 3)]
		[InlineData("-3", true, -3)]
		[InlineData(" 2", true, 2)]
		[InlineData("0", false, 0)]
		[InlineData("4", false, 0)]
		[InlineData("3.0", false, 0)]
		[InlineData("x", false, 0)]
		[InlineData("", false, 0)]
		[InlineData(null, false, 0)]
		public void StarsInputIsParsedWithoutGuessing(string input, bool valid, int expected)
		{
			int stars;

			Assert.Equal(valid, SkillEdit.TryParseStars(input, out stars));
			if (valid)
			{
				Assert.Equal(expected, stars);
			}
		}

		[Theory]
		[InlineData(0, 0, "None")]
		[InlineData(0, 4, "None")]
		[InlineData(1, 1, "All")]
		[InlineData(4, 4, "All")]
		[InlineData(3, 4, "Partial")]
		public void TheReportSeparatesAllSomeAndNoneApplied(int applied, int total, string expected)
		{
			Assert.Equal(expected, SkillEdit.Outcome(applied, total).ToString());
		}

		[Fact]
		public void TheEditorOnlyActsOnEmployeesCapturedThroughEmployeeTarget()
		{
			var source = EditorSource();

			Assert.DoesNotContain("SelectorController", source);
			Assert.Contains("EmployeeTarget.ResolveSelection()", SourceCode.Body(source, "private static List<EmployeeTarget> CaptureTargets()"));
		}

		[Fact]
		public void EveryWriteChecksTheEmployeeAgainAndHasOneCallSite()
		{
			var source = EditorSource();

			SourceCode.AssertInOrder(SourceCode.Body(source, "private static void SetSkills()"), "target.IsStillValid()", "AddSpecialization(", "the target is checked again before the write");
			SourceCode.AssertInOrder(SourceCode.Body(source, "private static void SetBaseSkills()"), "target.IsStillValid()", "SkillCeiling = 1f", "the target is checked again before the write");
			SourceCode.AssertInOrder(SourceCode.Body(source, "private static void SetBaseSkills()"), "SkillCeiling = 1f", "ChangeSkillDirect(", "the ceiling is raised first");
			Assert.Single(Regex.Matches(source, @"\.AddSpecialization\("));
			Assert.Single(Regex.Matches(source, @"SkillCeiling = 1f"));
			Assert.Single(Regex.Matches(source, @"\.ChangeSkillDirect\("));
		}

		[Fact]
		public void TargetsRolesAndSpecializationsAreCapturedAndCheckedBeforeTheDialog()
		{
			var source = EditorSource();
			var skills = SourceCode.Body(source, "private static void SetSkills()");
			var baseSkills = SourceCode.Body(source, "private static void SetBaseSkills()");

			SourceCode.AssertInOrder(skills, "CaptureTargets()", "CaptureRoles()", "employees are checked first, as before");
			SourceCode.AssertInOrder(skills, "CaptureRoles()", "CaptureSpecializations()", "roles come before specializations, as before");
			SourceCode.AssertInOrder(skills, "CaptureSpecializations()", "SpawnInputDialog(", "nothing is asked for before the choices are valid");
			SourceCode.AssertInOrder(baseSkills, "CaptureRoles()", "RequestFloat(", "nothing is asked for before the choices are valid");
			Assert.DoesNotContain("CaptureSpecializations", baseSkills);
			SourceCode.AssertInOrder(skills, "TryParseStars(", "AddSpecialization(", "the stars are validated before anything is written");
			Assert.Contains("\"3\",", skills);
		}
	}
}
