using System;

namespace Trainer_v5
{
	// TimeOfDay has no minute event; this raises one from its Minute field (int truncation makes the hour wrap a no-op).
	public sealed class GameMinuteWatcher
	{
		private int _lastObservedMinute = -1;

		public event EventHandler OnMinutePassed;

		public void Poll()
		{
			// TimeOfDay is absent during scene transitions; that is "not ready", not an error.
			if (TimeOfDay.Instance == null)
			{
				return;
			}

			Observe((int)TimeOfDay.Instance.Minute);
		}

		// Forces the next Poll() to fire, for a catch-up run on fresh subscription.
		public void Reset()
		{
			_lastObservedMinute = -1;
		}

		internal void Observe(int currentMinute)
		{
			if (currentMinute == _lastObservedMinute)
			{
				return;
			}

			_lastObservedMinute = currentMinute;
			OnMinutePassed?.Invoke(this, EventArgs.Empty);
		}
	}
}
