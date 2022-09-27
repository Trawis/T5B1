using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// ReSharper disable once InconsistentNaming
namespace Trainer_v5
{
	public static class UIFactory
	{
		public static GameObject
		EmptyBox()
		{
			var label = WindowManager.SpawnLabel();
			label.text = string.Empty;
			return label.gameObject;
		}


		public static GameObject
		Button(string text, UnityAction action)
		{
			var button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			button.onClick.AddListener(action);
			return button.gameObject;
		}


		public static GameObject 
		Toggle(string text, bool isOn, UnityAction<bool> action)
		{
			var toggle = WindowManager.SpawnCheckbox();
			toggle.GetComponentInChildren<Text>().text = text;
			toggle.isOn = isOn;
			toggle.onValueChanged.AddListener(action);
			return toggle.gameObject;
		}
	}
}
