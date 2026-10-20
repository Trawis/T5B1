using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// English is the reference locale: every key must exist there once; other languages may be incomplete (#264).
	public class LocalizationTests
	{
		private static readonly Regex Entry = new Regex(@"^(?<key>\w+)\s+""", RegexOptions.Compiled | RegexOptions.Multiline);
		private static readonly Regex UsedKey = new Regex(@"""(?<key>\w+)""\s*\.LocDef\(|CreateSettingToggle\(\s*\w+,\s*""(?<key>\w+)""", RegexOptions.Compiled);

		private static readonly Regex BareText = new Regex(@"(?:SpawnDialog|AddPopupMessage|Notification\.Popup|Notification\.ShowInfo|Notification\.ShowWarning|ShowError|CreateLabel|SpawnInputDialog|SpawnWindow|RequestInt|RequestUInt|RequestFloat|UIFactory\.Label|UIFactory\.Button)\(\s*""(?<text>(?:[^""\\]|\\.)+)""\s*[,)]", RegexOptions.Compiled);

		private static readonly Regex Formatted = new Regex(@"string\.Format\(\s*""(?<key>\w+)""\s*\.LocDef\(", RegexOptions.Compiled);

		private static readonly Regex Placeholder = new Regex(@"\{(?<index>\d+)\}", RegexOptions.Compiled);

		private static readonly Regex Fallback = new Regex(@"""(?<key>\w+)""\s*\.LocDef\(\s*""(?<text>[^""]*)""\s*\)", RegexOptions.Compiled);

		// Buttons and toggles of the Settings window tables, then labels, dialogs (information, confirmations, prompts), errors and HUD popups.
		private static readonly string[] EnglishSections = { "#Input fields", "#Actions", "#Toggles", "#Labels", "#Dialogs", "#Errors", "#Notifications" };

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

		[Fact]
		public void EveryLocalizationFileEndsWithANewline()
		{
			var files = new List<string>(Files());
			files.Add(Path.Combine(LocalizationDirectory, "credits.txt"));
			var unterminated = new List<string>();
			foreach (var file in files)
			{
				var text = File.ReadAllText(file);
				if (!text.EndsWith("\n"))
				{
					unterminated.Add(Path.GetFileName(Path.GetDirectoryName(file)) + "/" + Path.GetFileName(file));
				}
			}

			Assert.True(unterminated.Count == 0, "Missing final newline: " + string.Join(", ", unterminated));
		}

		[Fact]
		public void CorrectedTranslationsKeepTheirMeaning()
		{
			Assert.Equal("Brände deaktivieren", Value("German", "DisableFires"));
			Assert.NotEqual(Value("English", "MaxReputation"), Value("German", "MaxReputation"));
			Assert.NotEqual(Value("English", "FixBugs"), Value("German", "FixBugs"));
			Assert.Equal("Adquirir compañía", Value("Spanish", "TakeoverCompany"));
			Assert.Equal("Maks. reputacija", Value("Croatian", "MaxReputation"));
			Assert.Equal("Osnuj tvrtku kćer", Value("Croatian", "SubsidiaryCompany"));
		}

		[Fact]
		public void CodeFallbackTextMatchesTheEnglishFile()
		{
			var mismatches = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				foreach (Match match in Fallback.Matches(SourceText.Strip(File.ReadAllText(file), keepStrings: true)))
				{
					var expected = Value("English", match.Groups["key"].Value);
					if (expected != null && expected != match.Groups["text"].Value)
					{
						mismatches.Add(match.Groups["key"].Value + ": '" + match.Groups["text"].Value + "' vs '" + expected + "'");
					}
				}
			}

			Assert.True(mismatches.Count == 0, "Code fallback differs from English file: " + string.Join("; ", mismatches));
		}

		[Fact]
		public void ActiveMessagesAndLabelsGoThroughLocDef()
		{
			var bare = new SortedSet<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				foreach (Match match in BareText.Matches(SourceText.Strip(File.ReadAllText(file), keepStrings: true)))
				{
					bare.Add(Path.GetFileName(file) + ": " + match.Groups["text"].Value);
				}
			}

			Assert.True(bare.Count == 0, "Use \"Key\".LocDef(\"text\") and add the key to English/Trainer.tyd: " + string.Join("; ", bare));
		}

		[Fact]
		public void MessagesWithPlaceholdersAreFormattedByTheCode()
		{
			// A numbered placeholder in the English text is only filled in when the code passes the looked-up text to string.Format.
			var formatted = new SortedSet<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				foreach (Match match in Formatted.Matches(SourceText.Strip(File.ReadAllText(file), keepStrings: true)))
				{
					formatted.Add(match.Groups["key"].Value);
				}
			}

			var withPlaceholder = new SortedSet<string>();
			foreach (var key in Keys(File.ReadAllText(EnglishFile())))
			{
				var text = Value("English", key);
				var indexes = new SortedSet<int>();
				foreach (Match match in Placeholder.Matches(text))
				{
					indexes.Add(int.Parse(match.Groups["index"].Value));
				}

				if (indexes.Count == 0)
				{
					Assert.True(text.IndexOf('{') < 0 && text.IndexOf('}') < 0, key + " has a brace that is not a numbered placeholder");
					continue;
				}

				withPlaceholder.Add(key);
				var args = new object[Enumerable.Max(indexes) + 1];
				for (var i = 0; i < args.Length; i++)
				{
					args[i] = "<" + i + ">";
				}

				Assert.True(args.Length == indexes.Count, key + " skips a placeholder number");
				var result = string.Format(text, args);
				Assert.True(result.IndexOf('{') < 0, key + " does not format cleanly");
			}

			Assert.Equal(string.Join(", ", withPlaceholder), string.Join(", ", formatted));
		}

		[Fact]
		public void EnglishHasTheAgreedSectionsInOrderAndEveryEntryIsUnderOne()
		{
			var headers = new List<string>();
			var entryBeforeHeader = new List<string>();
			SectionHeaders(File.ReadAllText(EnglishFile()), headers, entryBeforeHeader);

			Assert.True(entryBeforeHeader.Count == 0, "Entries before the first header: " + string.Join(", ", entryBeforeHeader));
			Assert.Equal(string.Join(", ", EnglishSections), string.Join(", ", headers));
		}

		[Fact]
		public void SectionParsingGivesTheSameResultForLfAndCrlfFiles()
		{
			var lf = File.ReadAllText(EnglishFile()).Replace("\r\n", "\n");
			var crlf = lf.Replace("\n", "\r\n");
			var lfHeaders = new List<string>();
			var crlfHeaders = new List<string>();
			SectionHeaders(lf, lfHeaders, new List<string>());
			SectionHeaders(crlf, crlfHeaders, new List<string>());

			Assert.Equal(string.Join(", ", EnglishSections), string.Join(", ", lfHeaders));
			Assert.Equal(string.Join(", ", lfHeaders), string.Join(", ", crlfHeaders));

			var lfSections = EnglishSectionOfEveryKey(lf);
			var crlfSections = EnglishSectionOfEveryKey(crlf);
			Assert.Equal(lfSections.Count, crlfSections.Count);
			foreach (var pair in lfSections)
			{
				Assert.Equal(pair.Value, crlfSections[pair.Key]);
			}
		}

		[Fact]
		public void EnglishToggleRowsAreUnderTogglesAndActionRowsUnderActions()
		{
			var sectionOf = EnglishSectionOfEveryKey(File.ReadAllText(EnglishFile()));
			var misplaced = new List<string>();
			var toggleRows = SettingToggleRows.Create();
			toggleRows.AddRange(SettingToggleRows.CreateImmediate());
			foreach (var row in toggleRows)
			{
				if (!sectionOf.ContainsKey(row.LocKey) || sectionOf[row.LocKey] != "#Toggles")
				{
					misplaced.Add(row.LocKey + " (toggle)");
				}
			}

			foreach (var row in SettingActionRows.Create())
			{
				if (!sectionOf.ContainsKey(row.LocKey) || sectionOf[row.LocKey] != "#Actions")
				{
					misplaced.Add(row.LocKey + " (action)");
				}
			}

			Assert.True(misplaced.Count == 0, "Move to #Toggles or #Actions in English/Trainer.tyd: " + string.Join(", ", misplaced));
		}

		[Fact]
		public void NoKeyIsLookedUpInMoreThanOnePlace()
		{
			var places = new Dictionary<string, List<string>>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				foreach (Match match in UsedKey.Matches(SourceText.Strip(File.ReadAllText(file), keepStrings: true)))
				{
					var key = match.Groups["key"].Value;
					if (!places.ContainsKey(key))
					{
						places[key] = new List<string>();
					}

					places[key].Add(Path.GetFileName(file));
				}
			}

			var repeated = new List<string>();
			foreach (var pair in places)
			{
				if (pair.Value.Count > 1)
				{
					repeated.Add(pair.Key + " (" + string.Join(", ", pair.Value) + ")");
				}
			}

			Assert.True(repeated.Count == 0, "Looked up in more than one place; move each to SharedMessages: " + string.Join("; ", repeated));
		}

		[Fact]
		public void EverySharedMessageKeepsItsKeyAndFallbackAsOneLiteralPair()
		{
			var source = SourceText.Strip(File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "SharedMessages.cs")), keepStrings: true);
			var methods = Regex.Matches(source, @"public static string (?<name>\w+)\(");
			var pairs = new List<string>();
			foreach (Match match in Fallback.Matches(source))
			{
				pairs.Add(match.Groups["key"].Value);
			}

			Assert.True(methods.Count > 0);
			Assert.Equal(string.Join(", ", Enumerable.Select(methods.Cast<Match>(), method => method.Groups["name"].Value)), string.Join(", ", pairs));
		}

		[Fact]
		public void MessageKeysAreNotFakedInOtherLanguages()
		{
			// Other languages fall back to English through LocDef until a native speaker translates a key.
			foreach (var key in new[] { "EfficiencyLabel", "LeadEfficiencyLabel", "SelectEmployeesError", "NoActiveResearch", "ResearchCompleted" })
			{
				var english = Value("English", key);
				Assert.NotNull(english);
				foreach (var file in Files())
				{
					if (Language(file) == "English")
					{
						continue;
					}

					var translated = Value(Language(file), key);
					Assert.True(translated == null || translated != english, Language(file) + " copies the English text for " + key);
				}
			}
		}

		private static string[] Lines(string text)
		{
			return text.Replace("\r\n", "\n").Split('\n');
		}

		private static void SectionHeaders(string text, List<string> headers, List<string> entryBeforeHeader)
		{
			foreach (var line in Lines(text))
			{
				if (line.StartsWith("#"))
				{
					headers.Add(line);
				}
				else if (Entry.IsMatch(line) && headers.Count == 0)
				{
					entryBeforeHeader.Add(line);
				}
			}
		}

		private static Dictionary<string, string> EnglishSectionOfEveryKey(string text)
		{
			var sectionOf = new Dictionary<string, string>();
			var section = "";
			foreach (var line in Lines(text))
			{
				if (line.StartsWith("#"))
				{
					section = line;
				}
				else
				{
					var match = Entry.Match(line);
					if (match.Success)
					{
						sectionOf[match.Groups["key"].Value] = section;
					}
				}
			}

			return sectionOf;
		}

		private static string Value(string language, string key)
		{
			var text = File.ReadAllText(Path.Combine(LocalizationDirectory, language, "Trainer.tyd"));
			var match = Regex.Match(text, @"^" + key + @"\s+""(?<text>[^""]*)""", RegexOptions.Multiline);
			return match.Success ? match.Groups["text"].Value : null;
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
