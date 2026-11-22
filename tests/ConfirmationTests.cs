using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class ConfirmationTests
	{
		[Fact]
		public void OnlyTheSameTargetsInAnyOrderAreConfirmed()
		{
			object a = new object(), b = new object(), c = new object();
			var shown = new List<object> { a, b };

			Assert.True(Confirmation.IsSameTargets(shown, new List<object> { b, a }));
			Assert.False(Confirmation.IsSameTargets(shown, new List<object> { a, c }));
			Assert.False(Confirmation.IsSameTargets(shown, new List<object> { a }));
			Assert.False(Confirmation.IsSameTargets(shown, new List<object> { a, b, c }));
			Assert.False(Confirmation.IsSameTargets(new List<object>(), new List<object>()));
		}

		// Runs the sequence and records what it did, in order.
		private static string RunSequence(bool ready, string change, string result)
		{
			var steps = new List<string>();
			Confirmation.Run(
				() => { steps.Add("ready"); return ready; },
				() => { steps.Add("check"); return change; },
				() => { steps.Add("apply"); return result; },
				text => steps.Add("changed:" + text),
				text => steps.Add("result:" + text));
			return string.Join(" ", steps);
		}

		[Theory]
		[InlineData(false, null, "done", "ready")]
		[InlineData(true, "moved", "done", "ready check changed:moved")]
		[InlineData(true, null, "done", "ready check apply result:done")]
		[InlineData(true, null, null, "ready check apply")]
		public void TheSequenceAppliesOnlyWhenReadyAndUnchangedAndThenReports(bool ready, string change, string result, string expected)
		{
			Assert.Equal(expected, RunSequence(ready, change, result));
		}
	}
}
