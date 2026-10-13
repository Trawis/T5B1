namespace Trainer_v5
{
	public sealed class SettingKey<T>
	{
		internal SettingKey(string key, T defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}

		public string Key { get; }
		public T DefaultValue { get; }

		public override string ToString() => Key;
	}
}
