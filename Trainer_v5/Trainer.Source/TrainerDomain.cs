namespace Trainer_v5
{
	// What a Trainer Settings control is about. Order and identity here are code, never the localized section header.
	// None is the unset value: a table row without a domain fails SettingToggleRowsTests / SettingActionRowsTests.
	internal enum TrainerDomain
	{
		None = 0,
		Company,
		Employee,
		Product,
		Work,
		Environment
	}

	// The three top-level pages of the Trainer Settings window. None is "no page chosen yet" and shows Actions.
	internal enum SettingsTab
	{
		None = 0,
		Actions,
		Toggles,
		Settings
	}
}
