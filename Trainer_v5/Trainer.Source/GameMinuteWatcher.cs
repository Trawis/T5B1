using System;

namespace Trainer_v5
{
	// TimeOfDay exposes no minute-passed event -- only OnHourPassed/OnDayPassed/OnMonthPassed.
	// It does expose a public Minute field that the game itself increments every frame
	// (Minute += Time.deltaTime * GameSpeed), rolling over past 60 to fire AddHour. This class
	// polls that field once per Poll() call (intended to be driven from Update()) and raises
	// OnMinutePassed when the observed in-game minute changes, so callers can react to a minute
	// boundary without knowing that TimeOfDay itself has no native event for it.
	//
	// The int cast only checks for inequality, so it naturally handles the hour-boundary
	// rollover (Minute resets from just under 60 back down near 0) without any special-casing.
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

		// Forces the next Poll() to raise OnMinutePassed regardless of the observed minute, so a
		// fresh subscription (scene reload, MainMenu round-trip, trainer reactivate) always gets
		// an immediate catch-up run instead of waiting for the minute to actually change.
		public void Reset()
		{
			_lastObservedMinute = -1;
		}
	}
}
