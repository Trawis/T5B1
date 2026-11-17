using System;

namespace Trainer_v5
{
	// The trainer may act only between the game's OnActivate and OnDeactivate. A callback bound before the last
	// deactivation or game change belongs to UI that no longer exists, so it does nothing.
	internal static class TrainerLifecycle
	{
		private static int _generation;

		public static bool Active { get; private set; }

		public static int Generation
		{
			get { return _generation; }
		}

		public static void Activate()
		{
			Active = true;
			_generation++;
		}

		public static void Deactivate()
		{
			Active = false;
			_generation++;
		}

		public static void NewGame()
		{
			_generation++;
		}

		public static bool IsCurrent(int generation)
		{
			return Active && generation == _generation;
		}

		public static Action Bind(Action action)
		{
			var generation = _generation;
			return () =>
			{
				if (IsCurrent(generation))
				{
					action();
				}
			};
		}

		public static Action<T> Bind<T>(Action<T> action)
		{
			var generation = _generation;
			return value =>
			{
				if (IsCurrent(generation))
				{
					action(value);
				}
			};
		}
	}
}
