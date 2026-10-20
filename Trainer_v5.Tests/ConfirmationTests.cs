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

		[Theory]
		[InlineData(3, 3, true)]
		[InlineData(1, 1, true)]
		[InlineData(3, 2, false)]
		[InlineData(3, 4, false)]
		[InlineData(3, 0, false)]
		[InlineData(0, 0, false)]
		public void OnlyTheSameNonEmptyScopeIsConfirmed(int shown, int current, bool expected)
		{
			Assert.Equal(expected, Confirmation.IsScopeUnchanged(shown, current));
		}

		[Fact]
		public void TheHelperUsesTheNativeYesNoDialogWithoutQuestionIdOrPause()
		{
			var source = SourceCode.Read("Confirmation.cs");
			var ask = SourceCode.Body(source, "internal static void Ask(");

			Assert.Contains("WindowManager.Instance.ShowMessageBox(message, false, DialogWindow.DialogType.Warning,", ask);
			Assert.DoesNotContain("questionID", source);
			Assert.DoesNotContain("IgnoreQuestions", source);
			Assert.DoesNotContain("SpawnInputDialog", source);
			Assert.DoesNotContain("ForcePause", source);
			SourceCode.AssertInOrder(ask, "RequireGameReady(actionName)", "onConfirm()", "the game is checked before the action runs");
		}

		[Fact]
		public void TheActionOnlyRunsFromTheYesCallback()
		{
			var ask = SourceCode.Body(SourceCode.Read("Confirmation.cs"), "internal static void Ask(");

			Assert.Single(Regex.Matches(ask, @"onConfirm\(\)"));
			Assert.Contains("() => {", ask);
		}

		[Fact]
		public void TheNewActionsAskFirstAndOnlyChangeStateInTheirApplyMethod()
		{
			foreach (var name in new[] { "ClearLoans", "AIBankrupt", "UnlockAndClaimAllRewards" })
			{
				var entry = Company("internal static void " + name);

				Assert.Contains("Confirmation.Ask(", entry);
				Assert.Contains("RequireGameReady", entry);
				Assert.DoesNotContain("Bankrupt = true", entry);
				Assert.DoesNotContain(".Clear()", entry);
				Assert.DoesNotContain("AddRange", entry);
				SourceCode.AssertInOrder(entry, "ShowInfo", "Confirmation.Ask(", "an empty scope gets a message and no dialog");
			}

			var bankrupt = Company("private static void ApplyAIBankrupt");
			var loans = Company("private static void ApplyClearLoans");
			var rewards = Company("private static void ApplyUnlockAndClaimAllRewards");

			SourceCode.AssertInOrder(bankrupt, "ReferenceEquals(game, Settings)", "IsScopeUnchanged", "the game, then the scope");
			SourceCode.AssertInOrder(bankrupt, "IsScopeUnchanged", "Bankrupt = true", "the scope is checked before the flag");
			SourceCode.AssertInOrder(loans, "ReferenceEquals(game, Settings)", "IsScopeUnchanged", "the game, then the scope");
			SourceCode.AssertInOrder(loans, "IsScopeUnchanged", ".Clear()", "the scope is checked before the loans go");
			SourceCode.AssertInOrder(rewards, "ReferenceEquals(game, Settings)", "IsScopeUnchanged", "the game, then the scope");
			SourceCode.AssertInOrder(rewards, "IsScopeUnchanged", "AddRange", "the scope is checked before the rewards are set");
		}

		[Fact]
		public void MaxReputationAsksBeforeItChangesAnything()
		{
			var entry = Company("internal static void MaxReputation");
			var apply = Company("private static void ApplyMaxReputation");

			Assert.Contains("Confirmation.Ask(", entry);
			Assert.DoesNotContain("ChangeBusinessRep", entry);
			Assert.Equal(6, Regex.Matches(apply, @"ChangeBusinessRep\(1f,").Count);
		}

		[Fact]
		public void NoTypedYesConfirmationIsLeft()
		{
			foreach (var item in AllSources())
			{
				Assert.False(item.Value.Contains("\"YES\""), item.Key + " still has a typed YES");
			}
		}

		[Fact]
		public void TheAuditedSetIsExactlyTheDocumentedOne()
		{
			// A new confirmation needs a row in the architecture.md decision matrix and in this list.
			var found = new List<string>();
			foreach (var item in AllSources())
			{
				if (item.Key == "Confirmation.cs")
					continue;

				Assert.False(item.Value.Contains("ShowMessageBox"), item.Key + " must ask through Confirmation.Ask");
				var calls = Regex.Matches(item.Value, @"Confirmation\.Ask\(").Count;
				if (calls > 0)
					found.Add(item.Key + ":" + calls);
			}

			found.Sort(StringComparer.Ordinal);
			Assert.Equal("CompanyFeatures.cs:5, FounderToggle.cs:1", string.Join(", ", found));

			foreach (var name in new[] { "ClearLoans", "AIBankrupt", "UnlockAndClaimAllRewards", "ForceBankruptAction", "MaxReputation" })
			{
				Assert.Contains("Confirmation.Ask(", Company("internal static void " + name));
			}

			Assert.Contains("Confirmation.Ask(", SourceCode.Body(SourceCode.Read("FounderToggle.cs"), "public static void Start"));
		}

		[Fact]
		public void RoutineActionsDoNotAsk()
		{
			foreach (var method in new[] { "TakeoverCompanyAction", "SubDCompanyAction", "MaxMarketRecognition", "TransferOffshoreFunds", "UnlockFurniture", "UnlockAllSpace", "MonthDays" })
			{
				Assert.DoesNotContain("Confirmation.", Company("internal static void " + method));
			}
		}
	}
}
