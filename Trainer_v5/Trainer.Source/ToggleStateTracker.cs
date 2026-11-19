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
		private readonly HashSet<string> _enabledBeforeReset = new HashSet<string>();

		public void Reset()
		{
			foreach (KeyValuePair<string, bool> observed in _lastObserved)
			{
				if (observed.Value)
				{
					_enabledBeforeReset.Add(observed.Key);
				}
			}
			_lastObserved.Clear();
		}

		public ToggleTransitionEnum Observe(string settingKey, bool isEnabled)
		{
			bool wasEnabled;
			if (!_lastObserved.TryGetValue(settingKey, out wasEnabled))
			{
				wasEnabled = _enabledBeforeReset.Remove(settingKey) && !isEnabled;
			}
			_lastObserved[settingKey] = isEnabled;

			if (isEnabled == wasEnabled)
			{
				return ToggleTransitionEnum.Unchanged;
			}
			return isEnabled ? ToggleTransitionEnum.JustEnabled : ToggleTransitionEnum.JustDisabled;
		}
	}
}
