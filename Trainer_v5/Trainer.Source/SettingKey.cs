namespace Trainer_v5
{
	public sealed class SettingKey<T>
	{
		// Fields, not auto-properties: the game's runtime throws FieldAccessException on the generated backing fields of a generic type.
		public readonly string Key;
		public readonly T DefaultValue;

		internal SettingKey(string key, T defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}

		public override string ToString() => Key;
	}
}
