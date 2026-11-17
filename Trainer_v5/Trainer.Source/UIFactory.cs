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

		// Trainer controls stop acting once the trainer is deactivated or the game changes.
		public static Button Button(string text, UnityAction action, string name = null)
		{
			var button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			var bound = TrainerLifecycle.Bind(() => action());
			button.onClick.AddListener(() => bound());
			if (name != null)
			{
				button.name = name;
			}
			return button;
		}

		// The mod's Options screen is the game's, not the trainer's: its toggles also work while the trainer is inactive.
		public static Toggle Toggle(string text, bool isOn, UnityAction<bool> action, bool whileInactive = false)
		{
			var toggle = WindowManager.SpawnCheckbox();
			toggle.GetComponentInChildren<Text>().text = text;
			toggle.isOn = isOn;
			if (whileInactive)
			{
				toggle.onValueChanged.AddListener(action);
			}
			else
			{
				var bound = TrainerLifecycle.Bind<bool>(value => action(value));
				toggle.onValueChanged.AddListener(value => bound(value));
			}

			return toggle;
		}

		public static InputField InputBox(string text, UnityAction<string> action)
		{
			var inputBox = WindowManager.SpawnInputbox();
			inputBox.text = text;
			var bound = TrainerLifecycle.Bind<string>(value => action(value));
			inputBox.onValueChanged.AddListener(value => bound(value));
			return inputBox;
		}
	}

	public class TextStyle
	{
		public TextAnchor? Alignment { get; set; }
		public FontStyle? FontStyle { get; set; }
	}
}
