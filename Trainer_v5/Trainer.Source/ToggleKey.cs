namespace Trainer_v5
{
	// Not generic: the game's source-mod compiler emits invalid IL for fields of user-defined generic types.
	public sealed class ToggleKey
	{
		public readonly string Key;
		public readonly bool DefaultValue;

		internal ToggleKey(string key, bool defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}

		public override string ToString() => Key;
	}
}
