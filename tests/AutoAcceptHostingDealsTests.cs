using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class AutoAcceptHostingDealsTests
	{
		[Fact]
		public void OnlyValidPendingDealsAreAcceptedAndRegistrationFollowsSuccess()
		{
			var type = typeof(MarketFeatures);

			CompiledCalls.NeverCalls(type, "ApplyAutoAcceptHostingDeals", "ldfld DealWindow.AllDeals");
			CompiledCalls.InOrder(type, "ApplyAutoAcceptHostingDeals", "call MarketFeatures.FindPendingDeals", "call MarketFeatures.WarnOnce", "call Deal.StillValid", "call DealWindow.ActuallyAcceptDeal", "ldfld Deal.Active", "call GameSettings.RegisterWithServer");
			CompiledCalls.Adjacent(type, "ApplyAutoAcceptHostingDeals", "ldc 0", "call Deal.StillValid");
		}

		[Fact]
		public void TheStrongestGroupIsSelectedAndTheFirstWinsATie()
		{
			Assert.Equal(1, MarketFeatures.FindStrongestIndex(new List<float> { 10f, 50f, 50f }));
		}
	}
}
