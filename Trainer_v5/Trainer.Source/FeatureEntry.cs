using System;

namespace Trainer_v5
{
	internal enum FeatureTrigger
	{
		// Every frame while the feature's setting is on.
		Frame = 0,
		// Game-time cadences while the setting is on.
		Minute = 1,
		Hour = 2,
		Day = 3,
		Month = 4,
		// Once, on the frame a toggle goes from off to on or from on to off.
		Enabled = 5,
		Disabled = 6
	}

	// One scheduled call. A feature with several cadences has several entries sharing its Id.
	internal sealed class FeatureEntry
	{
		public readonly string Id;
		public readonly FeatureTrigger Trigger;
		public readonly ToggleKey Toggle;
		public readonly StoreKey Store;
		public readonly Action Action;

		public FeatureEntry(string id, FeatureTrigger trigger, ToggleKey toggle, StoreKey store, Action action)
		{
			Id = id;
			Trigger = trigger;
			Toggle = toggle;
			Store = store;
			Action = action;
		}
	}
}
