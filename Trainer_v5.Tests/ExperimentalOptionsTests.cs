using System.Collections.Generic;
using System.IO;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	// The Experimental opt-in lives in the native mod Options and the mod's own game-managed settings, not in saves (#245).
	[Collection(SettingsCollection.Name)]
	public class ExperimentalOptionsTests
	{
		[Fact]
		public void ItIsOffWhenNothingIsStoredOrTheStoredValueIsNotTrue()
		{
			foreach (var stored in new[] { null, "", "False", "0", "yes", "garbage", "Truee" })
			{
				var settings = new Dictionary<string, string>();
				settings[ExperimentalOptions.StorageKey] = stored;
				ExperimentalOptions.Load(settings);
				Assert.False(ExperimentalOptions.Enabled);
			}

			ExperimentalOptions.Load(new Dictionary<string, string>());
			Assert.False(ExperimentalOptions.Enabled);
			ExperimentalOptions.Load(null);
			Assert.False(ExperimentalOptions.Enabled);
		}

		[Fact]
		public void ItIsOnOnlyWhenTheStoredValueIsTrue()
		{
			var settings = new Dictionary<string, string>();
			settings[ExperimentalOptions.StorageKey] = "True";
			ExperimentalOptions.Load(settings);
			Assert.True(ExperimentalOptions.Enabled);

			ExperimentalOptions.Load(new Dictionary<string, string>());
			Assert.False(ExperimentalOptions.Enabled);
		}

		[Fact]
		public void SettingItStoresTheValueUnderTheOptionKey()
		{
			var saved = new List<string>();
			ExperimentalOptions.Set(true, (key, value) => saved.Add(key + "=" + value));
			Assert.True(ExperimentalOptions.Enabled);
			ExperimentalOptions.Set(false, (key, value) => saved.Add(key + "=" + value));
			Assert.False(ExperimentalOptions.Enabled);

			Assert.Equal(new[] { "EnableExperimentalFeatures=True", "EnableExperimentalFeatures=False" }, saved);
			Assert.True(ExperimentalOptions.Parse("True"));
			Assert.False(ExperimentalOptions.Parse("False"));
		}

		[Fact]
		public void ALegacySavedTrueCannotEnableIt()
		{
			using (new SettingsSnapshot())
			{
				ExperimentalOptions.Load(new Dictionary<string, string>());
				var legacy = new WriteDictionary();
				legacy["Experimental"] = true;
				new Main().Deserialize(legacy, GameReader.LoadMode.Full);

				Assert.DoesNotContain("Experimental", Helpers.Settings.Keys);
				Assert.False(ExperimentalOptions.Enabled);
			}
		}

		[Fact]
		public void OnlyTheKeyDeclarationMentionsTheLegacySettingKey()
		{
			var mentions = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				if (Path.GetFileName(file) != "SettingKeys.cs" && SourceCode.Normalize(File.ReadAllText(file)).Contains("SettingKeys.Experimental"))
				{
					mentions.Add(Path.GetFileName(file));
				}
			}

			Assert.True(mentions.Count == 0, "The legacy key is inert; nothing may read it: " + string.Join(", ", mentions));
		}

		[Fact]
		public void TheTrainerSettingsWindowHasNoExperimentalToggleOrHint()
		{
			var source = SourceCode.Read("SettingsWindow.cs");

			Assert.DoesNotContain("ExperimentalHint", source);
			Assert.DoesNotContain("SetTestControlsActive", source);
		}

		[Fact]
		public void TheNativeOptionsScreenOffersTheToggleAndSaysNothingIsAvailable()
		{
			var source = SourceCode.Read("Main.cs");
			var options = SourceCode.Body(source, "public override void ConstructOptionsScreen(");

			Assert.Contains("\"EnableExperimentalFeatures\".LocDef(\"Enable Experimental Features\")", options);
			Assert.Contains("ExperimentalOptions.Enabled", options);
			Assert.Contains("\"ExperimentalNone\".LocDef(", options);
			Assert.Contains("WindowManager.AddElementToElement(toggle.gameObject, parent.gameObject", options);
		}

		[Fact]
		public void TheOptionIsReadWhenTheModLoadsAndSavedThroughTheGamesModSettings()
		{
			var source = SourceCode.Read("Main.cs");
			var initialize = SourceCode.Body(source, "public override void Initialize(");
			var change = SourceCode.Body(source, "private void SetExperimentalEnabled(");

			Assert.Contains("ExperimentalOptions.Load(parentMod.Settings);", initialize);
			Assert.Contains("ExperimentalOptions.Set(enabled, _mod.SaveSetting);", change);
		}

		[Fact]
		public void TheOptionIsNotPartOfTheSavedGame()
		{
			var source = SourceCode.Read("Main.cs");

			Assert.DoesNotContain("ExperimentalOptions", SourceCode.Body(source, "public override WriteDictionary Serialize("));
			Assert.DoesNotContain("ExperimentalOptions", SourceCode.Body(source, "public override void Deserialize("));
			Assert.DoesNotContain("ExperimentalOptions", SourceCode.Read("TrainerBehaviour.cs"));
		}

		[Fact]
		public void ExperimentalCodeIsNotRegisteredAsAStableFeature()
		{
			Assert.DoesNotContain("Experimental", SourceCode.Read("FeatureRegistry.cs"));
			Assert.DoesNotContain("Experimental", SourceCode.Read("SettingToggleRows.cs"));
			Assert.Equal("Trainer_v5.Experimental", typeof(ExperimentalOptions).Namespace);
		}
	}
}
