using System.Collections.Generic;
using System.Linq;
using Trainer_v5.Trainer.Source.SDK;
using UnityEngine;

namespace Trainer_v5.SDK
{
	public static class WindowHelper
	{
		#region GUIWindow extensions

		public static void SetMinSize(this GUIWindow self, int width, int height)
		{
			self.MinSize.x = width;
			self.MinSize.y = height;
		}

		public static void Add(this GUIWindow self, Component com, Rect position)
		{
			WindowManager.AddElementToWindow(com.gameObject, self, position, new Rect(0, 0, 0, 0));
		}

		public static void Add(this GUIWindow self, GameObject go, Rect position)
		{
			WindowManager.AddElementToWindow(go, self, position, new Rect(0, 0, 0, 0));
		}

		public static void Add(this GUIWindow self, VerticalLayout layout, Rect position)
		{
			var components = layout.Components;
			var gap = layout.Gap;
			var anchors = new Rect(0, 0, 0, 0);
			float x = position.x, nextY = position.y, width = position.width;

			foreach (var component in components)
			{
				var style = component.GetStyle();
				var height = style.DefaultHeight;
				WindowManager.AddElementToWindow(component.gameObject, self, new Rect(x, nextY, width, height), anchors);
				nextY += height + gap;
			}
		}

		#endregion
	}

	public static class LayoutHelper
	{
		public static IEnumerable<Component> EnumerableOf(params object[] children)
		{
			foreach (var child in children)
			{
				if (child is Component)
					yield return (Component)child;
				else if (child is Component[])
					foreach (var component in (Component[])child)
						yield return component;
			}
		}
	}

	public class VerticalLayout
	{
		public List<Component> Components;
		public int Gap = 2;

		public int PreferHeight
		{
			get
			{
				if (Components == null) return 0;
				var height = Components.Select(component => component.GetStyle().DefaultHeight + Gap).Sum();
				return height - Gap;
			}
		}
	}
}