using System.Collections.Generic;

namespace Trainer_v5
{
	public static class Extensions
	{
		public static void Toggle(this Dictionary<string, bool> self, string key)
		{
			bool value;
			if (self.TryGetValue(key, out value))
			{
				self[key] = !value;
			}
		}

		public static void SetWindowSize(this GUIWindow window, int rows, int xWindowSize)
		{
			window.MinSize.x = xWindowSize;
			window.MinSize.y = (rows + 1) * Constants.ELEMENT_HEIGHT;
		}
	}
}
