using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	public class HotkeyInputTests
	{
		private static readonly Regex HeldKey = new Regex(@"\bInput\.GetKey\(", RegexOptions.Compiled);

		[Fact]
		public void TheBehaviourReadsKeyPressesNotHeldKeys()
		{
			var source = SourceCode.Read("TrainerBehaviour.cs");

			Assert.DoesNotMatch(HeldKey, source);
			Assert.Contains("Input.GetKeyDown(KeyCode.F1)", source);
			Assert.Contains("Input.GetKeyDown(KeyCode.F2)", source);
		}
	}
}
