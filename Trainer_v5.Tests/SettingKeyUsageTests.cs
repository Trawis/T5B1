using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// A misspelled setting key compiles fine and only shows up in-game as a logged warning and a
	// permanently disabled feature, so literal keys in the source are checked against the registry.
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

		private static List<string> UnregisteredUsages<T>(Regex usage, Dictionary<string, T> registry)
		{
			var problems = new List<string>();
			foreach (var found in FindKeys(usage))
			{
				if (!registry.ContainsKey(found.Value))
				{
					problems.Add(found.Key + ": " + found.Value);
				}
			}
			return problems;
		}

		[Fact]
		public void TheSourceScanFindsSettingUsages()
		{
			Assert.True(FindKeys(ToggleKeyUsage).Count > 50);
			Assert.True(FindKeys(StoreKeyUsage).Count > 0);
		}

		[Fact]
		public void EveryToggleKeyUsedInSourceIsRegistered()
		{
			Assert.Empty(UnregisteredUsages(ToggleKeyUsage, Helpers.Settings));
		}

		[Fact]
		public void EveryStoreKeyUsedInSourceIsRegistered()
		{
			Assert.Empty(UnregisteredUsages(StoreKeyUsage, Helpers.StoresSettings));
		}
	}
}
