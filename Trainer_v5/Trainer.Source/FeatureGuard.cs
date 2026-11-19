using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	public sealed class FeatureGuard
	{
		private const float FailureLogIntervalSeconds = 60f;

		private readonly Func<float> _clock;
		private readonly Action<string, Exception> _report;
		private readonly Dictionary<string, float> _failureLogTimes = new Dictionary<string, float>();

		public FeatureGuard(Func<float> clock, Action<string, Exception> report)
		{
			_clock = clock;
			_report = report;
		}

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
