using Xunit;

namespace Trainer_v5.Tests
{
	public class NoMaintenanceTests
	{
		[Fact]
		public void RepairIsRequestedWithAFactorNotAsASingleDefaultStep()
		{
			var source = SourceCode.Read("Features", "EnvironmentFeatures.cs");

			Assert.Contains("RepairMe(RepairFactor)", source);
			Assert.DoesNotContain("RepairMe()", source);
			Assert.DoesNotContain(".Broken = false", source);
			Assert.DoesNotContain(".Quality = ", source);
		}
	}
}
