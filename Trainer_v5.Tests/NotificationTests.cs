using System;
using System.Collections.Generic;
using System.IO;
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

		[Fact]
		public void OnlyNotificationCallsTheGamePopupAndDialogApis()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(file);
				if (name == "Notification.cs")
					continue;

				var source = SourceCode.Normalize(File.ReadAllText(file));
				if (source.Contains("AddPopupMessage("))
					offenders.Add(name + ": AddPopupMessage");
				if (source.Contains("WindowManager.SpawnDialog("))
					offenders.Add(name + ": SpawnDialog");
				if (name != "Confirmation.cs" && source.Contains("ShowMessageBox("))
					offenders.Add(name + ": ShowMessageBox");
				if (source.Contains("PopupManager.AddPopup("))
					offenders.Add(name + ": PopupManager.AddPopup");
			}

			Assert.True(offenders.Count == 0, "Use Notification.Popup / ShowInfo / ShowWarning / ShowError: " + string.Join("; ", offenders));
		}

		[Fact]
		public void ThePopupUsesTheNoneIdAndANonIssueSound()
		{
			var body = SourceCode.Body(SourceCode.Read("Notification.cs"), "public static void Popup(");

			Assert.Contains(", PopupManager.PopupIDs.None);", body);
			Assert.Contains("PopupManager.NotificationSound.Neutral", body);
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
