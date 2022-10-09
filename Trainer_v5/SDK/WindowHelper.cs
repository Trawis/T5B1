using UnityEngine;

namespace Trainer_v5.SDK
{
	public static class WindowHelper
	{
		#region GUIWindow extensions

		public static void Add(this GUIWindow self, Component com, Rect position)
		{
			WindowManager.AddElementToWindow(com.gameObject, self, position, new Rect(0, 0, 0, 0));
		}

		public static void Add(this GUIWindow self, GameObject go, Rect position)
		{
			WindowManager.AddElementToWindow(go, self, position, new Rect(0, 0, 0, 0));
		}

		#endregion
	}
}