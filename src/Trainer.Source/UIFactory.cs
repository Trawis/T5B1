using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trainer_v5
{
	public static class UIFactory
	{
		public static Text EmptyBox()
		{
			var label = WindowManager.SpawnLabel();
			label.text = string.Empty;
			return label;
		}

		public static Text Label(string text, TextStyle style = null)
		{
			var r = WindowManager.SpawnLabel();
			r.text = text;
			if (style != null)
			{
				if (style.Alignment != null)
					r.alignment = style.Alignment.Value;
				if (style.FontStyle != null)
					r.fontStyle = style.FontStyle.Value;
			}
			return r;
		}

		public static Button Button(string text, UnityAction action, string name = null)
		{
			var button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			button.onClick.AddListener(action);
			if (name != null)
			{
				button.name = name;
			}
			return button;
		}

		public static Toggle Toggle(string text, bool isOn, UnityAction<bool> action)
		{
			var toggle = WindowManager.SpawnCheckbox();
			toggle.GetComponentInChildren<Text>().text = text;
			toggle.isOn = isOn;
			toggle.onValueChanged.AddListener(action);
			return toggle;
		}

		public static InputField InputBox(string text, UnityAction<string> action)
		{
			var inputBox = WindowManager.SpawnInputbox();
			inputBox.text = text;
			inputBox.onValueChanged.AddListener(action);
			return inputBox;
		}
	}

	public class TextStyle
	{
		public TextAnchor? Alignment { get; set; }
		public FontStyle? FontStyle { get; set; }
	}
}
