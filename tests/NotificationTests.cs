using System;
using Xunit;

namespace Trainer_v5.Tests
{
	public class NotificationTests
	{
		[Theory]
		[InlineData("Offshore funds transferred!", "Trainer: Offshore funds transferred!")]
		[InlineData("Trainer: Offshore funds transferred!", "Trainer: Offshore funds transferred!")]
		[InlineData("Trainer:Offshore funds transferred!", "Trainer: Offshore funds transferred!")]
		[InlineData("Join us on our discord server\nhttps://example.test", "Trainer: Join us on our discord server\nhttps://example.test")]
		[InlineData("trainer: lower case is not the prefix", "Trainer: trainer: lower case is not the prefix")]
		[InlineData("TRAINER: upper case is not the prefix", "Trainer: TRAINER: upper case is not the prefix")]
		[InlineData(" Trainer: leading space is not the prefix", "Trainer:  Trainer: leading space is not the prefix")]
		[InlineData("The Trainer: is inside, not at the start", "Trainer: The Trainer: is inside, not at the start")]
		[InlineData("", "Trainer: ")]
		[InlineData(null, "Trainer: ")]
		public void APopupStartsWithThePrefixExactlyOnce(string message, string expected)
		{
			Assert.Equal(expected, Notification.WithPrefix(message));
		}

		[Theory]
		[InlineData("Company {0} not found!")]
		[InlineData("Trainer: Company {0} not found!")]
		[InlineData("Trainer:Company {0} not found!")]
		[InlineData("")]
		public void ThePrefixRuleIsIdempotent(string message)
		{
			var once = Notification.WithPrefix(message);

			Assert.Equal(once, Notification.WithPrefix(once));
			Assert.StartsWith("Trainer: ", once, System.StringComparison.Ordinal);
			Assert.False(once.StartsWith("Trainer: Trainer:", System.StringComparison.Ordinal));
		}

		// Input dialogs (SpawnInputDialog, via InputHelper) are a separate surface: they never get the popup prefix.
		[Fact]
		public void OnlyNotificationCallsTheGamePopupAndDialogApis()
		{
			CompiledCalls.OnlyIn("call HUD.AddPopupMessage", "Notification");
			CompiledCalls.OnlyIn("call WindowManager.SpawnDialog", "Notification");
			CompiledCalls.OnlyIn("call PopupManager.AddPopup", "Notification");
			CompiledCalls.OnlyIn("call WindowManager.ShowMessageBox", "Confirmation");
		}

		// HUD.AddPopupMessage ignores every id except PopupIDs.None (-1); sound 0 (Issue) would file the popup under Issues.
		[Fact]
		public void ThePopupUsesTheNoneIdAndANonIssueSound()
		{
			CompiledCalls.Adjacent(typeof(Notification), "Popup", "ldc " + (int)PopupManager.NotificationSound.Neutral, "ldc.r4 0", "ldc " + (int)PopupManager.PopupIDs.None);
			CompiledCalls.InOrder(typeof(Notification), "Popup", "call Notification.WithPrefix", "ldc " + (int)PopupManager.PopupIDs.None, "call HUD.AddPopupMessage");
		}

		[Fact]
		public void TheNamedGameConstantsHaveTheValuesTheRoutingRelies()
		{
			Assert.Equal(0, Convert.ToInt32(typeof(PopupManager.NotificationSound).GetField("Issue").GetRawConstantValue()));
			Assert.Equal(3, Convert.ToInt32(typeof(PopupManager.NotificationSound).GetField("Neutral").GetRawConstantValue()));
			Assert.Equal(-1, Convert.ToInt32(typeof(PopupManager.PopupIDs).GetField("None").GetRawConstantValue()));
		}
	}
}
