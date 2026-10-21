using System;
using UnityEngine;

namespace Trainer_v5
{
	// Lays the trainer buttons above the Employees window in one row, wrapping upward when the window is narrower (#182).
	internal static class DetailButtonLayout
	{
		public static Rect[] Place(int count, float availableWidth, int width, int height, int spacing)
		{
			var perRow = count;
			if (availableWidth > 0f)
			{
				perRow = Math.Max(1, Math.Min(count, (int)((availableWidth + spacing) / (width + spacing))));
			}

			var rects = new Rect[count];
			for (var i = 0; i < count; i++)
			{
				var column = i % perRow;
				var row = i / perRow;
				rects[i] = new Rect(column * (width + spacing), 0 - spacing - height - row * (height + spacing), width, height);
			}
			return rects;
		}
	}
}
