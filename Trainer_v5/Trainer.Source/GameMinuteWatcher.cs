using System;

namespace Trainer_v5
{
	// TimeOfDay has no minute-passed event; this raises one from its public Minute field.
	// Int truncation makes the hour rollover (Minute wraps past 60 back to ~0) a no-op case.
	public sealed class GameMinuteWatcher
	{
		public event EventHandler OnMinutePassed;

		private int _lastObservedMinute = -1;

		public void Poll()
		{
			int currentMinute = (int)TimeOfDay.Instance.Minute;
			if (currentMinute == _lastObservedMinute)
			{
				return;
			}

			_lastObservedMinute = currentMinute;
			OnMinutePassed?.Invoke(this, EventArgs.Empty);
		}

		// Forces the next Poll() to fire, for a catch-up run on fresh subscription.
		public void Reset()
		{
			_lastObservedMinute = -1;
		}
	}
}
