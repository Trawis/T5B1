using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// One message convention (#451): every trainer HUD popup starts with "Trainer: " exactly once, dialogs and input
	// prompts get no automatic prefix, and Notification is the only code that talks to the game's popup and dialog APIs.
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
		public void ALocalizedTextThatCarriesThePrefixIsNotPrefixedAgain()
		{
			// The English file keeps "Trainer: " inside the popup values (translations are not required to); the
			// formatted result must still carry it once.
			var formatted = string.Format("Trainer: Price for {0} has been setted up!", "Alpha");

			Assert.Equal("Trainer: Price for Alpha has been setted up!", Notification.WithPrefix(formatted));
		}

		[Fact]
		public void OnlyNotificationCallsTheGamePopupAndDialogApis()
		{
			// Allow-list: Notification.cs. Input dialogs (SpawnInputDialog, via InputHelper) are a separate surface and are
			// not listed here because they never get the popup prefix.
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
		public void EveryPopupGoesThroughThePrefixRuleAndNoDialogDoes()
		{
			var source = SourceCode.Read("Notification.cs");

			Assert.Contains("WithPrefix(msg)", SourceCode.Body(source, "public static void Popup("));
			foreach (var method in new[] { "public static void ShowInfo(", "public static void ShowWarning(", "public static void ShowError(" })
			{
				var body = SourceCode.Body(source, method);
				Assert.DoesNotContain("Prefix", body);
				Assert.Contains("SpawnDialog(msg, false, DialogWindow.DialogType.", body);
			}
		}

		[Fact]
		public void TheDialogsKeepTheirSeverityAndTheErrorsAreErrors()
		{
			var source = SourceCode.Read("Notification.cs");

			Assert.Contains("DialogWindow.DialogType.Information", SourceCode.Body(source, "public static void ShowInfo("));
			Assert.Contains("DialogWindow.DialogType.Warning", SourceCode.Body(source, "public static void ShowWarning("));
			Assert.Contains("DialogWindow.DialogType.Error", SourceCode.Body(source, "public static void ShowError("));

			// A product or company that cannot be found, or whose name is ambiguous, is a refusal and is shown as an error.
			var product = SourceCode.Read("Features", "ProductFeatures.cs");
			Assert.DoesNotContain("Notification.ShowInfo(error)", product);
			var company = SourceCode.Read("Features", "CompanyFeatures.cs");
			Assert.DoesNotContain("Notification.ShowInfo(string.Format(\"CompanyNotFound\"", company);
			Assert.DoesNotContain("Notification.ShowInfo(string.Format(\"CompanyNameAmbiguous\"", company);
		}

		[Fact]
		public void InputDialogPromptsAndTitlesAreNeverPrefixed()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(file);
				if (name == "Notification.cs")
					continue;

				foreach (var line in File.ReadAllText(file).Replace("\r\n", "\n").Split('\n'))
				{
					var asksForInput = line.Contains("SpawnInputDialog(") || line.Contains("InputHelper.Request");
					if (asksForInput && (line.Contains("Trainer:") || line.Contains("Notification.Prefix") || line.Contains("WithPrefix")))
						offenders.Add(name + ": " + line.Trim());
				}
			}

			Assert.True(offenders.Count == 0, "Input dialogs get no Trainer prefix: " + string.Join("; ", offenders));
		}
	}
}
