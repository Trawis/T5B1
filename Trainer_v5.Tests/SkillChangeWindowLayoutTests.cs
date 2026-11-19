using Xunit;

namespace Trainer_v5.Tests
{
	public class SkillChangeWindowLayoutTests
	{
		[Fact]
		public void TheWindowIsWideEnoughForTheSecondColumn()
		{
			Assert.True(Constants.X_EMPLOYEESKILLCHANGE_WINDOW >= Constants.SECOND_COLUMN + Constants.ELEMENT_WIDTH);
		}
	}
}
