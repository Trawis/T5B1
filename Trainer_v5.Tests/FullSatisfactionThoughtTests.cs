using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;

namespace Trainer_v5.Tests
{
	// Full Satisfaction runs every frame (#361): it must drop the same thoughts as the old Values.ToList() pass without copying the list.
	public class FullSatisfactionThoughtTests
	{
		private static readonly FieldInfo MoodField = typeof(Employee.ThoughtEffect).GetField("_mood", BindingFlags.Instance | BindingFlags.NonPublic);

		[Fact]
		public void NegativeSueAndQuitReasonThoughtsAreUnwanted()
		{
			Assert.True(EmployeeFeatures.IsUnwantedThought(Thought("a", true, false, null).Mood));
			Assert.True(EmployeeFeatures.IsUnwantedThought(Thought("b", false, true, null).Mood));
			Assert.True(EmployeeFeatures.IsUnwantedThought(Thought("c", false, false, "Quit").Mood));
			Assert.False(EmployeeFeatures.IsUnwantedThought(Thought("d", false, false, null).Mood));
			Assert.False(EmployeeFeatures.IsUnwantedThought(Thought("e", false, false, string.Empty).Mood));
		}

		[Fact]
		public void RemovesExactlyTheUnwantedThoughtsAndKeepsTheRestInOrder()
		{
			var thoughts = Thoughts(
				Thought("t0", true, false, null),
				Thought("t1", false, false, null),
				Thought("t2", false, true, null),
				Thought("t3", true, false, null),
				Thought("t4", false, false, "Quit"),
				Thought("t5", false, false, null),
				Thought("t6", true, false, null));

			EmployeeFeatures.RemoveUnwantedThoughts(thoughts);

			Assert.Equal(new[] { "t1", "t5" }, Keys(thoughts));
			Assert.False(thoughts.ContainsKey("t0"));
			Assert.Equal("t5", thoughts["t5"].Thought);
		}

		[Fact]
		public void MatchesTheCopyingImplementationOnEverySubsetOfSampleData()
		{
			for (int mask = 0; mask < 64; mask++)
			{
				var items = new List<Employee.ThoughtEffect>();
				for (int i = 0; i < 6; i++)
				{
					bool negative = ((mask >> i) & 1) == 1;
					items.Add(Thought("k" + i, negative, i % 3 == 0 && !negative, i == 4 ? "Quit" : null));
				}

				var expected = new List<string>();
				foreach (var item in items)
				{
					if (!(item.Mood.Negative || item.Mood.Sue || !string.IsNullOrEmpty(item.Mood.QuitReason)))
					{
						expected.Add(item.Thought);
					}
				}

				var actual = Thoughts(items.ToArray());
				EmployeeFeatures.RemoveUnwantedThoughts(actual);

				Assert.Equal(expected.ToArray(), Keys(actual));
			}
		}

		[Fact]
		public void LeavesAnEmptyOrAlreadyCleanListUntouched()
		{
			var empty = Thoughts();
			EmployeeFeatures.RemoveUnwantedThoughts(empty);
			Assert.Empty(Keys(empty));

			var clean = Thoughts(Thought("x", false, false, null), Thought("y", false, false, null));
			EmployeeFeatures.RemoveUnwantedThoughts(clean);
			Assert.Equal(new[] { "x", "y" }, Keys(clean));
		}

		// HashList.GetEnumerator boxes a list enumerator, and Thoughts.Values.ToList() copies: neither belongs in a per-frame pass.
		[Fact]
		public void TheFramePassesDoNotCopyListsOrBoxEnumerators()
		{
			string employee = Normalize(File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EmployeeFeatures.cs")));
			string environment = Normalize(File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "EnvironmentFeatures.cs")));

			Assert.DoesNotContain("Values.ToList()", employee);
			Assert.DoesNotContain("ApplyNoiseReduction()\n\t\t{\n\t\t\tforeach", environment);
			Assert.DoesNotContain("ApplyDisableActiveFires()\n\t\t{\n\t\t\tforeach", environment);
		}

		private static string Normalize(string text)
		{
			return text.Replace("\r\n", "\n");
		}

		private static Employee.ThoughtEffect Thought(string name, bool negative, bool sue, string quitReason)
		{
			var mood = (MoodEffect)FormatterServices.GetUninitializedObject(typeof(MoodEffect));
			mood.Thought = name;
			mood.Negative = negative;
			mood.Sue = sue;
			mood.QuitReason = quitReason;

			var thought = (Employee.ThoughtEffect)FormatterServices.GetUninitializedObject(typeof(Employee.ThoughtEffect));
			thought.Thought = name;
			MoodField.SetValue(thought, mood);
			return thought;
		}

		private static DictionaryList<string, Employee.ThoughtEffect> Thoughts(params Employee.ThoughtEffect[] items)
		{
			var list = new DictionaryList<string, Employee.ThoughtEffect>();
			for (int i = 0; i < items.Length; i++)
			{
				list.Add(items[i].Thought, items[i]);
			}

			return list;
		}

		private static string[] Keys(DictionaryList<string, Employee.ThoughtEffect> thoughts)
		{
			var keys = new string[thoughts.Count];
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = thoughts[i].Thought;
			}

			return keys;
		}
	}
}
