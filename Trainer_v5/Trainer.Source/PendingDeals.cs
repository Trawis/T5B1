using System.Collections.Generic;

namespace Trainer_v5
{
	// The deals still waiting for the player's answer (#198). The game mirrors its private pending list into one list
	// view of the deal window and keeps it current (DealWindow.UpdateNewDeals), minus the deals hidden by the player's
	// filter. That list is found by content: it is the one list of deals that holds none of the active deals.
	internal static class PendingDeals
	{
		// The pending deals, an empty list when there are none, or null when no single list can be identified.
		public static List<Deal> Select(List<List<object>> lists, List<Deal> activeDeals)
		{
			List<Deal> pending = null;
			foreach (var items in lists)
			{
				var deals = AsDeals(items);
				if (deals == null || ContainsAny(deals, activeDeals))
				{
					continue;
				}

				if (pending != null)
				{
					return null;
				}

				pending = deals;
			}

			return pending ?? new List<Deal>();
		}

		// Null unless the list is non-empty and holds only deals.
		private static List<Deal> AsDeals(List<object> items)
		{
			if (items.Count == 0)
			{
				return null;
			}

			var deals = new List<Deal>();
			foreach (var item in items)
			{
				var deal = item as Deal;
				if (deal == null)
				{
					return null;
				}

				deals.Add(deal);
			}

			return deals;
		}

		private static bool ContainsAny(List<Deal> deals, List<Deal> candidates)
		{
			foreach (var candidate in candidates)
			{
				if (deals.Contains(candidate))
				{
					return true;
				}
			}

			return false;
		}
	}
}
