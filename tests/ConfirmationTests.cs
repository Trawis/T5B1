using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class ConfirmationTests
	{
		private static string Company(string signature)
		{
			return SourceCode.Body(SourceCode.Read("Features", "CompanyFeatures.cs"), signature);
		}

		private static Dictionary<string, string> AllSources()
		{
			var result = new Dictionary<string, string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				result[Path.GetFileName(file)] = SourceCode.Normalize(File.ReadAllText(file));
			}

			return result;
		}

		[Fact]
		public void OnlyTheSameTargetsInAnyOrderAreConfirmed()
		{
			object a = new object(), b = new object(), c = new object();
			var shown = new List<object> { a, b };

			Assert.True(Confirmation.IsSameTargets(shown, new List<object> { b, a }));
			Assert.False(Confirmation.IsSameTargets(shown, new List<object> { a, c }));
			Assert.False(Confirmation.IsSameTargets(shown, new List<object> { a }));
			Assert.False(Confirmation.IsSameTargets(shown, new List<object> { a, b, c }));
			Assert.False(Confirmation.IsSameTargets(new List<object>(), new List<object>()));
		}

		// Runs the sequence and records what it did, in order.
		private static string RunSequence(bool ready, string change, string result)
		{
			var steps = new List<string>();
			Confirmation.Run(
				() => { steps.Add("ready"); return ready; },
				() => { steps.Add("check"); return change; },
				() => { steps.Add("apply"); return result; },
				text => steps.Add("changed:" + text),
				text => steps.Add("result:" + text));
			return string.Join(" ", steps);
		}

		[Theory]
		[InlineData(false, null, "done", "ready")]
		[InlineData(true, "moved", "done", "ready check changed:moved")]
		[InlineData(true, null, "done", "ready check apply result:done")]
		[InlineData(true, null, null, "ready check apply")]
		public void TheSequenceAppliesOnlyWhenReadyAndUnchangedAndThenReports(bool ready, string change, string result, string expected)
		{
			Assert.Equal(expected, RunSequence(ready, change, result));
		}

		[Fact]
		public void TheHelperUsesTheNativeYesNoDialogWithoutQuestionIdOrPauseAndRunsTheSequenceOnYes()
		{
			var source = SourceCode.Read("Confirmation.cs");
			var ask = SourceCode.Body(source, "internal static void AskAndApply(");

			Assert.Contains("WindowManager.Instance.ShowMessageBox(question, false, DialogWindow.DialogType.Warning, () => Run(", ask);
			Assert.Contains("Helpers.RequireGameReady(actionName)", ask);
			Assert.Contains("!ReferenceEquals(game, GameSettings.Instance) ? ScopeChangedText() :", ask);
			Assert.Contains("Notification.ShowError, Notification.Popup", ask);
			Assert.DoesNotContain("questionID", source);
			Assert.DoesNotContain("IgnoreQuestions", source);
			Assert.DoesNotContain("SpawnInputDialog", source);
			Assert.DoesNotContain("ForcePause", source);
		}

		[Fact]
		public void EmptyScopesGetAMessageAndNoQuestion()
		{
			foreach (var name in new[] { "ClearLoans", "AIBankrupt", "UnlockAndClaimAllRewards" })
			{
				var entry = Company("internal static void " + name);

				Assert.Contains("RequireGameReady", entry);
				SourceCode.AssertInOrder(entry, "ShowInfo", "Confirmation.AskAndApply(", "an empty scope gets a message and no dialog");
			}
		}

		[Fact]
		public void MaxReputationMaxesEveryCategory()
		{
			Assert.Equal(6, Regex.Matches(Company("private static string ApplyMaxReputation"), @"ChangeBusinessRep\(1f,").Count);
		}

		[Fact]
		public void NoTypedYesConfirmationIsLeft()
		{
			foreach (var item in AllSources())
			{
				Assert.False(item.Value.Contains("\"YES\""), item.Key + " still has a typed YES");
			}
		}

		// Every action in the architecture confirmation list, and nothing else, asks through AskAndApply.
		[Fact]
		public void TheAuditedSetIsExactlyTheDocumentedOne()
		{
			var found = new List<string>();
			foreach (var item in AllSources())
			{
				if (item.Key == "Confirmation.cs")
					continue;

				Assert.False(item.Value.Contains("ShowMessageBox"), item.Key + " must ask through Confirmation.AskAndApply");
				var calls = Regex.Matches(item.Value, @"Confirmation\.AskAndApply\(").Count;
				if (calls > 0)
					found.Add(item.Key + ":" + calls);
			}

			found.Sort(StringComparer.Ordinal);
			Assert.Equal("AmazingCompatibility.cs:1, CompanyFeatures.cs:6, EmployeeFeatures.cs:2, FounderToggle.cs:1, InstantHire.cs:1, QuietFire.cs:1, SkipTime.cs:1, WorkItemFeatures.cs:1", string.Join(", ", found));
		}

		[Fact]
		public void RoutineActionsDoNotAsk()
		{
			foreach (var method in new[] { "TakeoverCompanyAction", "SubDCompanyAction", "TakeOver(", "MakeSubsidiary(", "AdjustAIFunds(", "MaxMarketRecognition", "TransferOffshoreFunds", "UnlockFurniture", "UnlockAllSpace", "MonthDays" })
			{
				Assert.DoesNotContain("Confirmation.", Company("internal static void " + method));
			}
		}
	}
}
