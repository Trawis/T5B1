using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Runs a trigger's features in registration order; the settings lookup and guard are injected to stay Unity-free.
	internal sealed class FeatureDispatcher
	{
		// RunTransitions calls (frames) that retry every call before a failing transition backs off to every 60th call.
		private const int FastRetryVisits = 60;

		private readonly List<FeatureEntry>[] _byTrigger;
		private readonly List<ToggleKey> _transitionKeys;
		private readonly Func<ToggleKey, bool> _isToggleOn;
		private readonly Func<StoreKey, bool> _isStoreSet;
		private readonly ToggleStateTracker _toggleStates;
		private readonly Func<string, Action, bool> _execute;
		// Transition entries that failed, per toggle key. Kept across ToggleStateTracker.Reset on purpose: a restore that
		// failed for a toggle that is off in the next game would otherwise never run again (see RunTransitions).
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
		// An entry whose action reports failure is retried on the following calls instead of being consumed (#336):
		// only the failed entries repeat, a successful one never does, and the next transition of the same toggle
		// (a reversal included) replaces whatever is still pending, because its entries set the state outright.
		// Every Enabled/Disabled action must stay an idempotent "set the state" write or a state-driven catch-up
		// (a baseline captured once, an absolute value, a scan of what is still unfinished) so that a repeat after a
		// partial run is safe. A one-off effect with a lasting side effect belongs on no transition trigger.
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
		}

		private void RunPending(ToggleKey key)
		{
			PendingTransition pending;
			if (!_pending.TryGetValue(key.Key, out pending))
			{
				return;
			}

			// A permanently failing action backs off after the first minute of frames instead of failing every frame.
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
		}

		private bool IsOn(FeatureEntry entry)
		{
			return entry.Toggle != null ? _isToggleOn(entry.Toggle) : _isStoreSet(entry.Store);
		}

		// The entries of one toggle's latest transition that have not succeeded yet.
		private sealed class PendingTransition
		{
			public List<FeatureEntry> Entries = new List<FeatureEntry>();
			public int Visits;
		}
	}
}
