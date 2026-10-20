using System.Collections.Generic;

namespace Trainer_v5
{
	// Original ProductPrinter.PrintSpeed per printer, keyed by the printer's Unity instance id so that no
	// game object is held across games. An id that is no longer in the live printer list is dropped (#339).
	internal sealed class PrinterSpeedBaselines
	{
		private readonly Dictionary<int, float> _speeds = new Dictionary<int, float>();

		public int Count => _speeds.Count;

		// The first captured value wins, so writing the boosted speed back in cannot become the baseline.
		public float Capture(int printerId, float currentSpeed)
		{
			float baseline;
			if (!_speeds.TryGetValue(printerId, out baseline))
			{
				baseline = currentSpeed;
				_speeds[printerId] = baseline;
			}

			return baseline;
		}

		public bool TryTake(int printerId, out float baseline)
		{
			if (!_speeds.TryGetValue(printerId, out baseline))
			{
				return false;
			}

			_speeds.Remove(printerId);
			return true;
		}

		public void Retain(ICollection<int> livePrinterIds)
		{
			List<int> stale = new List<int>();
			foreach (KeyValuePair<int, float> entry in _speeds)
			{
				if (!livePrinterIds.Contains(entry.Key))
				{
					stale.Add(entry.Key);
				}
			}

			for (int i = 0; i < stale.Count; i++)
			{
				_speeds.Remove(stale[i]);
			}
		}

		public void Clear()
		{
			_speeds.Clear();
		}
	}
}
