using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;

namespace Trainer_v5.Tests
{
	// Full Satisfaction runs every frame: it must drop the same thoughts as the old Values.ToList() pass without copying the list.
	public class FullSatisfactionThoughtTests
	{
		private static readonly FieldInfo MoodField = typeof(Employee.ThoughtEffect).GetField("_mood", BindingFlags.Instance | BindingFlags.NonPublic);

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
