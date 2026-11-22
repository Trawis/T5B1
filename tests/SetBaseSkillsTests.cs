using System.Globalization;
using System.Threading;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SetBaseSkillsTests
	{
		[Theory]
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

				Assert.Equal(valid, error == InputError.None);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = original;
			}
		}
	}
}
