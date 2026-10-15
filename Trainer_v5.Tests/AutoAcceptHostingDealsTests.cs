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
	}
}
