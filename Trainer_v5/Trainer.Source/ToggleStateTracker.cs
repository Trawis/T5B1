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

		// Called when the game is left: the next observation is a cold start, but a toggle that was on still counts as on.
		// Without that, a restore for state the game keeps across loads (AI.MaxBoxes, EdCost, Cheats.*) would never run.
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
				// Carried over: on again replays the apply, off runs the restore.
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
