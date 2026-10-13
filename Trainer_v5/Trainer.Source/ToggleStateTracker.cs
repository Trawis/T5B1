using System.Collections.Generic;

namespace Trainer_v5
{
	internal enum ToggleTransition
	{
		Unchanged,
		JustEnabled,
		JustDisabled
	}

	internal sealed class ToggleStateTracker
	{
		private readonly Dictionary<string, bool> _lastObserved = new Dictionary<string, bool>();

		public ToggleTransition Observe(string settingKey, bool isEnabled)
		{
			bool wasEnabled;
			_lastObserved.TryGetValue(settingKey, out wasEnabled);
			_lastObserved[settingKey] = isEnabled;

			if (isEnabled == wasEnabled)
			{
				return ToggleTransition.Unchanged;
			}
			return isEnabled ? ToggleTransition.JustEnabled : ToggleTransition.JustDisabled;
		}
	}
}
