using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// English is the reference locale: every key must exist there once; other languages may be incomplete.
	public class LocalizationTests
	{
		private static readonly Regex Entry = new Regex(@"^(?<key>\w+)\s+""", RegexOptions.Compiled | RegexOptions.Multiline);

		private static readonly Regex Placeholder = new Regex(@"\{(?<index>\d+)\}", RegexOptions.Compiled);

		private static readonly string[] EnglishSections = { "#Input fields", "#Actions", "#Toggles", "#Labels", "#Dialogs", "#Errors", "#Notifications" };

		private static string LocalizationDirectory => Path.Combine(RepositoryFiles.Root, "assets", "Localization");

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
			foreach (var pair in LocDefPairs())
			{
				if (!english.Contains(pair.Key))
				{
					missing.Add(pair.Key);
				}
			}

			Assert.True(missing.Count == 0, "Add these to English/Trainer.tyd (use the in-code text): " + string.Join(", ", missing));
		}

		[Fact]
		public void CodeFallbackTextMatchesTheEnglishFile()
		{
			var mismatches = new List<string>();
			foreach (var pair in LocDefPairs())
			{
				var expected = Value("English", pair.Key);
				var text = Escaped(pair.Value);
				if (expected != null && expected != text)
				{
					mismatches.Add(pair.Key + ": '" + text + "' vs '" + expected + "'");
				}
			}

			Assert.True(mismatches.Count == 0, "Code fallback differs from English file: " + string.Join("; ", mismatches));
		}

		// A literal whose next call shows text to the player must be looked up through LocDef instead. A literal that is a
		// single identifier ("Cogs", "TrainerButton") is an icon or object name, not text the player reads.
		[Fact]
		public void ActiveMessagesAndLabelsGoThroughLocDef()
		{
			var bare = new SortedSet<string>();
			foreach (var method in CompiledCalls.AllMethods())
			{
				var steps = method.Value;
				for (var i = 0; i < steps.Count; i++)
				{
					if (!steps[i].StartsWith("ldstr ", StringComparison.Ordinal) || !Regex.IsMatch(steps[i].Substring(6), @"\W") || steps[i].Substring(6).Trim().Length == 0)
					{
						continue;
					}

					var next = i + 1;
					while (next < steps.Count && !steps[next].StartsWith("call ", StringComparison.Ordinal))
					{
						next++;
					}

					if (next < steps.Count && Array.IndexOf(DisplayCalls, steps[next]) >= 0)
					{
						bare.Add(method.Key + ": " + steps[i].Substring(6));
					}
				}
			}

			Assert.True(bare.Count == 0, "Use \"Key\".LocDef(\"text\") and add the key to English/Trainer.tyd: " + string.Join("; ", bare));
		}

		[Fact]
		public void MessagesWithPlaceholdersAreFormattedByTheCode()
		{
			// A numbered placeholder in the English text is only filled in when the code passes the looked-up text to
			// string.Format: a key counts as formatted when string.Format follows its lookup in the same method.
			var formatted = new SortedSet<string>();
			foreach (var method in CompiledCalls.AllMethods())
			{
				var steps = method.Value;
				for (var i = 0; i + 2 < steps.Count; i++)
				{
					if (IsLocDefAt(steps, i) && Placeholder.IsMatch(steps[i + 1]) && steps.IndexOf("call String.Format", i + 3) > 0)
					{
						formatted.Add(steps[i].Substring(6));
					}
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

		private static readonly string[] DisplayCalls =
		{
			"call Notification.Popup", "call Notification.ShowInfo", "call Notification.ShowWarning", "call Notification.ShowError",
			"call WindowManager.SpawnDialog", "call HUD.AddPopupMessage", "call WindowManager.SpawnInputDialog", "call WindowManager.SpawnWindow",
			"call InputHelper.RequestInt", "call InputHelper.RequestUInt", "call InputHelper.RequestFloat", "call UIFactory.Label", "call UIFactory.Button", "call UIHelper.CreateLabel",
		};

		private static bool IsLocDefAt(List<string> steps, int i)
		{
			return steps[i].StartsWith("ldstr ", StringComparison.Ordinal) && steps[i + 1].StartsWith("ldstr ", StringComparison.Ordinal) && steps[i + 2] == "call Localization.LocDef";
		}

		// Every "Key".LocDef("text") in the compiled trainer: two string constants followed by the LocDef call.
		private static List<KeyValuePair<string, string>> LocDefPairs()
		{
			var pairs = new List<KeyValuePair<string, string>>();
			foreach (var method in CompiledCalls.AllMethods())
			{
				for (var i = 0; i + 2 < method.Value.Count; i++)
				{
					if (IsLocDefAt(method.Value, i))
					{
						pairs.Add(new KeyValuePair<string, string>(method.Value[i].Substring(6), method.Value[i + 1].Substring(6)));
					}
				}
			}

			Assert.True(pairs.Count > 100, "The LocDef lookups were not found in the compiled trainer: " + pairs.Count);
			return pairs;
		}

		// The English file keeps escapes as written; the compiled text has the characters.
		private static string Escaped(string text)
		{
			return text.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"");
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
