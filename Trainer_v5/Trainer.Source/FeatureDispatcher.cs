using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Runs the registered features for a trigger, in registration order. The game-facing pieces (settings
	// lookup, the guard) are passed in so this stays free of Unity calls.
	internal sealed class FeatureDispatcher
	{
		private readonly List<FeatureEntry>[] _byTrigger;
		private readonly List<ToggleKey> _transitionKeys;
		private readonly Func<ToggleKey, bool> _isToggleOn;
		private readonly Func<StoreKey, bool> _isStoreSet;
		private readonly ToggleStateTracker _toggleStates;
		private readonly Func<string, Action, bool> _execute;

		public FeatureDispatcher(IList<FeatureEntry> entries, Func<ToggleKey, bool> isToggleOn, Func<StoreKey, bool> isStoreSet, ToggleStateTracker toggleStates, Func<string, Action, bool> execute)
		{
			_isToggleOn = isToggleOn;
			_isStoreSet = isStoreSet;
			_toggleStates = toggleStates;
			_execute = execute;
			_byTrigger = new List<FeatureEntry>[Enum.GetValues(typeof(FeatureTrigger)).Length];
			for (int i = 0; i < _byTrigger.Length; i++)
			{
				_byTrigger[i] = new List<FeatureEntry>();
			}

			_transitionKeys = new List<ToggleKey>();
			foreach (FeatureEntry entry in entries)
			{
				_byTrigger[(int)entry.Trigger].Add(entry);
				bool isTransition = entry.Trigger == FeatureTrigger.Enabled || entry.Trigger == FeatureTrigger.Disabled;
				if (isTransition && !_transitionKeys.Contains(entry.Toggle))
				{
					_transitionKeys.Add(entry.Toggle);
				}
			}
		}

		// Runs every entry of a cadence whose setting is currently on.
		public void Run(FeatureTrigger cadence)
		{
			List<FeatureEntry> entries = _byTrigger[(int)cadence];
			for (int i = 0; i < entries.Count; i++)
			{
				FeatureEntry entry = entries[i];
				if (IsOn(entry))
				{
					_execute(entry.Id, entry.Action);
				}
			}
		}

		// Observes each transition toggle once, in registration order, and runs its Enabled or Disabled entries.
		public void RunTransitions()
		{
			for (int i = 0; i < _transitionKeys.Count; i++)
			{
				ToggleKey key = _transitionKeys[i];
				ToggleTransitionEnum transition = _toggleStates.Observe(key.Key, _isToggleOn(key));
				if (transition == ToggleTransitionEnum.JustEnabled)
				{
					RunFor(FeatureTrigger.Enabled, key);
				}
				else if (transition == ToggleTransitionEnum.JustDisabled)
				{
					RunFor(FeatureTrigger.Disabled, key);
				}
			}
		}

		private void RunFor(FeatureTrigger trigger, ToggleKey key)
		{
			List<FeatureEntry> entries = _byTrigger[(int)trigger];
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].Toggle == key)
				{
					_execute(entries[i].Id, entries[i].Action);
				}
			}
		}

		private bool IsOn(FeatureEntry entry)
		{
			return entry.Toggle != null ? _isToggleOn(entry.Toggle) : _isStoreSet(entry.Store);
		}
	}
}
