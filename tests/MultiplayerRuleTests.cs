using Xunit;

namespace Trainer_v5.Tests
{
	public class MultiplayerRuleTests
	{
		[Theory]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		public void TogglesAndSettingsActAsOffInMultiplayer(bool saved, bool multiplayer, bool expected)
		{
			Assert.Equal(expected, MultiplayerRule.ActsOn(saved, multiplayer));
		}

		// The rule is enforced where every button and every toggle read passes, so a new action or toggle cannot skip it.
		// Helpers holds the overloads, Main serializes the settings, and the efficiency rows read their store value only while
		// the store is set.
		[Fact]
		public void OnlyTheRuleReadsMultiplayerModeAndEveryButtonAndToggleReadPassesThroughIt()
		{
			CompiledCalls.OnlyIn("ldsfld GameData.MultiplayerMode", "MultiplayerRule");
			CompiledCalls.OnlyIn("call WindowManager.SpawnButton", "UIFactory");
			CompiledCalls.OnlyIn("call Button.get_onClick", "UIFactory");
			CompiledCalls.OnlyIn("call Helpers.GetProperty", "Helpers", "MultiplayerRule", "Main", "EmployeeFeatures.ApplyLeadEfficiencyStore", "EmployeeFeatures.ApplyEfficiencyStore");
		}

		[Fact]
		public void EveryButtonAndTheHotkeyRefuseInMultiplayer()
		{
			CompiledCalls.InOrder(typeof(UIFactory), "Button", "call MultiplayerRule.Refuse", "call UnityAction.Invoke");
			CompiledCalls.InOrder(typeof(Main), "OpenSettingsWindow", "call MultiplayerRule.Refuse", "call SettingsWindow.Open");
		}
	}
}
