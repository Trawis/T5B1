using UnityEngine;

namespace Trainer_v5.Trainer.Source.SDK
{
	public static class ComponentStyleHelper
	{
		private static readonly ComponentStyle _defaultStyle = new ComponentStyle(32);

		public static ComponentStyle GetStyle(this Component self)
		{
			return _defaultStyle;
		}
	}

	public class ComponentStyle
	{
		private readonly int _defaultHeight;
		
		public int DefaultHeight => _defaultHeight;

		public ComponentStyle(int defaultHeight)
		{
			_defaultHeight = defaultHeight;
		}
	}
}
