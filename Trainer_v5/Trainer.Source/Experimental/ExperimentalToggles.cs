using System.Collections.Generic;

namespace Trainer_v5.Experimental
{
	internal static class ExperimentalToggles
	{
		public static ToggleKey[] Keys()
		{
			return new[] { SettingKeys.NoForgetfulSkillLoss, SettingKeys.AutoInstantPatent, SettingKeys.AutoInstantResearch, SettingKeys.AutoMaxFollowers };
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
