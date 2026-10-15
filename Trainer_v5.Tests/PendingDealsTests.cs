using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace Trainer_v5.Tests
{
	// Telling the pending list from the others, with real deal objects created without their constructors (#198).
	public class PendingDealsTests
	{
		[Fact]
		public void TheListWithoutAnyActiveDealIsThePendingList()
		{
			var active = Deal();
			var firstOffer = Deal();
			var secondOffer = Deal();

			var pending = PendingDeals.Select(Lists(new object[] { active }, new object[] { firstOffer, secondOffer }), Deals(active));

			Assert.Equal(new[] { firstOffer, secondOffer }, pending.ToArray());
		}

		[Fact]
		public void ARejectedOfferIsGoneFromTheGamesPendingListSoItIsNeverReturned()
		{
			var active = Deal();
			var rejected = Deal();
			var stillPending = Deal();
			// The game removes a rejected offer from its pending list; it only stays in AllDeals.
			var pending = PendingDeals.Select(Lists(new object[] { active }, new object[] { stillPending }), Deals(active));

			Assert.Equal(new[] { stillPending }, pending.ToArray());
			Assert.DoesNotContain(rejected, pending);
		}

		[Fact]
		public void CancelledAndAcceptedDealsAreNotPendingOffers()
		{
			var accepted = Deal();
			var cancelledEarlier = Deal();

			var onlyActive = PendingDeals.Select(Lists(new object[] { accepted }, new object[0]), Deals(accepted));
			var nothingAtAll = PendingDeals.Select(Lists(new object[0], new object[0]), Deals());

			Assert.Empty(onlyActive);
			Assert.Empty(nothingAtAll);
			Assert.DoesNotContain(cancelledEarlier, onlyActive);
		}

		[Fact]
		public void WithNoActiveDealsASingleListOfDealsIsThePendingList()
		{
			var offer = Deal();

			var pending = PendingDeals.Select(Lists(new object[] { offer }, new object[0]), Deals());

			Assert.Equal(new[] { offer }, pending.ToArray());
		}

		[Fact]
		public void ListsOfOtherThingsAreIgnored()
		{
			var active = Deal();
			var offer = Deal();

			var pending = PendingDeals.Select(Lists(new object[] { active }, new object[] { offer }, new object[] { "Printing", "Hosting" }, new object[] { offer, "mixed" }), Deals(active));

			Assert.Equal(new[] { offer }, pending.ToArray());
		}

		[Fact]
		public void TwoCandidateListsAreAmbiguousSoNothingIsReturned()
		{
			var active = Deal();

			var pending = PendingDeals.Select(Lists(new object[] { active }, new object[] { Deal() }, new object[] { Deal() }), Deals(active));

			Assert.Null(pending);
		}

		private static Deal Deal()
		{
			return (Deal)FormatterServices.GetUninitializedObject(typeof(ServerDeal));
		}

		private static List<Deal> Deals(params Deal[] deals)
		{
			return new List<Deal>(deals);
		}

		private static List<List<object>> Lists(params object[][] lists)
		{
			var result = new List<List<object>>();
			foreach (var items in lists)
			{
				result.Add(new List<object>(items));
			}

			return result;
		}
	}
}
