using System;

namespace Trainer_v5
{
	public sealed class GameMinuteWatcher
	{
		private int _lastObservedMinute = -1;

		public event EventHandler OnMinutePassed;

		public void Poll()
		{
			if (TimeOfDay.Instance == null)
			{
				return;
			}

			Observe((int)TimeOfDay.Instance.Minute);
		}

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
