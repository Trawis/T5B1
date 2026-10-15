using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Auto Accept Hosting Deals (#198): skip deals the game already answered, and register only after a real accept.
	public class AutoAcceptHostingDealsTests
	{
		[Fact]
		public void OnlyValidNeverAcceptedDealsAreAcceptedAndRegistrationFollowsSuccess()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "MarketFeatures.cs"));
			var start = source.IndexOf("internal static void ApplyAutoAcceptHostingDeals", System.StringComparison.Ordinal);
			var end = source.IndexOf("private static bool HasBeenAccepted", start, System.StringComparison.Ordinal);
			var body = source.Substring(start, end - start);

			Assert.Contains("HasBeenAccepted(serverDeal)", body);
			Assert.Contains("StillValid(false)", body);
			var accept = body.IndexOf("ActuallyAcceptDeal", System.StringComparison.Ordinal);
			var active = body.IndexOf("serverDeal.Active", accept, System.StringComparison.Ordinal);
			var register = body.IndexOf("RegisterWithServer", System.StringComparison.Ordinal);
			Assert.True(accept > 0 && active > accept && register > active, "RegisterWithServer must come after the accept result is checked");
		}
	}
}
