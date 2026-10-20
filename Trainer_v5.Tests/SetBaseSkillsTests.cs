using System.Globalization;
using System.Threading;
using Xunit;

namespace Trainer_v5.Tests
{
	// Set Base Skills enforces the 0-1 range it shows and states its skill-ceiling side effect (#359).
	public class SetBaseSkillsTests
	{
		private static string SetBaseSkillsBody()
		{
			return SourceCode.Body(SourceCode.Read("Window", "EmployeeSkillChangeWindow.cs"), "private static void SetBaseSkills()");
		}

		[Fact]
		public void RequestUsesTheRangeItShows()
		{
			var body = SetBaseSkillsBody();

			Assert.Contains("Min = 0, Max = 1.0", body);
			Assert.Contains("0f, 1f);", body);
		}

		[Fact]
		public void PromptStatesTheSkillCeilingEffect()
		{
			var body = SetBaseSkillsBody();

			Assert.Contains("SkillCeiling = 1f", body);
			Assert.Contains("raises each selected employee's skill ceiling to 1.0", body);
		}

		[Theory]
		[InlineData("-0.1", false)]
		[InlineData("0", true)]
		[InlineData("0.5", true)]
		[InlineData("1", true)]
		[InlineData("1.01", false)]
		public void ZeroToOneRangeRejectsOutOfRangeValues(string input, bool valid)
		{
			var original = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			try
			{
				float value;
				var error = InputHelper.Validate<float>(input, InputHelper.ParseFloat, 0f, 1f, out value);

				Assert.Equal(valid, error == null);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = original;
			}
		}
	}
}
