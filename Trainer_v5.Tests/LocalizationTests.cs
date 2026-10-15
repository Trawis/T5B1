using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// English is the reference locale: the game falls back to it for any key another language lacks, so other
	// languages may be incomplete, but every key must exist in English and be defined once (#264).
	public class LocalizationTests
	{
		private static readonly Regex Entry = new Regex(@"^(?<key>\w+)\s+""", RegexOptions.Compiled | RegexOptions.Multiline);
		private static readonly Regex UsedKey = new Regex(@"""(?<key>\w+)""\s*\.LocDef\(|CreateSettingToggle\(\s*\w+,\s*""(?<key>\w+)""", RegexOptions.Compiled);

		private static string LocalizationDirectory => Path.Combine(RepositoryFiles.Root, "Trainer_v5", "Trainer.Localization");

		[Fact]
		public void TheScanReadsEntriesAndIgnoresComments()
		{
			var keys = Keys("#Buttons\t\nAddMoney\t\t\"Add Money\"\n  Indented\t\"x\"\nOther \"y\"\n");

			Assert.Equal(new[] { "AddMoney", "Other" }, keys);
		}

		[Fact]
		public void NoLanguageDefinesAKeyTwice()
		{
			var duplicates = new List<string>();
			foreach (var file in Files())
			{
				foreach (var group in Enumerable.GroupBy(Keys(File.ReadAllText(file)), key => key))
				{
					if (group.Count() > 1)
					{
						duplicates.Add(Language(file) + ": " + group.Key);
					}
				}
			}

			Assert.True(duplicates.Count == 0, "Duplicate keys: " + string.Join(", ", duplicates));
		}

		[Fact]
		public void EveryLanguageKeyAlsoExistsInEnglish()
		{
			var english = new HashSet<string>(Keys(File.ReadAllText(EnglishFile())));
			var orphans = new List<string>();
			foreach (var file in Files())
			{
				foreach (var key in Keys(File.ReadAllText(file)))
				{
					if (!english.Contains(key))
					{
						orphans.Add(Language(file) + ": " + key);
					}
				}
			}

			Assert.True(orphans.Count == 0, "Keys missing from English: " + string.Join(", ", orphans));
		}

		[Fact]
		public void EveryKeyTheCodeLooksUpExistsInEnglish()
		{
			var english = new HashSet<string>(Keys(File.ReadAllText(EnglishFile())));
			var missing = new SortedSet<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				foreach (Match match in UsedKey.Matches(SourceText.Strip(File.ReadAllText(file), keepStrings: true)))
				{
					if (!english.Contains(match.Groups["key"].Value))
					{
						missing.Add(match.Groups["key"].Value);
					}
				}
			}

			Assert.True(missing.Count == 0, "Add these to English/Trainer.tyd (use the in-code text): " + string.Join(", ", missing));
		}

		private static IEnumerable<string> Files()
		{
			return Directory.GetFiles(LocalizationDirectory, "Trainer.tyd", SearchOption.AllDirectories);
		}

		private static string EnglishFile()
		{
			return Path.Combine(LocalizationDirectory, "English", "Trainer.tyd");
		}

		private static string Language(string file)
		{
			return Path.GetFileName(Path.GetDirectoryName(file));
		}

		private static List<string> Keys(string text)
		{
			return Enumerable.Select(Entry.Matches(text).Cast<Match>(), match => match.Groups["key"].Value).ToList();
		}
	}
}
