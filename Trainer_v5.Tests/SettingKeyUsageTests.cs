using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Settings are referenced through SettingKeys so a misspelled key fails to compile. A raw key
	// string would bypass that and only show up in-game as a logged warning and a dead feature.
	// LINQ is avoided here: the game assembly's global Utilities class defines extension methods
	// that can win overload resolution and fail to initialize outside Unity.
	public class SettingKeyUsageTests
	{
		private static readonly Regex CommentLine = new Regex("^[ \\t]*//.*$", RegexOptions.Multiline | RegexOptions.Compiled);

		private static readonly Regex ToggleKeyUsage = new Regex(
			"(?:settings\\.(?:Get|Toggle)|Helpers\\.GetProperty\\((?:TrainerSettings|settings|Helpers\\.Settings)|ToggleJustEnabled|GetToggleTransition|Helpers\\.SetProperty\\((?:TrainerSettings|Helpers\\.Settings))\\(?[ ,]*\"(?<key>\\w+)\"",
			RegexOptions.Compiled);

		private static readonly Regex StoreKeyUsage = new Regex(
			"(?:storesSettings\\.(?:Get|Set)|Helpers\\.GetProperty\\(Helpers\\.StoresSettings,)\\(?\\s*\"(?<key>\\w+)\"",
			RegexOptions.Compiled);

		private static List<KeyValuePair<string, string>> FindKeys(Regex usage)
		{
			var found = new List<KeyValuePair<string, string>>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var text = CommentLine.Replace(File.ReadAllText(file), string.Empty);
				foreach (Match match in usage.Matches(text))
				{
					found.Add(new KeyValuePair<string, string>(Path.GetFileName(file), match.Groups["key"].Value));
				}
			}
			return found;
		}

		[Fact]
		public void TheScanFindsRawKeyUsagesWhenPresent()
		{
			Assert.Single(ToggleKeyUsage.Matches("settings.Get(\"NoStress\")"));
			Assert.Single(ToggleKeyUsage.Matches("GetToggleTransition(\"FreePrint\")"));
			Assert.Single(StoreKeyUsage.Matches("storesSettings.Set(\"EfficiencyStore\", 1)"));
		}

		[Fact]
		public void SourceUsesSettingKeysInsteadOfRawKeyStrings()
		{
			var rawUsages = new List<string>();
			foreach (var found in FindKeys(ToggleKeyUsage))
			{
				rawUsages.Add(found.Key + ": " + found.Value);
			}

			foreach (var found in FindKeys(StoreKeyUsage))
			{
				rawUsages.Add(found.Key + ": " + found.Value);
			}

			Assert.Empty(rawUsages);
		}
	}
}
