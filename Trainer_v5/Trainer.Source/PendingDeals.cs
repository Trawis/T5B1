using System.Collections.Generic;

namespace Trainer_v5
{
	internal static class PendingDeals
	{
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
