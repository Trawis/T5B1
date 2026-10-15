using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	// Missing object-store values fall back predictably instead of mapping to a wrong option (#267).
	public class StoreLookupTests
	{
		[Fact]
		public void AMissingStoreSelectsTheFirstOptionForEveryValueType()
		{
			var items = new Dictionary<string, object>();
			var settings = new Dictionary<string, object>();

			foreach (var valueType in new[] { 1, 2, 3, 4 })
			{
				Assert.Equal(0, items.GetIndex(settings, "NotAKey", valueType));
			}
		}

		[Fact]
		public void AnUnknownValueTypeIsRejected()
		{
			var items = new Dictionary<string, object>();
			var settings = new Dictionary<string, object> { { "Present", 1f } };

			Assert.Equal(-1, items.GetIndex(settings, "Present", 99));
		}

		[Fact]
		public void AnUnsetEfficiencyStoreIsNullAndOnlyStoreGatedRowsReadIt()
		{
			// null means "not set": the dispatcher skips the row, so MakeFloat is never asked to turn it into 0.
			Assert.Null(Helpers.GetProperty(Helpers.StoresSettings, SettingKeys.EfficiencyStore));
			Assert.Null(Helpers.GetProperty(Helpers.StoresSettings, SettingKeys.LeadEfficiencyStore));

			var gated = 0;
			foreach (var entry in FeatureRegistry.Create())
			{
				if (entry.Store != null)
				{
					gated++;
					Assert.Null(entry.Toggle);
				}
			}
			Assert.Equal(2, gated);
		}
	}
}
