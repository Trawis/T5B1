using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	public class GameVersionCheckTests
	{
		[Fact]
		public void TargetMatchesTheVendoredAssemblyConstants()
		{
			Assert.Equal(GameVersionCheck.TargetTypeInt, ConstantOf("Type"));
			Assert.Equal(GameVersionCheck.TargetMajor, ConstantOf("Major"));
			Assert.Equal(GameVersionCheck.TargetMinor, ConstantOf("Minor"));
			Assert.Equal(GameVersionCheck.TargetRevision, ConstantOf("Revision"));
			Assert.Equal(GameVersionCheck.TargetText,
				"Beta " + GameVersionCheck.TargetMajor + "." + GameVersionCheck.TargetMinor + "." + GameVersionCheck.TargetRevision);
		}

		[Fact]
		public void OnlyTheExactTargetMatches()
		{
			Assert.True(GameVersionCheck.IsTarget(2, 1, 8, 42));
			Assert.False(GameVersionCheck.IsTarget(2, 1, 8, 43));
			Assert.False(GameVersionCheck.IsTarget(2, 1, 9, 42));
			Assert.False(GameVersionCheck.IsTarget(2, 2, 8, 42));
			Assert.False(GameVersionCheck.IsTarget(3, 1, 8, 42));
		}

		private static int ConstantOf(string name)
		{
			var field = typeof(Versioning).GetField(name, BindingFlags.Public | BindingFlags.Static);
			return (int)field.GetRawConstantValue();
		}
	}
}
