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
		[InlineData(-3, true)]
		[InlineData(0, false)]
		[InlineData(4, false)]
		public void StarsAreAChangeOfOneToThreeInEitherDirection(int stars, bool valid)
		{
			Assert.Equal(valid, SkillEdit.IsValidStars(stars));
		}

		[Theory]
		[InlineData("-3", true, -3)]
		[InlineData("3.0", false, 0)]
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
		[InlineData(0, 4, "None")]
		[InlineData(3, 4, "Partial")]
		public void TheReportSeparatesAllSomeAndNoneApplied(int applied, int total, string expected)
		{
			Assert.Equal(expected, SkillEdit.Outcome(applied, total).ToString());
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
	}
}
