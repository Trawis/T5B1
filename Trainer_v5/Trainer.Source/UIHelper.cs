using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trainer_v5
{
	public static class UIHelper
	{
		public static GameObject CreateLabel(string text = null, string name = null)
		{
			var control = WindowManager.SpawnLabel();
			control.name = name.NameOrDefault<Text>(text);
			control.text = text.TextOrEmpty();

			return control.gameObject;
		}

		public static GameObject CreateButton(string text, UnityAction action, string name = null)
		{
			var control = WindowManager.SpawnButton();
			control.name = name.NameOrDefault<Button>(text);
			control.GetComponentInChildren<Text>().text = text.TextOrEmpty();
			control.onClick.AddListener(action);

			return control.gameObject;
		}

		public static GameObject CreateInputBox(string text, UnityAction<string> action, string name = null)
		{
			var control = WindowManager.SpawnInputbox();
			control.name = name.NameOrDefault<InputField>(text);
			control.text = text;
			control.onValueChanged.AddListener(action);

			return control.gameObject;
		}

		public static GameObject CreateToggle(string text, bool isOn, UnityAction<bool> action, string name = null)
		{
			var control = WindowManager.SpawnCheckbox();
			control.name = name.NameOrDefault<Toggle>(text);
			control.GetComponentInChildren<Text>().text = text;
			control.isOn = isOn;
			control.onValueChanged.AddListener(action);

			return control.gameObject;
		}

		public static GUICombobox CreateComboBox(List<KeyValuePair<string, object>> selectableItems, int selection, string name = null)
		{
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

		public static void AddToWindow(this List<GameObject> gameObjects, GUIWindow window, int column, bool isComboBox = false)
		{
			for (int i = 0; i < gameObjects.Count; i++)
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

		private static string NameOrDefault<T>(this string name, string text = null)
		{
			return (name?.RemoveWhitespaces() ?? text?.RemoveWhitespaces() ?? "default") + "_" + nameof(T);
		}

		private static string TextOrEmpty(this string text)
		{
			return text ?? string.Empty;
		}

		private static string RemoveWhitespaces(this string input)
		{
			return new string(input.ToCharArray()
				.Where(c => !char.IsWhiteSpace(c))
				.ToArray());
		}
	}

	public class ComboBox
	{
		public GameObject Label { get; set; }
		public GameObject Dropdown { get; set; }
	}
}