using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class AutoAcceptHostingDealsTests
	{
		[Fact]
		public void OnlyValidPendingDealsAreAcceptedAndRegistrationFollowsSuccess()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "MarketFeatures.cs"), "internal static void ApplyAutoAcceptHostingDeals");

			Assert.DoesNotContain("AllDeals", body);
			Assert.Contains("FindPendingDeals(dealWindow)", body);
			Assert.Contains("StillValid(false)", body);
			SourceCode.AssertInOrder(body, "pending == null", "ActuallyAcceptDeal", "an unidentified list must stop before anything is accepted");
			var afterAccept = SourceCode.Span(body, "ActuallyAcceptDeal");
			SourceCode.AssertInOrder(afterAccept, "serverDeal.Active", "RegisterWithServer", "RegisterWithServer must come after the accept result is checked");
		}

		[Fact]
		public void TheStrongestGroupIsSelectedAndTheFirstWinsATie()
		{
			Assert.Equal(1, MarketFeatures.FindStrongestIndex(new List<float> { 10f, 50f, 50f }));
		}
	}
}
