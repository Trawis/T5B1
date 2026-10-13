using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	[CollectionDefinition(Name)]
	public sealed class SettingsCollection
	{
		public const string Name = "Helpers.Settings";
	}

	public sealed class SettingsSnapshot : System.IDisposable
	{
		private readonly Dictionary<string, bool> _settings = new Dictionary<string, bool>(Helpers.Settings);
		private readonly Dictionary<string, object> _stores = new Dictionary<string, object>(Helpers.StoresSettings);

		public void Dispose()
		{
			foreach (var pair in _settings)
			{
				Helpers.Settings[pair.Key] = pair.Value;
			}

			foreach (var pair in _stores)
			{
				Helpers.StoresSettings[pair.Key] = pair.Value;
			}
		}
	}
}
