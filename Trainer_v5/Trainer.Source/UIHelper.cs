using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trainer_v5
{
	public static class UIHelper
	{
		public static Text CreateLabel(string text = null, string name = null)
		{
			var control = WindowManager.SpawnLabel();
			control.name = name.NameOrDefault<Text>();
			control.text = text;

			return control;
		}

		public static Button CreateButton(string text, UnityAction action, string name = null)
		{
			var control = WindowManager.SpawnButton();
			control.name = name.NameOrDefault<Button>();
			control.GetComponentInChildren<Text>().text = text;
			control.onClick.AddListener(action);

			return control;
		}

		public static InputField CreateInputBox(string text, UnityAction<string> action, string name = null)
		{
			var control = WindowManager.SpawnInputbox();
			control.name = name.NameOrDefault<InputField>();
			control.text = text;
			control.onValueChanged.AddListener(action);

			return control;
		}

		public static Toggle CreateToggle(bool isOn, UnityAction<bool> action, string text, string name = null)
		{
			var control = WindowManager.SpawnCheckbox();
			control.name = name.NameOrDefault<Toggle>();
			control.GetComponentInChildren<Text>().text = text;
			control.isOn = isOn;
			control.onValueChanged.AddListener(action);

			return control;
		}

		public static GUICombobox CreateComboBox(List<KeyValuePair<string, object>> selectableItems, int selection, string text, string name = null)
		{
			var label = WindowManager.SpawnLabel();
			label.name = name.NameOrDefault<Text>();
			label.text = text;

			var comboBox = WindowManager.SpawnComboBox();
			comboBox.name = name.NameOrDefault<GUICombobox>();
			comboBox.UpdateContent(selectableItems.Select((KeyValuePair<string, object> x) => x.Key));
			comboBox.UpdateSelection(selection);

			return comboBox;
		}

		public static void AddToElement(this GameObject gameObject, string elementPath, Rect location)
		{
			WindowManager.AddElementToElement(gameObject, WindowManager.FindElementPath(elementPath).gameObject, location, new Rect(0, 0, 0, 0));
		}

		public static void AddToWindow(this GameObject[] gameObjects, GUIWindow window, int column, bool isComboBox = false)
		{
			for (int i = 0; i < gameObjects.Length; i++)
			{
				var gameObject = gameObjects[i];

				WindowManager.AddElementToWindow(gameObject, window,
						new Rect(column, (i - (isComboBox ? 1 : 0)) * Constants.ELEMENT_HEIGHT + (isComboBox && i % 2 == 0 ? 16 : 0), Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT),
						new Rect(0, 0, 0, 0));
			}
		}

		public static void SetWindowSize(this GUIWindow window, int rows, int xWindowSize)
		{
			window.MinSize.x = xWindowSize;
			window.MinSize.y = (rows + 1) * Constants.ELEMENT_HEIGHT;
		}

		private static string NameOrDefault<T>(this string name)
		{
			return (name ?? "default") + "_" + nameof(T);
		}
	}
}