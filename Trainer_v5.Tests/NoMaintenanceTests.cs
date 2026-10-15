using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// No Maintenance repairs through the game's RepairMe, with a factor that completes the repair (#195).
	public class NoMaintenanceTests
	{
		[Fact]
		public void RepairIsRequestedWithAFactorNotAsASingleDefaultStep()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs"));

			Assert.Contains("RepairMe(RepairFactor)", source);
			Assert.DoesNotContain("RepairMe()", source);
			Assert.DoesNotContain(".Broken = false", source);
			Assert.DoesNotContain(".Quality = ", source);
		}
	}
}
