using Xunit;

namespace Trainer_v5.Tests
{
	// The editors share the scrolling column layout, so no window can be taller than the canvas allows.
	public class WindowSizingTests
	{
		[Theory]
		[InlineData("EmployeeTraitChangeWindow.cs", 3)]
		[InlineData("EmployeeDemandChangeWindow.cs", 1)]
		public void TheEditorsLayOutTheirColumnsThroughTheScrollableColumnHelper(string file, int columns)
		{
			var source = SourceCode.Read("Window", file);

			Assert.Equal(columns, Count(source, "UIHelper.CreateScrollableColumn("));
			Assert.Contains("UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT,", source);
			Assert.Contains("window.SetMinSize(", source);
			Assert.DoesNotContain("SetWindowSize(", source);
			Assert.DoesNotContain("AddToWindow(", source);
		}

		[Fact]
		public void TheLeadSpecRowLimitUsesTheRowStepTheColumnsLayOutWith()
		{
			var source = SourceCode.Read("Window", "EmployeeLeadSpecChangeWindow.cs");

			Assert.Contains("UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, reservedRows, gap)", source);
			Assert.Contains("Constants.ELEMENT_HEIGHT, gap, maxVisibleRows", source);
		}

		private static int Count(string source, string marker)
		{
			var count = 0;
			var index = source.IndexOf(marker, System.StringComparison.Ordinal);
			while (index >= 0)
			{
				count++;
				index = source.IndexOf(marker, index + marker.Length, System.StringComparison.Ordinal);
			}

			return count;
		}
	}
}
