using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Settings go through SettingKeys so a misspelled key fails to compile. LINQ is avoided: the game's global Utilities
	// extensions can win overload resolution.
	public class SettingKeyUsageTests
	{
		private static readonly Regex ToggleKeyUsage = new Regex(
			"(?:settings\\s*\\.\\s*(?:Get|Toggle)|Helpers\\.GetProperty\\((?:TrainerSettings|settings|Helpers\\.Settings)|ToggleJustEnabled|GetToggleTransition|Helpers\\.SetProperty\\((?:TrainerSettings|Helpers\\.Settings))\\(?[\\s,]*\"(?<key>\\w+)\"",
			RegexOptions.Compiled);

		private static readonly Regex StoreKeyUsage = new Regex(
			"(?:storesSettings\\s*\\.\\s*(?:Get|Set)|Helpers\\.GetProperty\\(Helpers\\.StoresSettings,)\\(?\\s*\"(?<key>\\w+)\"",
			RegexOptions.Compiled);

		private static List<KeyValuePair<string, string>> FindKeys(Regex usage)
		{
			var found = new List<KeyValuePair<string, string>>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var text = SourceText.Strip(File.ReadAllText(file), keepStrings: true);
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
			Assert.Single(ToggleKeyUsage.Matches("settings\n\t.Get(\n\t\"NoStress\")"));
			Assert.Single(ToggleKeyUsage.Matches(SourceText.Strip("var x = 1; /* c */ settings.Get(\"NoStress\"); // t", keepStrings: true)));
		}

		[Fact]
		public void TheScanIgnoresKeysThatAreOnlyMentionedInComments()
		{
			Assert.Empty(ToggleKeyUsage.Matches(SourceText.Strip("// settings.Get(\"NoStress\")\n/* settings.Get(\"NoStress\") */ settings.Get(SettingKeys.NoStress);", keepStrings: true)));
			Assert.Empty(StoreKeyUsage.Matches(SourceText.Strip("var s = 1; // storesSettings.Get(\"EfficiencyStore\")", keepStrings: true)));
		}

		[Fact]
		public void TheScanStillFindsRawKeysNextToInterpolatedStrings()
		{
			var code = SourceText.Strip("var m = $\"{name}\"; settings.Get(\"NoStress\"); var n = $\"{settings.Get(\"FreePrint\")}\";", keepStrings: true);
			Assert.Equal(2, ToggleKeyUsage.Matches(code).Count);
		}

		// Main.Serialize/Deserialize iterate the persisted key strings, so they are the only code that may hand a string
		// key to the settings accessors. Everything else passes a ToggleKey/StoreKey.
		private static readonly Regex PropertyCall = new Regex(
			"Helpers\\.(?:Get|Set)Property\\(\\s*[\\w.]+\\s*,\\s*(?<key>[^,)]+)",
			RegexOptions.Compiled);

		private static readonly Regex TypedKeyArgument = new Regex("^(?:SettingKeys\\.\\w+|settingKey|key)$", RegexOptions.Compiled);

		private static readonly Regex RawIndexer = new Regex(
			"(?:Helpers\\.)?(?:Settings|StoresSettings|TrainerSettings|settings|storesSettings)\\s*\\[\\s*\"",
			RegexOptions.Compiled);

		[Fact]
		public void OnlySerializationPassesStringKeysToThePropertyAccessors()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(file);
				if (name == "Main.cs" || name == "Helpers.cs")
				{
					continue;
				}

				var text = SourceText.Strip(File.ReadAllText(file), keepStrings: true);
				foreach (Match match in PropertyCall.Matches(text))
				{
					var key = match.Groups["key"].Value.Trim();
					if (!TypedKeyArgument.IsMatch(key))
					{
						offenders.Add(name + ": " + key);
					}
				}
			}

			Assert.Empty(offenders);
		}

		[Fact]
		public void SourceNeverIndexesTheSettingsDictionariesWithAStringLiteral()
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var text = SourceText.Strip(File.ReadAllText(file), keepStrings: true);
				foreach (Match match in RawIndexer.Matches(text))
				{
					offenders.Add(Path.GetFileName(file) + ": " + match.Value);
				}
			}

			Assert.Empty(offenders);
		}

		[Fact]
		public void TheStringKeyScansRecognizeRawAndTypedCalls()
		{
			Assert.Equal("\"NoStress\"", PropertyCall.Match("Helpers.GetProperty(Helpers.Settings, \"NoStress\")").Groups["key"].Value);
			Assert.Matches(TypedKeyArgument, PropertyCall.Match("Helpers.GetProperty(TrainerSettings, SettingKeys.NoStress)").Groups["key"].Value);
			Assert.Single(RawIndexer.Matches("Helpers.Settings[\"NoStress\"] = true;"));
			Assert.Empty(RawIndexer.Matches("Helpers.Settings[key.Key] = true;"));
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
