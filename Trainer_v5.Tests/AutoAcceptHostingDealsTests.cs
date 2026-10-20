using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	// Auto Accept Hosting Deals (#198): only the game's pending list is considered, and registration follows a real accept.
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
		public void DealsAreOnlyRegisteredWithAGroupTheGameStoresUnderItsName()
		{
			var source = SourceCode.Read("Features", "MarketFeatures.cs");
			var body = SourceCode.Body(source, "internal static void ApplyAutoAcceptHostingDeals");

			Assert.DoesNotContain("\"TRAINER\"", source);
			Assert.DoesNotContain("new ServerGroup(", source);
			Assert.Contains("Settings.GetServerGroup(group.Name) != null", body);
			Assert.Contains("RegisterWithServer(targetGroup.Name, serverDeal)", body);
			SourceCode.AssertInOrder(body, "strongest < 0", "ActuallyAcceptDeal", "no group must stop before any deal is accepted");
		}

		[Fact]
		public void NoGroupsSelectsNothing()
		{
			Assert.Equal(-1, MarketFeatures.FindStrongestIndex(new List<float>()));
		}

		[Fact]
		public void GroupsAtZeroPowerStillSelectTheFirstRealGroup()
		{
			Assert.Equal(0, MarketFeatures.FindStrongestIndex(new List<float> { 0f, 0f }));
		}

		[Fact]
		public void TheStrongestGroupIsSelectedAndTheFirstWinsATie()
		{
			Assert.Equal(1, MarketFeatures.FindStrongestIndex(new List<float> { 10f, 50f, 50f }));
		}
	}
}
