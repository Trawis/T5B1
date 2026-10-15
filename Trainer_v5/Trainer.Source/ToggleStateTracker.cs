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

		// Forgets what was seen so the next observation is a cold start; called when the game is left.
		public void Reset()
		{
			_lastObserved.Clear();
		}

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
