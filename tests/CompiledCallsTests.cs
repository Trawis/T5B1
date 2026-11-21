using Xunit;
using Xunit.Sdk;

namespace Trainer_v5.Tests
{
	public class CompiledCallsTests
	{
		[Fact]
		public void ALambdaBodyIsReadWhereTheLambdaIsCreated()
		{
			// Update polls the minute watcher only inside a lambda passed to TryExecute.
			CompiledCalls.InOrder(typeof(TrainerBehaviour), "Update", "ldstr MinutePoll", "call GameMinuteWatcher.Poll", "call Helpers.TryExecute");
		}

		[Fact]
		public void TheAssemblyScanFindsARealCallerAndNamesIt()
		{
			var failure = Assert.ThrowsAny<XunitException>(() => CompiledCalls.OnlyIn("call HUD.AddPopupMessage", "Nobody"));

			Assert.Contains("Notification.Popup", failure.Message);
		}

		[Fact]
		public void AMissingMethodFailsWithItsName()
		{
			var failure = Assert.Throws<XunitException>(() => CompiledCalls.Steps(typeof(Helpers), "NoSuchMethod"));

			Assert.Contains("Helpers.NoSuchMethod", failure.Message);
		}

		[Fact]
		public void AMissingOrMisorderedStepFailsWithTheMethodAndTheStep()
		{
			var missing = Assert.ThrowsAny<XunitException>(() => CompiledCalls.Calls(typeof(Notification), "Popup", "call Nowhere.Nothing"));
			var reversed = Assert.ThrowsAny<XunitException>(() => CompiledCalls.InOrder(typeof(Notification), "Popup", "call HUD.AddPopupMessage", "call Notification.WithPrefix"));

			Assert.Contains("Notification.Popup", missing.Message);
			Assert.Contains("call Nowhere.Nothing", missing.Message);
			Assert.Contains("call Notification.WithPrefix", reversed.Message);
		}
	}
}
