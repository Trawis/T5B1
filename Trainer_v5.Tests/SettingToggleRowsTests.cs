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
		private static readonly string[] Explicit = { "UnlimitedSubsidiaries", "ForceLights", "ShowRoomCeilings", "Experimental" };

		// Persisted but with no Settings window toggle today; a gap to close, not a rule.
		private static readonly string[] WithoutToggle = { "DisableSkillDecay", "DisableBurglars", "ReduceBoxPrice", "AutoMaxMarketShare" };

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

		[Fact]
		public void RowsStayInTheFirstFourColumnsAndHaveAUniqueLocalizationKey()
		{
			var rows = SettingToggleRows.Create();

			Assert.All(rows, row => Assert.InRange(row.Column, 2, 4));
			Assert.Equal(rows.Count, Enumerable.Count(Enumerable.Distinct(Enumerable.Select(rows, row => row.LocKey))));
			Assert.Equal(new[] { 11, 14, 21 }, Enumerable.ToArray(Enumerable.Select(new[] { 2, 3, 4 }, column => Enumerable.Count(rows, row => row.Column == column))));
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
			foreach (var row in SettingToggleRows.Create())
			{
				string text;
				if (english.TryGetValue(row.LocKey, out text) && text != row.DefaultText)
				{
					wrong.Add(row.LocKey + ": \"" + row.DefaultText + "\" vs \"" + text + "\"");
				}
			}

			Assert.True(wrong.Count == 0, string.Join("; ", wrong));
		}
	}
}
