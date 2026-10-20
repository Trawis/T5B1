using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trainer_v5
{
	public static class UIHelper
	{
		private const float FALLBACK_CANVAS_HEIGHT = 1080f;
		private const float WINDOW_CHROME_HEIGHT = 200f;
		private const int MIN_VISIBLE_ROWS = 6;
		private const int TITLE_BAR_HEIGHT = Constants.ELEMENT_HEIGHT;

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

		// False when the game has no element at the path, which the game's own lookup reports as null.
		public static bool AddToElement(this GameObject gameObject, string elementPath, Rect location)
		{
			var parent = WindowManager.FindElementPath(elementPath);
			if (parent == null)
			{
				$"UI element '{elementPath}' was not found; the control was not added.".Log();
				return false;
			}

			WindowManager.AddElementToElement(gameObject, parent.gameObject, location, new Rect(0, 0, 0, 0));
			return true;
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

		/// <summary>Estimates how many rows of rowHeight fit on screen after window chrome and reservedRows.</summary>
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

		/// <summary>Lays out a header, scrolling rows (up to maxVisibleRows) and a footer; returns the column height.</summary>
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

		/// <summary>Window height for two scrollable columns: the taller column, the offset its origin sits below the top (0 when it starts at y = 0), and one title-bar row; without that row the footer falls outside the window.</summary>
		internal static int GetColumnWindowHeight(int firstColumnHeight, int secondColumnHeight, int columnOriginY)
		{
			return System.Math.Max(firstColumnHeight, secondColumnHeight) + columnOriginY + TITLE_BAR_HEIGHT;
		}

		internal static Rect GetColumnSlot(int index, int column, bool isComboBox)
		{
			return new Rect(column, (index - (isComboBox ? 1 : 0)) * Constants.ELEMENT_HEIGHT + (isComboBox && index % 2 == 0 ? 16 : 0), Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT);
		}

		// Same x slots as the FIRST_COLUMN..FOURTH_COLUMN constants.
		internal static int GetGridColumnX(int column)
		{
			return column <= 0 ? Constants.FIRST_COLUMN : Constants.SECOND_COLUMN + (column - 1) * (Constants.THIRD_COLUMN - Constants.SECOND_COLUMN);
		}

		internal static Rect GetGridSlot(int index, int columns, int firstRow)
		{
			return new Rect(GetGridColumnX(index % columns), (firstRow + index / columns) * Constants.ELEMENT_HEIGHT, Constants.ELEMENT_WIDTH, Constants.ELEMENT_HEIGHT);
		}

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
