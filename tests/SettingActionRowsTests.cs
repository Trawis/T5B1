using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class SettingActionRowsTests
	{
		private static List<SettingActionRow> AllRows()
		{
			var rows = SettingActionRows.Create();
			rows.AddRange(SettingActionRows.CreateExperimental());
			return rows;
		}

		[Fact]
		public void EveryRowHasAUniqueKeyATextAHandlerAndAValidPlace()
		{
			var rows = AllRows();
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
		public void EveryRowDefaultTextMatchesTheEnglishFile()
		{
			var path = Path.Combine(RepositoryFiles.Root, "assets", "Localization", "English", "Trainer.tyd");
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
