namespace Trainer_v5
{
	// The three bit sets the game keeps per employee for lead demands, and how they change together (#185).
	// Employee.AcceptDemand sets DemandsMet, clears DemandsRequested and sets one DemandResults bit of the pair;
	// the native detail window shows demands only while DemandsMet is not zero.
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

		// demand: the flag toggled; sibling: the other flag of its choice (0 if none); choiceId: DemandChoice.ID.
		// Granting and removing run no effect and charge nothing, unlike the game's own AcceptDemand.
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
