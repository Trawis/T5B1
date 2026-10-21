using System.Collections.Generic;

namespace Trainer_v5.Experimental
{
	// Trainer Settings toggles that exist only while the Experimental option is on (#457). Turning the option off
	// switches them off; a save that still has one on is cleared when its game is ready.
	internal static class ExperimentalToggles
	{
		public static ToggleKey[] Keys()
		{
			return new[] { SettingKeys.NoForgetfulSkillLoss };
		}

		public static void TurnOffAll(Dictionary<string, bool> settings)
		{
			if (settings == null)
			{
				return;
			}

			foreach (ToggleKey key in Keys())
			{
				settings[key.Key] = false;
			}
		}
	}
}
