using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SettingActionRowsTests
	{
		// A moved or removed button fails here until this list is edited on purpose.
		private static readonly string[] GoldenActions =
		{
			"AddMoney=Actions/Company", "AddAIFunds=Actions/Company", "TakeoverCompany=Actions/Company", "SubsidiaryCompany=Actions/Company",
			"Bankrupt=Actions/Company", "BankruptAll=Actions/Company", "ClearAllLoans=Actions/Company", "TransferOffshoreFunds=Actions/Company",
			"MaxReputation=Actions/Company", "DaysPerMonth=Actions/Company", "UnlockAndClaimRewards=Actions/Company",
			"MaxSkill=Actions/Employee", "ResetAge=Actions/Employee",
			"SetProductPrice=Actions/Product", "SetProductStock=Actions/Product", "SetActiveUsers=Actions/Product", "MaxFollowers=Actions/Product",
			"MaxMarketRecognition=Actions/Product", "InstantMarketing=Actions/Product", "SellProductsStock=Actions/Product",
			"FixBugs=Actions/Work", "InstantResearch=Actions/Work", "InstantPorting=Actions/Work", "OptimizeTeamAssignments=Actions/Work", "ExtendDeadline=Actions/Work",
			"UnlockAllFurniture=Actions/Environment", "UnlockAllSpace=Actions/Environment",
			"Discord=Settings/None"
		};

		[Fact]
		public void EveryActionButtonIsOnExactlyTheGoldenTabAndDomain()
		{
			var actual = new List<string>();
			foreach (var row in SettingActionRows.Create())
			{
				actual.Add(row.LocKey + "=" + row.Tab + "/" + row.Domain);
			}

			Assert.Equal(GoldenActions.Length, actual.Count);
			Assert.Equal(System.Linq.Enumerable.OrderBy(GoldenActions, x => x, System.StringComparer.Ordinal), System.Linq.Enumerable.OrderBy(actual, x => x, System.StringComparer.Ordinal));
		}

		[Fact]
		public void EveryRowHasAUniqueKeyATextAHandlerAndAValidPlace()
		{
			var rows = SettingActionRows.Create();
			var keys = new HashSet<string>();
			foreach (var row in rows)
			{
				Assert.True(keys.Add(row.LocKey), "Duplicate localization key " + row.LocKey);
				Assert.False(string.IsNullOrEmpty(row.LocKey));
				Assert.False(string.IsNullOrEmpty(row.DefaultText));
				Assert.NotNull(row.Action);
				Assert.True(row.Tab == SettingsTab.Actions || row.Tab == SettingsTab.Settings, row.LocKey + " is not on the Actions or Settings tab.");
				Assert.Equal(row.Tab == SettingsTab.Actions, row.Domain != TrainerDomain.None);
			}
		}

		[Fact]
		public void EveryActionDomainOfThePageIsUsedAndEveryUsedDomainIsOnThePage()
		{
			var used = new HashSet<TrainerDomain>();
			foreach (var row in SettingActionRows.Create())
			{
				if (row.Tab == SettingsTab.Actions)
				{
					used.Add(row.Domain);
				}
			}

			var page = SettingsWindowState.ActionDomains();
			Assert.Equal(page.Length, new HashSet<TrainerDomain>(page).Count);
			Assert.True(used.SetEquals(page), "The Actions page sections and the domains of its buttons differ.");
		}

		[Fact]
		public void EveryRowDefaultTextMatchesTheEnglishFile()
		{
			var path = Path.Combine(RepositoryFiles.Root, "Trainer_v5", "Trainer.Localization", "English", "Trainer.tyd");
			var english = new Dictionary<string, string>();
			foreach (Match match in Regex.Matches(File.ReadAllText(path), @"^(?<key>\w+)\s+""(?<text>[^""]*)""", RegexOptions.Multiline))
			{
				english[match.Groups["key"].Value] = match.Groups["text"].Value;
			}

			var wrong = new List<string>();
			foreach (var row in SettingActionRows.Create())
			{
				string text;
				if (!english.TryGetValue(row.LocKey, out text))
				{
					wrong.Add(row.LocKey + ": missing from English/Trainer.tyd");
				}
				else if (text != row.DefaultText)
				{
					wrong.Add(row.LocKey + ": \"" + row.DefaultText + "\" vs \"" + text + "\"");
				}
			}

			Assert.True(wrong.Count == 0, string.Join("; ", wrong));
		}
	}
}
