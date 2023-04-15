using System;
using System.Linq;
using System.Text;
using NLua;
using Trainer_v5.SDK;
using UnityEngine;

namespace Trainer_v5.Trainer.Source.Window
{
	public class ConsoleWindow : MonoBehaviour
	{
		public static ConsoleWindow Instance => _instance.Value;
		private static readonly Lazy<ConsoleWindow> _instance = new Lazy<ConsoleWindow>(() => new ConsoleWindow());

		private GUIWindow _window;

		public void Show()
		{
			if (_window == null)
				CreateWindow();
			else
				_window.Toggle();
		}

		private void CreateWindow()
		{
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = "Console";
			window.name = "ConsoleWindowEx";
			window.MainPanel.name = "MainPanel";

			var input = UIFactory.MultilineInput();
			var executeButton = UIFactory.Button("Execute", () =>
			{
				var text = input.text;
				LuaEngine.Execute(text);
			});
			var clearButton = UIFactory.Button("Clear", () => input.text = "");

			window.Add(input, new Rect(5, 5, 640, 480));
			window.Add(executeButton, new Rect(5, 490, 150, 32));
			window.Add(clearButton, new Rect(160, 490, 150, 32));
			window.MinSize.x = 650;
			window.MinSize.y = 527;

			_window = window;
		}
	}

	public static class LuaEngine
	{
		private static Lua lua;
		private static bool invalid = true;

		private static void Init()
		{
			if (!invalid) return;

			lua = new Lua();
			lua["wm"] = WindowManager.Instance;
			lua["gs"] = GameSettings.Instance;
			lua["WM"] = WM.Instance;
		}

		public static void Execute(string script)
		{
			try
			{
				Init();
				var result = lua.DoString(script);
				Notification.ShowError($"{result.Length}\n{string.Join("\n", result)}");
			}
			catch (Exception ex)
			{
				Notification.ShowError(ex.ToString());
			}
		}
	}

	// return WM:FindElementPath("DetailWindow")
	// return WM:ListChild("DetailWindow")
	// WM:ListChild("DetailWindow/ContentPanel/LeftInfo/ButtonPanel", 1)
	public class WM
	{
		public static WM Instance = new WM();

		public RectTransform FindElementPath(string path)
		{
			return WindowManager.FindElementPath(path);
		}


		public void ListChild(string path, int maxDepth)
		{
			var root = WindowManager.FindElementPath(path);
			if (root == null)
			{
				Notification.ShowError($"path '{path}' not found");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine($"Children of {root}\n");

			Action<Transform, int> visitor = null;
			visitor = (node, depth) =>
			{
				for (var i = 0; i < depth - 1; i++)
					sb.Append("--");
				if (depth != 0) // ignore root
					sb.AppendLine(node?.ToString() ?? "  ");
				if (node == null || depth == maxDepth)
					return;

				for (var i = 0; i < node.childCount; i++)
				{
					var child = node.GetChild(i);
					visitor(child, depth + 1);
				}
			};
			visitor(root, 0);

			Notification.ShowError(sb.ToString());
		}

		public WorkItem[] WorkItems()
		{
			return GameSettings.Instance.MyCompany.WorkItems.ToArray();
		}
	}
}
