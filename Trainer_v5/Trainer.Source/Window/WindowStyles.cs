using UnityEngine;

namespace Trainer_v5.Window
{
	// Moved TextStyle class here from deleted UIFactory.cs
	public class TextStyle
	{
		public TextAnchor? Alignment { get; set; }
		public FontStyle? FontStyle { get; set; }
	}

	public static class WindowStyles
	{
		public static readonly TextStyle TitleStyle = new TextStyle
		{
			Alignment = TextAnchor.MiddleCenter,
			FontStyle = FontStyle.Bold
		};
	}
}
