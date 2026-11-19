namespace Trainer_v5
{
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
