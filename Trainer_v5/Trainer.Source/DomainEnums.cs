namespace Trainer_v5
{
	internal enum TrainerDomain
	{
		None = 0,
		Company = 1,
		Employee = 2,
		Product = 3,
		Work = 4,
		Environment = 5
	}

	internal enum SettingsTab
	{
		None = 0,
		Actions = 1,
		Toggles = 2,
		Settings = 3
	}

	internal enum PatentOutcome
	{
		None = 0,
		Patented = 1,
		Unaffordable = 2,
		NotPatentable = 3
	}

	internal enum InputError
	{
		None = 0,
		Invalid = 1,
		BelowMin = 2,
		AboveMax = 3
	}
}
