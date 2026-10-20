namespace Trainer_v5
{
	// None is unset: a table row without a domain fails SettingToggleRowsTests / SettingActionRowsTests.
	internal enum TrainerDomain
	{
		None = 0,
		Company = 1,
		Employee = 2,
		Product = 3,
		Work = 4,
		Environment = 5
	}

	// None means no page chosen yet and shows Actions.
	internal enum SettingsTab
	{
		None = 0,
		Actions = 1,
		Toggles = 2,
		Settings = 3
	}
}
