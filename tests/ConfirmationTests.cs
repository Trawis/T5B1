using System;
using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class ConfirmationTests
	{
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

		// On Yes the sequence runs with the readiness check, the save check and the two message routes; no questionID
		// (which offers "Don't ask again"), no input dialog and no pause.
		[Fact]
		public void TheHelperUsesTheNativeYesNoDialogWithoutQuestionIdOrPauseAndRunsTheSequenceOnYes()
		{
			var type = typeof(Confirmation);
			var steps = CompiledCalls.Steps(type, "AskAndApply");

			CompiledCalls.InOrder(type, "AskAndApply", "call Helpers.RequireGameReady", "call Confirmation.ScopeChangedText", "ldftn Notification.ShowError", "ldftn Notification.Popup", "call Confirmation.Run", "call WindowManager.ShowMessageBox");
			Assert.DoesNotContain(steps, step => step.StartsWith("ldstr ", StringComparison.Ordinal));
			CompiledCalls.NeverCalls(type, "AskAndApply", "ldsfld GameSettings.ForcePause");
			CompiledCalls.NeverCalls(type, "AskAndApply", "call WindowManager.SpawnInputDialog");
		}

		[Theory]
		[InlineData("ClearLoans")]
		[InlineData("AIBankrupt")]
		[InlineData("UnlockAndClaimAllRewards")]
		public void EmptyScopesGetAMessageAndNoQuestion(string action)
		{
			CompiledCalls.InOrder(typeof(CompanyFeatures), action, "call Helpers.RequireGameReady", "call Notification.ShowInfo", "call Confirmation.AskAndApply");
		}

		[Fact]
		public void MaxReputationMaxesEveryCategory()
		{
			CompiledCalls.Count(typeof(CompanyFeatures), "ApplyMaxReputation", "call Company.ChangeBusinessRep", 6);
		}

		[Fact]
		public void NoTypedYesConfirmationIsLeft()
		{
			CompiledCalls.OnlyIn("ldstr YES");
		}

		// Every action in the architecture confirmation list, and nothing else, asks through AskAndApply.
		[Fact]
		public void TheAuditedSetIsExactlyTheDocumentedOne()
		{
			var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
			foreach (var method in CompiledCalls.AllMethods())
			{
				var owner = method.Key.Substring(0, method.Key.IndexOf('.'));
				foreach (var step in method.Value)
				{
					if (step == "call Confirmation.AskAndApply")
					{
						int count;
						counts.TryGetValue(owner, out count);
						counts[owner] = count + 1;
					}
				}
			}

			var found = new List<string>();
			foreach (var pair in counts)
			{
				found.Add(pair.Key + ":" + pair.Value);
			}

			Assert.Equal("AmazingCompatibility:1, CompanyFeatures:6, EmployeeFeatures:2, FounderToggle:1, InstantHire:1, QuietFire:1, SkipTime:1, WorkItemFeatures:1", string.Join(", ", found));
		}

		[Theory]
		[InlineData("TakeoverCompanyAction")]
		[InlineData("SubDCompanyAction")]
		[InlineData("TakeOver")]
		[InlineData("MakeSubsidiary")]
		[InlineData("AdjustAIFunds")]
		[InlineData("MaxMarketRecognition")]
		[InlineData("TransferOffshoreFunds")]
		[InlineData("UnlockFurniture")]
		[InlineData("UnlockAllSpace")]
		[InlineData("MonthDays")]
		public void RoutineActionsDoNotAsk(string action)
		{
			CompiledCalls.NeverCalls(typeof(CompanyFeatures), action, "call Confirmation.AskAndApply");
		}
	}
}
