using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trainer_v5
{
	public static class UIHelper
	{
		public static GUIWindow SpawnWindow(string title, string name, string panelName)
		{
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = title;
			window.name = name;
			window.MainPanel.name = panelName;

			return window;
		}

		public static GameObject CreateLabel(string text = null, string name = null)
		{
			var control = UIFactory.Label(text.TextOrEmpty());
			control.name = name.NameOrDefault("Text", text);

			return control.gameObject;
		}

		public static GameObject CreateButton(string text, UnityAction action, string name = null)
		{
			var control = UIFactory.Button(text.TextOrEmpty(), action, name.NameOrDefault("Button", text));

			return control.gameObject;
		}

		public static GameObject CreateInputBox(string text, UnityAction<string> action, string name = null)
		{
			var control = UIFactory.InputBox(text, action);
			control.name = name.NameOrDefault("InputField", text);

			return control.gameObject;
		}

		public static GameObject CreateToggle(string text, bool isOn, UnityAction<bool> action, string name = null)
		{
			var control = UIFactory.Toggle(text, isOn, action);
			control.name = name.NameOrDefault("Toggle", text);

			return control.gameObject;
		}

		public static GUICombobox CreateComboBox(Dictionary<string, object> selectableItems, int selection, string name = null)
		{
			var comboBox = WindowManager.SpawnComboBox();
			comboBox.name = name.NameOrDefault("GUICombobox");
			comboBox.UpdateContent(selectableItems.Select(x => x.Key));
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
				WindowManager.AddElementToWindow(gameObjects[i], window, GetColumnSlot(i, column, isComboBox), new Rect(0, 0, 0, 0));
			}
		}

		public static void SetWindowSize(this GUIWindow window, int rows, int xWindowSize)
		{
			window.SetMinSize(xWindowSize, (rows + 1) * Constants.ELEMENT_HEIGHT);
		}

		public static void SetMinSize(this GUIWindow window, int width, int height)
		{
			window.MinSize.x = width;
			window.MinSize.y = height;
		}

		/// <summary>
		/// Estimates how many rows of <paramref name="rowHeight"/> comfortably fit on screen for a
		/// window, after reserving space for window chrome (title bar/margins) and any fixed,
		/// non-scrolling rows (<paramref name="reservedRows"/>) the caller will also place.
		/// </summary>
		public static int GetMaxVisibleRows(int rowHeight, int reservedRows)
		{
			float canvasHeight = FALLBACK_CANVAS_HEIGHT;

			var canvas = WindowManager.Instance != null ? WindowManager.Instance.Canvas : null;
			var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
			if (canvasRect != null && canvasRect.rect.height > 0f)
			{
				canvasHeight = canvasRect.rect.height;
			}

			int availableRows = Mathf.FloorToInt((canvasHeight - WINDOW_CHROME_HEIGHT) / rowHeight) - reservedRows;
			return Mathf.Max(MIN_VISIBLE_ROWS, availableRows);
		}

		/// <summary>
		/// Lays out a column made of a fixed header, a list of rows that only scrolls when it would
		/// otherwise exceed <paramref name="maxVisibleRows"/>, and a fixed footer that always stays
		/// visible right below the (bounded-height) scroll area. Returns the total height, in window
		/// units, the column occupies below <paramref name="origin"/>.y.
		/// </summary>
		public static int CreateScrollableColumn(GUIWindow window, Rect origin, GameObject[] header, GameObject[] scrollableItems, GameObject[] footer, int itemHeight, int gap, int maxVisibleRows)
		{
			var zeroAnchors = new Rect(0, 0, 0, 0);
			float x = origin.x;
			float width = origin.width;
			float y = origin.y;
			int step = itemHeight + gap;
			int totalSlots = 0;

			foreach (var item in header)
			{
				WindowManager.AddElementToWindow(item, window, new Rect(x, y, width, itemHeight), zeroAnchors);
				y += step;
				totalSlots++;
			}

			int rowCount = scrollableItems.Length;
			if (rowCount <= maxVisibleRows)
			{
				foreach (var item in scrollableItems)
				{
					WindowManager.AddElementToWindow(item, window, new Rect(x, y, width, itemHeight), zeroAnchors);
					y += step;
					totalSlots++;
				}
			}
			else
			{
				float scrollAreaHeight = maxVisibleRows * step - gap;
				float contentHeight = rowCount * step - gap;

				var viewport = WindowManager.SpawnPanel();
				viewport.gameObject.name = "ScrollViewport";
				WindowManager.AddElementToWindow(viewport.gameObject, window, new Rect(x, y, width, scrollAreaHeight), zeroAnchors);
				viewport.gameObject.AddComponent<RectMask2D>();
				var scrollRect = viewport.gameObject.AddComponent<ScrollRect>();

				var content = WindowManager.SpawnPanel();
				content.gameObject.name = "ScrollContent";
				WindowManager.AddElementToElement(content.gameObject, viewport.gameObject, new Rect(0, 0, width, contentHeight), zeroAnchors);

				float itemY = 0;
				foreach (var item in scrollableItems)
				{
					WindowManager.AddElementToElement(item, content.gameObject, new Rect(0, itemY, width, itemHeight), zeroAnchors);
					itemY += step;
				}

				scrollRect.content = content;
				scrollRect.horizontal = false;
				scrollRect.vertical = true;
				scrollRect.movementType = ScrollRect.MovementType.Clamped;
				scrollRect.scrollSensitivity = itemHeight;

				y += maxVisibleRows * step;
				totalSlots += maxVisibleRows;
			}

			foreach (var item in footer)
			{
				WindowManager.AddElementToWindow(item, window, new Rect(x, y, width, itemHeight), zeroAnchors);
				y += step;
				totalSlots++;
			}

			return totalSlots > 0 ? Mathf.RoundToInt(y - origin.y - gap) : 0;
		}

		internal static Rect GetColumnSlot(int index, int column, bool isComboBox)
		{
			return new Rect(column, (index - (isComboBox ? 1 : 0)) * Constants.ELEMENT_HEIGHT + (isComboBox && index % 2 == 0 ? 16 : 0), Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT);
		}

		private const float FALLBACK_CANVAS_HEIGHT = 1080f;
		private const float WINDOW_CHROME_HEIGHT = 200f;
		private const int MIN_VISIBLE_ROWS = 6;

		private static string NameOrDefault(this string name, string suffix, string text = null)
		{
			return (name?.RemoveWhitespaces() ?? text?.RemoveWhitespaces() ?? "default") + "_" + suffix;
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
}