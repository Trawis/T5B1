using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Auto Accept Hosting Deals (#198): only the game's pending list is considered, and registration follows a real accept.
	public class AutoAcceptHostingDealsTests
	{
		[Fact]
		public void OnlyValidPendingDealsAreAcceptedAndRegistrationFollowsSuccess()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "MarketFeatures.cs"));
			var start = source.IndexOf("internal static void ApplyAutoAcceptHostingDeals", System.StringComparison.Ordinal);
			var end = source.IndexOf("// Progress holds one slot", start, System.StringComparison.Ordinal);
			var body = source.Substring(start, end - start);

			Assert.DoesNotContain("AllDeals", body);
			Assert.Contains("FindPendingDeals(dealWindow)", body);
			Assert.Contains("StillValid(false)", body);
			var guard = body.IndexOf("pending == null", System.StringComparison.Ordinal);
			var accept = body.IndexOf("ActuallyAcceptDeal", System.StringComparison.Ordinal);
			var active = body.IndexOf("serverDeal.Active", accept, System.StringComparison.Ordinal);
			var register = body.IndexOf("RegisterWithServer", System.StringComparison.Ordinal);
			Assert.True(guard > 0 && accept > guard, "an unidentified list must stop before anything is accepted");
			Assert.True(accept > 0 && active > accept && register > active, "RegisterWithServer must come after the accept result is checked");
		}

		[Fact]
		public void DealsAreOnlyRegisteredWithAGroupTheGameStoresUnderItsName()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "MarketFeatures.cs"));
			var start = source.IndexOf("internal static void ApplyAutoAcceptHostingDeals", System.StringComparison.Ordinal);
			var end = source.IndexOf("internal static int FindStrongestIndex", start, System.StringComparison.Ordinal);
			var body = source.Substring(start, end - start);

			Assert.DoesNotContain("\"TRAINER\"", source);
			Assert.DoesNotContain("new ServerGroup(", source);
			Assert.Contains("Settings.GetServerGroup(group.Name) != null", body);
			Assert.Contains("RegisterWithServer(targetGroup.Name, serverDeal)", body);
			Assert.True(body.IndexOf("strongest < 0", System.StringComparison.Ordinal) < body.IndexOf("ActuallyAcceptDeal", System.StringComparison.Ordinal), "no group must stop before any deal is accepted");
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
