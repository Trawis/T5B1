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
			window.SetTitle(title);
			window.name = name;
			window.MainPanel.name = panelName;

			return window;
		}

		// The three title fields must change together or the game redraws the old title.
		public static void SetTitle(this GUIWindow window, string title)
		{
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = title;
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

			PlaceInElement(gameObject, parent.gameObject, location);
			return true;
		}

		public static void PlaceInWindow(GameObject element, GUIWindow window, Rect location)
		{
			WindowManager.AddElementToWindow(element, window, location, new Rect(0, 0, 0, 0));
		}

		public static void PlaceInElement(GameObject element, GameObject container, Rect location)
		{
			WindowManager.AddElementToElement(element, container, location, new Rect(0, 0, 0, 0));
		}

		// Makes a viewport scroll vertically over its content, one step per mouse-wheel notch.
		public static void MakeVerticallyScrollable(GameObject viewport, RectTransform content, float step)
		{
			viewport.AddComponent<RectMask2D>();
			var scrollRect = viewport.AddComponent<ScrollRect>();
			scrollRect.content = content;
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.scrollSensitivity = step;
		}

		public static void AddToWindow(this List<GameObject> gameObjects, GUIWindow window, int column, bool isComboBox = false)
		{
			for (int i = 0; i < gameObjects.Count; i++)
			{
				PlaceInWindow(gameObjects[i], window, GetColumnSlot(i, column, isComboBox));
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
		public static int GetMaxVisibleRows(int rowHeight, int reservedRows, int gap = 0)
		{
			float canvasHeight = FALLBACK_CANVAS_HEIGHT;

			var canvas = WindowManager.Instance != null ? WindowManager.Instance.Canvas : null;
			var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
			if (canvasRect != null && canvasRect.rect.height > 0f)
			{
				canvasHeight = canvasRect.rect.height;
			}

			return FitRows(canvasHeight, rowHeight + gap, reservedRows);
		}

		// The preferred minimum of rows gives way when the canvas cannot hold it; a larger MinSize would push the footer off screen.
		internal static int FitRows(float canvasHeight, int rowStep, int reservedRows)
		{
			int availableRows = (int)System.Math.Floor((canvasHeight - WINDOW_CHROME_HEIGHT) / rowStep) - reservedRows;
			int fittingRows = (int)System.Math.Floor(canvasHeight / rowStep) - reservedRows - 1;
			return System.Math.Max(1, System.Math.Max(System.Math.Min(MIN_VISIBLE_ROWS, fittingRows), availableRows));
		}

		/// <summary>Lays out a header, scrolling rows (up to maxVisibleRows) and a footer; returns the column height.</summary>
		public static int CreateScrollableColumn(GUIWindow window, Rect origin, GameObject[] header, GameObject[] scrollableItems, GameObject[] footer, int itemHeight, int gap, int maxVisibleRows)
		{
			float x = origin.x;
			float width = origin.width;
			float y = origin.y;
			int step = itemHeight + gap;
			int totalSlots = 0;

			foreach (var item in header)
			{
				PlaceInWindow(item, window, new Rect(x, y, width, itemHeight));
				y += step;
				totalSlots++;
			}

			int rowCount = scrollableItems.Length;
			if (rowCount <= maxVisibleRows)
			{
				foreach (var item in scrollableItems)
				{
					PlaceInWindow(item, window, new Rect(x, y, width, itemHeight));
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
				PlaceInWindow(viewport.gameObject, window, new Rect(x, y, width, scrollAreaHeight));

				var content = WindowManager.SpawnPanel();
				content.gameObject.name = "ScrollContent";
				PlaceInElement(content.gameObject, viewport.gameObject, new Rect(0, 0, width, contentHeight));

				float itemY = 0;
				foreach (var item in scrollableItems)
				{
					PlaceInElement(item, content.gameObject, new Rect(0, itemY, width, itemHeight));
					itemY += step;
				}

				MakeVerticallyScrollable(viewport.gameObject, content, itemHeight);

				y += maxVisibleRows * step;
				totalSlots += maxVisibleRows;
			}

			foreach (var item in footer)
			{
				PlaceInWindow(item, window, new Rect(x, y, width, itemHeight));
				y += step;
				totalSlots++;
			}

			return totalSlots > 0 ? Mathf.RoundToInt(y - origin.y - gap) : 0;
		}

		/// <summary>Window height for two scrollable columns: the taller column, the offset its origin sits below the top (0 when it starts at y = 0), and one title-bar row (else the footer falls outside).</summary>
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
