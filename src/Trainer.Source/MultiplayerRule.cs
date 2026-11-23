namespace Trainer_v5
{
	// In multiplayer the trainer changes nothing: every button refuses and every toggle and setting acts as off, so
	// toggles that were on run their restores. The game sets MultiplayerMode only when a lobby starts or is joined.
	internal static class MultiplayerRule
	{
		internal static bool Active
		{
			get { return GameData.MultiplayerMode; }
		}

		internal static bool ActsOn(bool saved, bool multiplayer)
		{
			return saved && !multiplayer;
		}

		internal static bool IsToggleOn(ToggleKey key)
		{
			return ActsOn(Helpers.Settings.Get(key), Active);
		}

		internal static bool IsStoreSet(StoreKey key)
		{
			return ActsOn(Helpers.StoresSettings.Get(key) != null, Active);
		}

		// True when the click was refused.
		internal static bool Refuse()
		{
			if (!Active)
			{
				return false;
			}

			Notification.ShowError(SharedMessages.NotAvailableInMultiplayer());
			return true;
		}
	}
}
