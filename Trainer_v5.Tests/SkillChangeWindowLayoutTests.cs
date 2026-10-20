using Xunit;

namespace Trainer_v5.Tests
{
	// The skill editor window has two columns; the second one must end inside the window width.
	public class SkillChangeWindowLayoutTests
	{
		[Fact]
		public void TheWindowIsWideEnoughForTheSecondColumn()
		{
			Assert.True(Constants.X_EMPLOYEESKILLCHANGE_WINDOW >= Constants.SECOND_COLUMN + Constants.ELEMENT_WIDTH);
		}
	}
}
