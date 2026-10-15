namespace Trainer_v5
{
	public sealed class StoreKey
	{
		public readonly string Key;
		public readonly object DefaultValue;

		internal StoreKey(string key, object defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}

		public override string ToString() => Key;
	}
}
