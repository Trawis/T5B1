using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	internal sealed class FeatureDispatcher
	{
		private const int FastRetryVisits = 60;

		private readonly List<FeatureEntry>[] _byTrigger;
		private readonly List<ToggleKey> _transitionKeys;
		private readonly Func<ToggleKey, bool> _isToggleOn;
		private readonly Func<StoreKey, bool> _isStoreSet;
		private readonly ToggleStateTracker _toggleStates;
		private readonly Func<string, Action, bool> _execute;
		private readonly Dictionary<string, PendingTransition> _pending = new Dictionary<string, PendingTransition>();

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

		public void RunTransitions()
		{
			for (int i = 0; i < _transitionKeys.Count; i++)
			{
				ToggleKey key = _transitionKeys[i];
				ToggleTransitionEnum transition = _toggleStates.Observe(key.Key, _isToggleOn(key));
				if (transition == ToggleTransitionEnum.JustEnabled)
				{
					StartPending(FeatureTrigger.Enabled, key);
				}
				else if (transition == ToggleTransitionEnum.JustDisabled)
				{
					StartPending(FeatureTrigger.Disabled, key);
				}

				RunPending(key);
			}
		}

		private void StartPending(FeatureTrigger trigger, ToggleKey key)
		{
			PendingTransition pending = new PendingTransition();
			List<FeatureEntry> entries = _byTrigger[(int)trigger];
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].Toggle == key)
				{
					pending.Entries.Add(entries[i]);
				}
			}

			if (pending.Entries.Count == 0)
			{
				_pending.Remove(key.Key);
			}
			else
			{
				_pending[key.Key] = pending;
			}

			if (LoggingOptions.DebugEnabled)
			{
				("toggle '" + key.Key + "' " + trigger + ": " + pending.Entries.Count + " entries queued").LogDebug();
			}
		}

		private void RunPending(ToggleKey key)
		{
			PendingTransition pending;
			if (!_pending.TryGetValue(key.Key, out pending))
			{
				return;
			}

			pending.Visits++;
			if (pending.Visits > FastRetryVisits && pending.Visits % FastRetryVisits != 0)
			{
				return;
			}

			List<FeatureEntry> failed = new List<FeatureEntry>();
			for (int i = 0; i < pending.Entries.Count; i++)
			{
				if (!_execute(pending.Entries[i].Id, pending.Entries[i].Action))
				{
					failed.Add(pending.Entries[i]);
				}
			}

			pending.Entries = failed;
			if (failed.Count == 0)
			{
				_pending.Remove(key.Key);
			}

			if (LoggingOptions.DebugEnabled)
			{
				if (failed.Count > 0 && pending.Visits == 1)
				{
					("toggle '" + key.Key + "': " + failed.Count + " entries failed, retrying").LogDebug();
				}
				else if (failed.Count == 0 && pending.Visits > 1)
				{
					("toggle '" + key.Key + "': retries finished after " + pending.Visits + " visits").LogDebug();
				}
			}
		}

		private bool IsOn(FeatureEntry entry)
		{
			return entry.Toggle != null ? _isToggleOn(entry.Toggle) : _isStoreSet(entry.Store);
		}

		private sealed class PendingTransition
		{
			public List<FeatureEntry> Entries = new List<FeatureEntry>();
			public int Visits;
		}
	}
}
