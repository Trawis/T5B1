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
	}
}
