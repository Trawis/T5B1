using System.Collections.Generic;

namespace Trainer_v5
{
	internal enum ToggleTransitionEnum
	{
		Unknown = 0,
		Unchanged = 1,
		JustEnabled = 2,
		JustDisabled = 3
	}

	internal sealed class ToggleStateTracker
	{
		private readonly Dictionary<string, bool> _lastObserved = new Dictionary<string, bool>();

		public ToggleTransitionEnum Observe(string settingKey, bool isEnabled)
		{
			bool wasEnabled;
			_lastObserved.TryGetValue(settingKey, out wasEnabled);
			_lastObserved[settingKey] = isEnabled;

			if (isEnabled == wasEnabled)
			{
				return ToggleTransitionEnum.Unchanged;
			}
			return isEnabled ? ToggleTransitionEnum.JustEnabled : ToggleTransitionEnum.JustDisabled;
		}
	}
}
