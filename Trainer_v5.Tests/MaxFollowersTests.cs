using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Max Followers fills the game's own cap once; it adds no arbitrary amounts (#232).
	public class MaxFollowersTests
	{
		[Fact]
		public void TheActionRecomputesTheCapAndFillsItWithoutAddingBillions()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "WorkItemFeatures.cs"));
			var start = source.IndexOf("internal static void MaxFollowersAction", System.StringComparison.Ordinal);
			var body = source.Substring(start, source.IndexOf("internal static void MaxFollowers()", start, System.StringComparison.Ordinal) - start);

			Assert.Contains("ReEvaluateMaxFollowers()", body);
			Assert.Contains("Followers = WorkItem.MaxFollowers", body);
			Assert.DoesNotContain("1000000000", body);
			Assert.DoesNotContain("FollowerChange", body);
			Assert.DoesNotContain("+=", body);
		}
	}
}
