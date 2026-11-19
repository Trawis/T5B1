using System;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Refresh() sets Toggle.isOn on every trait, which raises the callback; the callback must ignore those events.
	public class TraitWindowRefreshGuardTests
	{
		[Fact]
		public void ToggleTraitReturnsBeforeTouchingTheEmployeeWhileRefreshing()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeTraitChangeWindow.cs"));
			var start = source.IndexOf("private void ToggleTrait(", StringComparison.Ordinal);

			Assert.True(start >= 0);
			var guard = source.IndexOf("if (_isRefreshing)", start, StringComparison.Ordinal);
			var write = source.IndexOf("employee.Traits ^= trait", start, StringComparison.Ordinal);
			Assert.True(guard > start);
			Assert.True(guard < write);
		}
	}
}
