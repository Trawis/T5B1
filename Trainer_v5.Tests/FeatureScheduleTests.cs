using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	// A persistent toggle needs a cadence and a decision about what happens the moment it is switched on (#273).
	public class FeatureScheduleTests
	{
		private static readonly string[] Periodic = { "Minute", "Hour", "Day", "Month" };

		[Fact]
		public void EveryPeriodicFeatureCatchesUpWhenEnabledOrIsListedAsNotNeedingTo()
		{
			var triggers = Triggers();
			var missing = new List<string>();
			foreach (var id in triggers.Keys)
			{
				if (HasAny(triggers[id], Periodic) && !triggers[id].Contains("Enabled") && System.Array.IndexOf(FeatureRegistry.NoCatchUp, id) < 0)
				{
					missing.Add(id);
				}
			}

			Assert.True(missing.Count == 0, "Add an Enabled row so these run when switched on, or list them in FeatureRegistry.NoCatchUp: " + string.Join(", ", missing));
		}

		[Fact]
		public void TheNoCatchUpListHasNoStaleEntries()
		{
			var triggers = Triggers();
			var stale = new List<string>();
			foreach (var id in FeatureRegistry.NoCatchUp)
			{
				HashSet<string> own;
				if (!triggers.TryGetValue(id, out own) || !HasAny(own, Periodic) || own.Contains("Enabled"))
				{
					stale.Add(id);
				}
			}

			Assert.True(stale.Count == 0, "Not periodic, unknown, or already catching up: " + string.Join(", ", stale));
		}

		[Fact]
		public void EveryRestoreRowHasAnApplyRowForTheSameFeature()
		{
			var orphans = new List<string>();
			foreach (var pair in Triggers())
			{
				if (pair.Value.Contains("Disabled") && !pair.Value.Contains("Enabled"))
				{
					orphans.Add(pair.Key);
				}
			}

			Assert.True(orphans.Count == 0, "Disabled rows without an Enabled row: " + string.Join(", ", orphans));
		}

		private static Dictionary<string, HashSet<string>> Triggers()
		{
			var triggers = new Dictionary<string, HashSet<string>>();
			// The registry the trainer actually runs, not its source text: delegates are created, nothing is invoked.
			foreach (var entry in FeatureRegistry.Create())
			{
				if (!triggers.ContainsKey(entry.Id))
				{
					triggers[entry.Id] = new HashSet<string>();
				}
				triggers[entry.Id].Add(entry.Trigger.ToString());
			}

			Assert.NotEmpty(triggers);
			return triggers;
		}

		private static bool HasAny(HashSet<string> set, string[] names)
		{
			foreach (var name in names)
			{
				if (set.Contains(name))
				{
					return true;
				}
			}
			return false;
		}
	}
}
