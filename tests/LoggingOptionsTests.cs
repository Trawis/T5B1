using System.Collections.Generic;
using System.IO;
using System.Text;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	[Collection(SettingsCollection.Name)]
	public class LoggingOptionsTests
	{
		private const string DebugCall = ".LogDebug(";

		private static string[] SourceFiles()
		{
			return Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories);
		}

		[Fact]
		public void ItIsOffWhenNothingIsStoredOrTheStoredValueIsNotTrue()
		{
			foreach (var stored in new[] { null, "", "False", "0", "yes", "garbage", "Truee" })
			{
				var settings = new Dictionary<string, string>();
				settings[LoggingOptions.StorageKey] = stored;
				LoggingOptions.Load(settings);
				Assert.False(LoggingOptions.DebugEnabled);
			}

			LoggingOptions.Load(new Dictionary<string, string>());
			Assert.False(LoggingOptions.DebugEnabled);
			LoggingOptions.Load(null);
			Assert.False(LoggingOptions.DebugEnabled);
		}

		[Fact]
		public void ItIsOnOnlyWhenTheStoredValueIsTrue()
		{
			var settings = new Dictionary<string, string>();
			settings[LoggingOptions.StorageKey] = "True";
			LoggingOptions.Load(settings);
			Assert.True(LoggingOptions.DebugEnabled);

			LoggingOptions.Load(new Dictionary<string, string>());
			Assert.False(LoggingOptions.DebugEnabled);
		}

		[Fact]
		public void EveryDebugLineIsGuardedByTheOptionAtItsCallSite()
		{
			var unguarded = new List<string>();
			var calls = 0;
			foreach (var file in SourceFiles())
			{
				var name = Path.GetFileName(file);
				if (name == "Logger.cs")
				{
					continue;
				}

				var source = SourceCode.Normalize(File.ReadAllText(file));
				var headers = new List<string>();
				var header = new StringBuilder();
				for (var i = 0; i < source.Length; i++)
				{
					var c = source[i];
					if (c == '{')
					{
						headers.Add(header.ToString());
						header.Length = 0;
					}
					else if (c == '}')
					{
						if (headers.Count > 0)
						{
							headers.RemoveAt(headers.Count - 1);
						}

						header.Length = 0;
					}
					else if (c == ';')
					{
						header.Length = 0;
					}
					else
					{
						header.Append(c);
					}

					var start = i + 1 - DebugCall.Length;
					if (c == '(' && start >= 0 && string.CompareOrdinal(source, start, DebugCall, 0, DebugCall.Length) == 0)
					{
						calls++;
						var guarded = false;
						foreach (var enclosing in headers)
						{
							guarded |= enclosing.Contains("if (LoggingOptions.DebugEnabled");
						}

						if (!guarded)
						{
							unguarded.Add(name);
						}
					}
				}
			}

			Assert.True(calls >= 8, "The expected trace sites are missing: " + calls);
			Assert.True(unguarded.Count == 0, "Every .LogDebug( call must sit inside if (LoggingOptions.DebugEnabled...): " + string.Join(", ", unguarded));
		}

		[Fact]
		public void TheTraceDoesNotUseTheUnityLogBecauseOfThePlayerLogLimits()
		{
			CompiledCalls.OnlyIn("call Debug.Log");
			CompiledCalls.OnlyIn("call Debug.LogWarning");
			CompiledCalls.OnlyIn("call Debug.LogError");
		}
	}
}
