using System;

namespace Trainer_v5
{
	internal enum FeatureTrigger
	{
		Frame = 0,
		Minute = 1,
		Hour = 2,
		Day = 3,
		Month = 4,
		Enabled = 5,
		Disabled = 6
	}

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
