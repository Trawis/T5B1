using UnityEngine;

namespace Trainer_v5.Trainer.Source.SDK
{
	public static class ComponentStyleHelper
	{
		private static readonly ComponentStyle DefaultStyle = new ComponentStyle(32);

		public static ComponentStyle GetStyle(this Component self)
		{
			return DefaultStyle;
		}
	}

	public class ComponentStyle
	{
		public readonly int DefaultHeight;

		public ComponentStyle(int defaultHeight)
		{
			DefaultHeight = defaultHeight;
		}
	}
}
