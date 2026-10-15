using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Deal types are selected by type, never by the text of ToString() (#246).
	public class HostingRewardTypeTests
	{
		[Fact]
		public void NoFeatureSelectsDealsByToString()
		{
			foreach (var path in Directory.GetFiles(Path.Combine(RepositoryFiles.SourceDirectory, "Features"), "*.cs"))
			{
				var source = File.ReadAllText(path);

				Assert.DoesNotContain(".ToString() == \"ServerDeal\"", source);
			}
		}
	}
}
