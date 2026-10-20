using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Trainer Settings toggles are data (#274); these keep the table consistent with the settings and strings.
	public class SettingToggleRowsTests
	{
		private static readonly string[] Explicit = { "UnlimitedSubsidiaries", "ForceLights", "ShowRoomCeilings" };

		// Persisted but with no Settings window toggle today; a gap to close, not a rule.
		private static readonly string[] WithoutToggle = { "DisableSkillDecay", "ReduceBoxPrice", "AutoMaxMarketShare", "MoreCreativity", "ReduceExpansionCost", "Experimental" };

		[Fact]
		public void EveryPersistedToggleHasOneRowOrIsExplicitInTheWindow()
		{
			var rowKeys = Enumerable.ToList(Enumerable.Select(SettingToggleRows.Create(), row => row.Key.Key));
			Assert.Equal(rowKeys.Count, Enumerable.Count(Enumerable.Distinct(rowKeys)));

			var declared = new List<string>();
			foreach (var field in typeof(SettingKeys).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.FieldType == typeof(ToggleKey))
				{
					declared.Add(((ToggleKey)field.GetValue(null)).Key);
				}
			}

			var missing = Enumerable.ToList(Enumerable.Where(declared, key => !rowKeys.Contains(key) && System.Array.IndexOf(Explicit, key) < 0 && System.Array.IndexOf(WithoutToggle, key) < 0));
			Assert.True(missing.Count == 0, "No Settings window row for: " + string.Join(", ", missing));
			Assert.All(rowKeys, key => Assert.Contains(key, declared));
		}

		// Every toggle the window shows, with the domain (section) it sits in: a moved or removed toggle fails here until
		// this list is edited on purpose (#449). Retired keys and the native-Options Experimental switch are not listed.
		private static readonly string[] GoldenToggleDomains =
		{
			"NoNeeds=Employee", "NoStress=Employee", "FreeEmployees=Employee", "FreeStaff=Employee", "FullSatisfaction=Employee", "LockAge=Employee",
			"NoVacation=Employee", "NoSickness=Employee", "NoEducationCost=Employee", "IncreaseWalkSpeed=Employee", "IncreaseBookshelfSkill=Employee", "MoreInspiration=Employee",
			"DisableFireInspection=Environment", "FullEnvironment=Environment", "FullRoomBrightness=Environment", "TemperatureLock=Environment", "NoMaintenance=Environment",
			"NoiseReduction=Environment", "CleanRooms=Environment", "DisableFires=Environment", "DisableBurglars=Environment", "DisableFurnitureStealing=Environment",
			"NoWaterElectricity=Environment", "ForceLights=Environment", "ShowRoomCeilings=Environment",
			"AutoResearchStart=Work", "AutoEndDesign=Work", "AutoContractProgression=Work", "AutoEndResearch=Work", "AutoEndPatent=Work", "AutoPorting=Work",
			"DisableForcePause=Work", "DisableForceFreeze=Work",
			"ReduceISPCost=Company", "NoServerCost=Company", "NoInsuranceCost=Company", "NoLoanInterest=Company", "NoOffshoreHeat=Company", "NoFounderDividends=Company",
			"UnlimitedSubsidiaries=Company",
			"FreePrint=Product", "IncreasePrintSpeed=Product", "MoreHostingDeals=Product", "AutoAcceptHostingDeals=Product", "DigitalDistributionMonopol=Product",
			"NoMissedSupportTickets=Product", "FreeMarketing=Product", "IncreaseCourierCapacity=Product"
		};

		private static List<SettingToggleRow> AllRows()
		{
			var rows = SettingToggleRows.Create();
			rows.AddRange(SettingToggleRows.CreateImmediate());
			return rows;
		}

		[Fact]
		public void RowsHaveADomainAndAUniqueLocalizationKey()
		{
			var rows = AllRows();

			Assert.All(rows, row => Assert.NotEqual(TrainerDomain.None, row.Domain));
			Assert.Equal(rows.Count, Enumerable.Count(Enumerable.Distinct(Enumerable.Select(rows, row => row.LocKey))));
			Assert.All(rows, row => Assert.False(string.IsNullOrEmpty(row.DefaultText)));
		}

		[Fact]
		public void EveryToggleSitsInExactlyTheGoldenDomain()
		{
			var actual = Enumerable.ToList(Enumerable.Select(AllRows(), row => row.Key.Key + "=" + row.Domain));

			Assert.Equal(Enumerable.Count(GoldenToggleDomains), actual.Count);
			Assert.Equal(Enumerable.OrderBy(GoldenToggleDomains, x => x, System.StringComparer.Ordinal), Enumerable.OrderBy(actual, x => x, System.StringComparer.Ordinal));
		}

		[Fact]
		public void TheImmediateTogglesAreTheExplicitOnesAndNoStandardRowRepeatsThem()
		{
			var immediate = Enumerable.ToList(Enumerable.Select(SettingToggleRows.CreateImmediate(), row => row.Key.Key));
			var standard = Enumerable.ToList(Enumerable.Select(SettingToggleRows.Create(), row => row.Key.Key));

			Assert.Equal(Explicit, Enumerable.ToArray(immediate));
			Assert.Empty(Enumerable.Intersect(immediate, standard));
		}

		[Fact]
		public void EveryToggleDomainOfThePageIsUsedAndEveryUsedDomainIsOnThePage()
		{
			var used = Enumerable.ToList(Enumerable.Distinct(Enumerable.Select(AllRows(), row => row.Domain)));
			var page = SettingsWindowState.ToggleDomains();

			Assert.Equal(Enumerable.Count(page), Enumerable.Count(Enumerable.Distinct(page)));
			Assert.Empty(Enumerable.Except(used, page));
			Assert.Empty(Enumerable.Except(page, used));
		}

		[Fact]
		public void DisableBurglarsHasARowNextToDisableFires()
		{
			var rows = SettingToggleRows.Create();
			int fires = Enumerable.ToList(Enumerable.Select(rows, row => row.Key.Key)).IndexOf("DisableFires");
			int burglars = Enumerable.ToList(Enumerable.Select(rows, row => row.Key.Key)).IndexOf("DisableBurglars");

			Assert.True(burglars >= 0, "Disable Burglars has no Settings row.");
			Assert.Equal(fires + 1, burglars);
			Assert.Equal(rows[fires].Domain, rows[burglars].Domain);
			Assert.Equal("Disable Burglars", rows[burglars].DefaultText);
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
			foreach (var row in AllRows())
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
