using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trainer_v5
{
	public static class Utilities
	{
		public static void AddButton(string text, UnityAction action, List<GameObject> buttons)
		{
			Button button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			button.onClick.AddListener(action);
			buttons.Add(button.gameObject);
		}

		public static void AddInputBox(string text, UnityAction<string> action, List<GameObject> objects)
		{
			InputField inputBox = WindowManager.SpawnInputbox();
			inputBox.text = text;
			inputBox.onValueChanged.AddListener(action);
			objects.Add(inputBox.gameObject);
		}

		public static void AddLabel(string text, Rect labelRect, GUIWindow window)
		{
			Text label = WindowManager.SpawnLabel();
			label.text = text;
			WindowManager.AddElementToWindow(label.gameObject, window, labelRect, new Rect(0, 0, 0, 0));
		}

		public static void AddToggle(string text, bool isOn, UnityAction<bool> action, List<GameObject> toggles)
		{
			Toggle toggle = WindowManager.SpawnCheckbox();
			toggle.GetComponentInChildren<Text>().text = text;
			toggle.isOn = isOn;
			toggle.onValueChanged.AddListener(action);
			toggles.Add(toggle.gameObject);
		}

		public static void AddEmptyBox(List<GameObject> objects)
		{
			Text label = WindowManager.SpawnLabel();
			label.text = string.Empty;
			objects.Add(label.gameObject);
		}

		public static GUICombobox AddComboBox(string text, List<KeyValuePair<string, object>> selectableItems, int selection, List<GameObject> comboBoxes)
		{
			Text label = WindowManager.SpawnLabel();
			label.text = text;

			GUICombobox comboBox = WindowManager.SpawnComboBox();
			comboBox.UpdateContent(selectableItems.Select((KeyValuePair<string, object> x) => x.Key));
			comboBox.UpdateSelection(selection);
			comboBoxes.Add(label.gameObject);
			comboBoxes.Add(comboBox.gameObject);

			return comboBox;
		}

		public static void CreateGameObjects(int column, GameObject[] gameObjects, GUIWindow window, bool isComboBox = false)
		{
			for (int i = 0; i < gameObjects.Length; i++)
			{
				GameObject item = gameObjects[i];

				WindowManager.AddElementToWindow(item, window,
						new Rect(column, (i - (isComboBox ? 1 : 0)) * Constants.ELEMENT_HEIGHT + (isComboBox && i % 2 == 0 ? 16 : 0), Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT),
						new Rect(0, 0, 0, 0));
			}
		}

		public static Button CreateUIButton(UnityAction action, string title, string name)
		{
			var button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = title;
			button.onClick.AddListener(action);
			button.name = name;

			return button;
		}

		public static void AddElementToElement(GameObject gameObject, string path, Rect location)
		{
			WindowManager.AddElementToElement(gameObject, WindowManager.FindElementPath(path).gameObject, location, new Rect(0, 0, 0, 0));
		}

		public static void SetWindowSize(int colums, int xWindowSize, GUIWindow window)
		{
			window.MinSize.x = xWindowSize;
			window.MinSize.y = (colums + 1) * Constants.ELEMENT_HEIGHT;
		}
	}
}