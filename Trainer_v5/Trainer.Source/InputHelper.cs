using System;
using System.Globalization;

namespace Trainer_v5
{
	public static class InputHelper
	{
		public static void RequestFloat(
			string prompt,
			string title,
			Action<float> onFinish,
			float min,
			float max)
		{
			WindowManager.SpawnInputDialog(prompt, title,
				string.Empty,
				input =>
				{
					var val = TryParseAndValidate(input, ParseFloat, min, max);
					if (val.HasValue)
						onFinish.Invoke(val.Value);
				});
		}

		public static void RequestFloat(
			string prompt,
			string title,
			Action<float> onFinish)
		{
			WindowManager.SpawnInputDialog(prompt, title, string.Empty,
				input =>
				{
					var val = TryParseAndValidate<float>(input, ParseFloat);
					if (val != null)
						onFinish.Invoke(val.Value);
				});
		}

		public static void RequestInt(
			string prompt,
			string title,
			string defaultValue,
			Action<int> onFinish,
			int min,
			int max)
		{
			WindowManager.SpawnInputDialog(prompt, title,
				defaultValue,
				input =>
				{
					var val = TryParseAndValidate(input, ParseInt, min, max);
					if (val.HasValue)
						onFinish.Invoke(val.Value);
				});
		}

		public static void RequestInt(
			string prompt,
			string title,
			string defaultValue,
			Action<int> onFinish)
		{
			WindowManager.SpawnInputDialog(prompt, title, defaultValue,
				input =>
				{
					var val = TryParseAndValidate<int>(input, ParseInt);
					if (val != null)
						onFinish.Invoke(val.Value);
				});
		}

		public static void RequestUInt(
			string prompt,
			string title,
			string defaultValue,
			Action<uint> onFinish,
			uint min,
			uint max)
		{
			WindowManager.SpawnInputDialog(prompt, title,
				defaultValue,
				input =>
				{
					var val = TryParseAndValidate(input, ParseUInt, min, max);
					if (val.HasValue)
						onFinish.Invoke(val.Value);
				});
		}

		public static void RequestUInt(
			string prompt,
			string title,
			string defaultValue,
			Action<uint> onFinish)
		{
			WindowManager.SpawnInputDialog(prompt, title, defaultValue,
				input =>
				{
					var val = TryParseAndValidate<uint>(input, ParseUInt);
					if (val != null)
						onFinish.Invoke(val.Value);
				});
		}

		internal delegate bool TryParse<T>(string s, out T result) where T : struct;

		internal static bool ParseFloat(string s, out float result)
		{
			return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}

		internal static bool ParseInt(string s, out int result)
		{
			return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}

		internal static bool ParseUInt(string s, out uint result)
		{
			return uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}

		internal static InputError Validate<T>(
			string str,
			TryParse<T> tryParse,
			out T val
			) where T : struct, IComparable<T>
		{
			if (str.Length == 0 || !tryParse(str, out val))
			{
				val = default(T);
				return InputError.Invalid;
			}

			if (typeof(T) == typeof(float))
			{
				var f = (float)(object)val;
				if (float.IsNaN(f) || float.IsInfinity(f))
				{
					return InputError.Invalid;
				}
			}

			return InputError.None;
		}

		internal static InputError Validate<T>(
			string str,
			TryParse<T> tryParse,
			T min,
			T max,
			out T val
			) where T : struct, IComparable<T>
		{
			InputError error = Validate(str, tryParse, out val);
			if (error != InputError.None)
			{
				return error;
			}

			if (val.CompareTo(min) < 0)
			{
				return InputError.BelowMin;
			}

			if (val.CompareTo(max) > 0)
			{
				return InputError.AboveMax;
			}

			return InputError.None;
		}

		private static string ErrorText(InputError error, object min, object max)
		{
			if (error == InputError.BelowMin)
				return string.Format("InvalidInputMin".LocDef("Invalid input!\nmin value is {0}"), min);

			if (error == InputError.AboveMax)
				return string.Format("InvalidInputMax".LocDef("Invalid input!\nmax value is {0}"), max);

			return "InvalidInput".LocDef("Invalid input!");
		}

		private static T? TryParseAndValidate<T>(
			string str,
			TryParse<T> tryParse
			) where T : struct, IComparable<T>
		{
			T val;
			if (Validate(str, tryParse, out val) != InputError.None)
			{
				Notification.ShowError(ErrorText(InputError.Invalid, null, null));
				return null;
			}

			return val;
		}

		private static T? TryParseAndValidate<T>(
			string str,
			TryParse<T> tryParse,
			T min,
			T max
			) where T : struct, IComparable<T>
		{
			T val;
			InputError error = Validate(str, tryParse, min, max, out val);
			if (error != InputError.None)
			{
				Notification.ShowError(ErrorText(error, min, max));
				return null;
			}

			return val;
		}
	}
}
