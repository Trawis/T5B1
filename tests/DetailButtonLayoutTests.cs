using System.Linq;
using UnityEngine;
using Xunit;

namespace Trainer_v5.Tests
{
	public class DetailButtonLayoutTests
	{
		[Fact]
		public void UnknownWidthKeepsTheOriginalSingleRow()
		{
			var slots = DetailButtonLayout.Place(5, 0f, 80, 32, 4);

			Assert.Equal(new Rect(0, -36, 80, 32), slots[0]);
			Assert.Equal(new Rect(336, -36, 80, 32), slots[4]);
		}

		[Fact]
		public void AWideEnoughWindowKeepsOneRow()
		{
			var slots = DetailButtonLayout.Place(5, 420f, 80, 32, 4);

			Assert.All(slots, slot => Assert.Equal(-36f, slot.y));
		}

		[Fact]
		public void ANarrowWindowWrapsUpwardAndNothingPassesItsRightEdge()
		{
			const float available = 300f;
			var slots = DetailButtonLayout.Place(5, available, 80, 32, 4);

			Assert.All(slots, slot => Assert.True(slot.x + slot.width <= available, "button at x=" + slot.x));
			Assert.Equal(new[] { -36f, -36f, -36f, -72f, -72f }, Enumerable.ToArray(Enumerable.Select(slots, slot => slot.y)));
			Assert.Equal(0f, slots[3].x);
		}

		[Fact]
		public void AWindowNarrowerThanOneButtonStillGetsOneColumn()
		{
			var slots = DetailButtonLayout.Place(3, 40f, 80, 32, 4);

			Assert.All(slots, slot => Assert.Equal(0f, slot.x));
			Assert.Equal(new[] { -36f, -72f, -108f }, Enumerable.ToArray(Enumerable.Select(slots, slot => slot.y)));
		}
	}
}
