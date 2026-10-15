using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class FeatureDispatcherTests
	{
		private readonly List<string> _calls = new List<string>();
		private readonly HashSet<string> _on = new HashSet<string>();
		private readonly HashSet<string> _stores = new HashSet<string>();
		private readonly ToggleStateTracker _tracker = new ToggleStateTracker();

		[Fact]
		public void RunExecutesOnlyTheCadenceAndOnlyEnabledFeaturesInRegistrationOrder()
		{
			_on.Add(SettingKeys.NoStress.Key);
			_on.Add(SettingKeys.NoNeeds.Key);
			var dispatcher = Create(
				Entry("NoNeeds", FeatureTrigger.Hour, SettingKeys.NoNeeds),
				Entry("NoStress", FeatureTrigger.Hour, SettingKeys.NoStress),
				Entry("FullSatisfaction", FeatureTrigger.Hour, SettingKeys.FullSatisfaction),
				Entry("NoStress", FeatureTrigger.Minute, SettingKeys.NoStress));

			dispatcher.Run(FeatureTrigger.Hour);

			Assert.Equal(new[] { "NoNeeds", "NoStress" }, _calls);
		}

		[Fact]
		public void AStoreEntryRunsOnlyWhileTheStoreHasAValue()
		{
			var dispatcher = Create(new FeatureEntry("EfficiencyStore", FeatureTrigger.Frame, null, SettingKeys.EfficiencyStore, Record("EfficiencyStore")));

			dispatcher.Run(FeatureTrigger.Frame);
			_stores.Add(SettingKeys.EfficiencyStore.Key);
			dispatcher.Run(FeatureTrigger.Frame);

			Assert.Equal(new[] { "EfficiencyStore" }, _calls);
		}

		[Fact]
		public void ATransitionRunsOnceWhenTheToggleChangesAndNotWhileItStaysTheSame()
		{
			var dispatcher = Create(
				Entry("NoEducationCost", FeatureTrigger.Enabled, SettingKeys.NoEducationCost, "apply"),
				Entry("NoEducationCost", FeatureTrigger.Disabled, SettingKeys.NoEducationCost, "restore"));

			dispatcher.RunTransitions();
			_on.Add(SettingKeys.NoEducationCost.Key);
			dispatcher.RunTransitions();
			dispatcher.RunTransitions();
			_on.Remove(SettingKeys.NoEducationCost.Key);
			dispatcher.RunTransitions();
			dispatcher.RunTransitions();

			Assert.Equal(new[] { "NoEducationCost:apply", "NoEducationCost:restore" }, _calls);
		}

		[Fact]
		public void AToggleAlreadyOnAtTheFirstObservationCountsAsJustEnabled()
		{
			_on.Add(SettingKeys.NoMaintenance.Key);
			var dispatcher = Create(Entry("NoMaintenance", FeatureTrigger.Enabled, SettingKeys.NoMaintenance));

			dispatcher.RunTransitions();
			dispatcher.RunTransitions();

			Assert.Equal(new[] { "NoMaintenance" }, _calls);
		}

		[Fact]
		public void ARestoreRunsWhenTheNextSaveHasTheToggleOffAfterTheTrackerWasReset()
		{
			var dispatcher = Create(
				Entry("ReduceISPCost", FeatureTrigger.Enabled, SettingKeys.ReduceISPCost, "apply"),
				Entry("ReduceISPCost", FeatureTrigger.Disabled, SettingKeys.ReduceISPCost, "restore"));
			_on.Add(SettingKeys.ReduceISPCost.Key);
			dispatcher.RunTransitions();

			_tracker.Reset();
			_on.Remove(SettingKeys.ReduceISPCost.Key);
			dispatcher.RunTransitions();
			dispatcher.RunTransitions();

			Assert.Equal(new[] { "ReduceISPCost:apply", "ReduceISPCost:restore" }, _calls);
		}

		[Fact]
		public void TransitionsAreObservedInTheOrderTheirKeysWereRegistered()
		{
			_on.Add(SettingKeys.NoVacation.Key);
			_on.Add(SettingKeys.NoMaintenance.Key);
			var dispatcher = Create(
				Entry("NoVacation", FeatureTrigger.Enabled, SettingKeys.NoVacation),
				Entry("NoMaintenance", FeatureTrigger.Enabled, SettingKeys.NoMaintenance),
				Entry("NoVacation", FeatureTrigger.Disabled, SettingKeys.NoVacation, "restore"));

			dispatcher.RunTransitions();

			Assert.Equal(new[] { "NoVacation", "NoMaintenance" }, _calls);
		}

		[Fact]
		public void AFailingFeatureDoesNotStopTheNextOne()
		{
			_on.Add(SettingKeys.NoStress.Key);
			_on.Add(SettingKeys.NoNeeds.Key);
			var dispatcher = new FeatureDispatcher(
				new[] { Entry("NoStress", FeatureTrigger.Minute, SettingKeys.NoStress), Entry("NoNeeds", FeatureTrigger.Minute, SettingKeys.NoNeeds) },
				key => _on.Contains(key.Key),
				store => _stores.Contains(store.Key),
				_tracker,
				(id, action) =>
				{
					_calls.Add("guard:" + id);
					return false;
				});

			dispatcher.Run(FeatureTrigger.Minute);

			Assert.Equal(new[] { "guard:NoStress", "guard:NoNeeds" }, _calls);
		}

		[Fact]
		public void TheRegistryUsesEachFeatureIdWithOneSettingOnly()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "FeatureRegistry.cs"));
			var keyById = new Dictionary<string, string>();
			var conflicts = new List<string>();
			foreach (Match match in Regex.Matches(source, @"Add(?:Store)?\(entries, FeatureTrigger\.\w+, ""(\w+)"", SettingKeys\.(\w+),"))
			{
				string existing;
				if (keyById.TryGetValue(match.Groups[1].Value, out existing) && existing != match.Groups[2].Value)
				{
					conflicts.Add(match.Groups[1].Value);
				}
				keyById[match.Groups[1].Value] = match.Groups[2].Value;
			}

			Assert.True(keyById.Count > 40, "The registry scan found " + keyById.Count + " features; the scan or the registry changed.");
			Assert.True(conflicts.Count == 0, "Feature ids tied to two settings: " + string.Join(", ", conflicts));
		}

		private FeatureDispatcher Create(params FeatureEntry[] entries)
		{
			return new FeatureDispatcher(entries, key => _on.Contains(key.Key), store => _stores.Contains(store.Key), _tracker, (id, action) =>
			{
				action();
				return true;
			});
		}

		private FeatureEntry Entry(string id, FeatureTrigger trigger, ToggleKey key, string label = null)
		{
			return new FeatureEntry(id, trigger, key, null, Record(label == null ? id : id + ":" + label));
		}

		private Action Record(string call)
		{
			return () => _calls.Add(call);
		}
	}
}
