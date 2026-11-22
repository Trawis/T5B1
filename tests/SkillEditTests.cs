using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SkillEditTests
	{
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
			var type = typeof(Trainer_v5.Window.EmployeeSkillChangeWindow);

			CompiledCalls.InOrder(type, "SetSkills", "call EmployeeTarget.IsStillValid", "call Employee.AddSpecialization");
			CompiledCalls.InOrder(type, "SetBaseSkills", "call EmployeeTarget.IsStillValid", "stfld Employee.SkillCeiling", "call Employee.ChangeSkillDirect");
			CompiledCalls.OnlyIn("stfld Employee.SkillCeiling", "EmployeeSkillChangeWindow.SetBaseSkills");
			CompiledCalls.OnlyIn("call Employee.AddSpecialization", "EmployeeSkillChangeWindow.SetSkills", "EmployeeFeatures.EmployeesToMax");
		}
	}
}
