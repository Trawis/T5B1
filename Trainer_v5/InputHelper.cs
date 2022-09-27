using LibTessDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// ReSharper disable once InconsistentNaming
namespace Trainer_v5
{
	public static class InputHelper
	{
		public static void RequestFloat(
			string prompt, 
			string title,
			Action<float> onFinish,
			float? @default = null,
			float? min = null,
			float? max = null)
		{
			WindowManager.SpawnInputDialog(prompt, title, 
				@default == null ? "" : @default.ToString(),
				input =>
				{
					var val = TryParseAndValidate(input, float.TryParse, min, max);
					if (val != null)
						onFinish.Invoke(val.Value);
				});
		}
		

		

		private delegate bool TryParse<T>(string s, out T result) where T : struct;


		private static T? TryParseAndValidate<T>(
			string str,
			TryParse<T> tryParse,
			T? min = null,
			T? max = null
			) where T : struct, IComparable<T>
		{
			// parse string
			T val;
			if (str.Length == 0 || !tryParse(str, out val))
			{
				Notification.ShowError("Invalid input!");
				return null;
			}

			if (min != null && val.CompareTo(min.Value) < 0)
			{
				Notification.ShowError("Invalid input!\nmin value is " + min.Value);
				return null;
			}

			if (max != null && val.CompareTo(max.Value) > 0)
			{
				Notification.ShowError("Invalid input!\nmax value is " + max.Value);
				return null;
			}

			return val;
		} 
	}
}
