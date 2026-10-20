using System.Collections.Generic;
using System.IO;
using System.Text;
using Trainer_v5.Experimental;
using Xunit;

namespace Trainer_v5.Tests
{
	// Debug logging is an opt-in of the native mod Options, stored like the Experimental opt-in and not in saves (#278).
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
		public void SettingItStoresTheValueUnderTheOptionKey()
		{
			var saved = new List<string>();
			LoggingOptions.Set(true, (key, value) => saved.Add(key + "=" + value));
			Assert.True(LoggingOptions.DebugEnabled);
			LoggingOptions.Set(false, (key, value) => saved.Add(key + "=" + value));
			Assert.False(LoggingOptions.DebugEnabled);
			LoggingOptions.Set(true, null);
			Assert.True(LoggingOptions.DebugEnabled);
			LoggingOptions.Set(false, null);

			Assert.Equal(new[] { "DebugLogging=True", "DebugLogging=False" }, saved);
			Assert.True(LoggingOptions.Parse("True"));
			Assert.False(LoggingOptions.Parse("False"));
			Assert.False(LoggingOptions.Parse(null));
		}

		[Fact]
		public void ItIsIndependentOfTheExperimentalOption()
		{
			ExperimentalOptions.Load(null);
			LoggingOptions.Load(null);

			ExperimentalOptions.Set(true, null);
			Assert.False(LoggingOptions.DebugEnabled);
			LoggingOptions.Set(true, null);
			ExperimentalOptions.Set(false, null);
			Assert.True(LoggingOptions.DebugEnabled);
			LoggingOptions.Set(false, null);
			Assert.False(ExperimentalOptions.Enabled);
			Assert.NotEqual(ExperimentalOptions.StorageKey, LoggingOptions.StorageKey);
			Assert.Equal("Trainer_v5", typeof(LoggingOptions).Namespace);
		}

		[Fact]
		public void TheOptionIsReadWhenTheModLoadsAndSavedThroughTheGamesModSettings()
		{
			var source = SourceCode.Read("Main.cs");

			Assert.Contains("LoggingOptions.Load(parentMod.Settings);", SourceCode.Body(source, "public override void Initialize("));
			Assert.Contains("LoggingOptions.Set(enabled, _mod.SaveSetting);", SourceCode.Body(source, "private void SetDebugLoggingEnabled("));
		}

		[Fact]
		public void TheNativeOptionsScreenOffersTheToggleBetweenTheExperimentalNoteAndTheLoadHint()
		{
			var options = SourceCode.Body(SourceCode.Read("Main.cs"), "public override void ConstructOptionsScreen(");

			Assert.Contains("\"DebugLogging\".LocDef(\"Debug Logging\")", options);
			Assert.Contains("LoggingOptions.DebugEnabled", options);
			Assert.Contains("UIFactory.Toggle(", options);
			Assert.Contains("\"DebugLoggingHint\".LocDef(", options);
			SourceCode.AssertInOrder(options, "\"ExperimentalNone\".LocDef(", "\"DebugLogging\".LocDef(", "Debug Logging sits below the Experimental option");
			SourceCode.AssertInOrder(options, "\"DebugLoggingHint\".LocDef(", "\"OptionsLoadHint\".LocDef(", "the load hint stays last");
		}

		[Fact]
		public void TheOptionIsNotPartOfTheSavedGameOrTheTrainerSettings()
		{
			var source = SourceCode.Read("Main.cs");

			Assert.DoesNotContain("LoggingOptions", SourceCode.Body(source, "public override WriteDictionary Serialize("));
			Assert.DoesNotContain("LoggingOptions", SourceCode.Body(source, "public override void Deserialize("));
			Assert.DoesNotContain("LoggingOptions", SourceCode.Read("TrainerBehaviour.cs").Replace("LoggingOptions.DebugEnabled", string.Empty));
			Assert.DoesNotContain("DebugLogging", SourceCode.Read("SettingKeys.cs"));
			Assert.DoesNotContain("DebugLogging", SourceCode.Read("SettingToggleRows.cs"));
			Assert.DoesNotContain("DebugLogging", SourceCode.Read("FeatureRegistry.cs"));
			Assert.DoesNotContain("DebugLogging", SourceCode.Read("SettingsWindow.cs"));
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
		public void NormalLoggingNeverLooksAtTheOptionAndOnlyTheTraceSitesUseTheDebugSink()
		{
			var logger = SourceCode.Read("Logger.cs");
			Assert.DoesNotContain("DebugEnabled", SourceCode.Span(logger, "public static void Log(this string str", "public static void LogDebug("));
			Assert.DoesNotContain("DebugEnabled", SourceCode.Span(logger, "internal static string FormatException(", null));

			foreach (var file in SourceFiles())
			{
				var name = Path.GetFileName(file);
				var source = SourceCode.Normalize(File.ReadAllText(file));
				var isTraceSite = name == "FeatureDispatcher.cs" || name == "TrainerBehaviour.cs";
				var isOption = name == "Logger.cs" || name == "LoggingOptions.cs" || name == "Main.cs";
				if (!isTraceSite && !isOption)
				{
					Assert.DoesNotContain("DebugEnabled", source);
					Assert.DoesNotContain(DebugCall, source);
				}
			}
		}

		[Fact]
		public void TheTraceDoesNotUseTheUnityLogBecauseOfThePlayerLogLimits()
		{
			foreach (var file in SourceFiles())
			{
				var source = SourceCode.Normalize(File.ReadAllText(file));
				Assert.DoesNotContain("Debug.Log(", source);
				Assert.DoesNotContain("Debug.LogWarning(", source);
				Assert.DoesNotContain("Debug.LogError(", source);
			}
		}

		[Fact]
		public void RetriesAreTracedOnlyOnTheFirstFailureAndAtTheEnd()
		{
			var run = SourceCode.Body(SourceCode.Read("FeatureDispatcher.cs"), "private void RunPending(");

			Assert.Contains("pending.Visits == 1", run);
			Assert.Contains("retries finished", run);
		}
	}
}
