using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	[Collection(SettingsCollection.Name)]
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
	}
}
