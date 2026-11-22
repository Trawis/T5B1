using System.Collections.Generic;
using System.IO;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
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
	}
}
