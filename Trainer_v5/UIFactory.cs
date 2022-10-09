using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ReSharper disable once InconsistentNaming
namespace Trainer_v5
{
	public static class UIFactory
	{
		public static Text
		EmptyBox()
		{
			var label = WindowManager.SpawnLabel();
			label.text = string.Empty;
			return label;
		}

		public static Text
		Label(string text, TextStyle style = null)
		{
			var r = WindowManager.SpawnLabel();
			r.text = text;
			// apply style
			if (style != null)
			{
				if (style.Alignment != null)
					r.alignment = style.Alignment.Value;
				if (style.FontStyle != null)
					r.fontStyle = style.FontStyle.Value;
			}
			return r;
		}

		public static Button
		Button(string text, UnityAction action)
		{
			var button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			button.onClick.AddListener(action);
			return button;
		}

		public static Button 
		UIButton(string text, string name, UnityAction action)
		{
			var button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			button.onClick.AddListener(action);
			button.name = name;
			return button;
		}

		public static Toggle 
		Toggle(string text, bool isOn, UnityAction<bool> action)
		{
			var toggle = WindowManager.SpawnCheckbox();
			toggle.GetComponentInChildren<Text>().text = text;
			toggle.isOn = isOn;
			toggle.onValueChanged.AddListener(action);
			return toggle;
		}
	}

	public class TextStyle
	{
		public TextAnchor? Alignment;
		public FontStyle? FontStyle;
	}
}
