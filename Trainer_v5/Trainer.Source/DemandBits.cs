namespace Trainer_v5
{
	internal sealed class DemandBits
	{
		public readonly uint Results;
		public readonly uint Met;
		public readonly uint Requested;

		public DemandBits(uint results, uint met, uint requested)
		{
			Results = results;
			Met = met;
			Requested = requested;
		}

		public static DemandBits Apply(DemandBits current, uint demand, uint sibling, uint choiceId, bool on)
		{
			var results = current.Results;
			var met = current.Met;
			var requested = current.Requested;
			if (on)
			{
				results = (results & ~sibling) | demand;
				met |= choiceId;
				requested &= ~choiceId;
			}
			else
			{
				results &= ~demand;
				if ((results & (demand | sibling)) == 0)
				{
					met &= ~choiceId;
				}
			}

			return new DemandBits(results, met, requested);
		}
	}
}
