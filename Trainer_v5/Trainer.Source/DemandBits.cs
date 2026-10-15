namespace Trainer_v5
{
	// The employee's demand bit sets, changed together; the detail window shows demands only while DemandsMet is set (#185).
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

		// demand/sibling: the toggled flag and the other flag of its choice (0 if none); choiceId: DemandChoice.ID.
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
