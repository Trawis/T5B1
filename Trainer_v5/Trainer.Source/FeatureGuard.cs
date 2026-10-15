using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Isolates one trainer feature so its exceptions don't stop others (#123); the clock and report sink are injected.
	public sealed class FeatureGuard
	{
		private const float FailureLogIntervalSeconds = 60f;

		private readonly Func<float> _clock;
		private readonly Action<string, Exception> _report;
		// Last failure time per feature and exception type: a repeat reports once per interval, a new failure at once.
		private readonly Dictionary<string, float> _failureLogTimes = new Dictionary<string, float>();

		public FeatureGuard(Func<float> clock, Action<string, Exception> report)
		{
			_clock = clock;
			_report = report;
		}

		// Returns whether the action completed without throwing.
		public bool TryExecute(string featureId, Action action)
		{
			try
			{
				action.Invoke();
				return true;
			}
			catch (Exception ex)
			{
				string logKey = featureId + "|" + ex.GetType().FullName;
				float now = _clock();
				float lastLogged;
				if (!_failureLogTimes.TryGetValue(logKey, out lastLogged) || now - lastLogged >= FailureLogIntervalSeconds)
				{
					_failureLogTimes[logKey] = now;
					_report(featureId, ex);
				}
				return false;
			}
		}
	}
}
