using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Isolates one trainer feature so its exceptions can't stop unrelated features running after it. See #123.
	// The clock and the report sink are supplied by Helpers so this stays free of Unity calls.
	public sealed class FeatureGuard
	{
		private const float FailureLogIntervalSeconds = 60f;

		private readonly Func<float> _clock;
		private readonly Action<string, Exception> _report;
		// Last failure time per feature+exception type, so a repeat of the same failure reports once per interval while a different failure still reports immediately.
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
