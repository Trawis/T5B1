using System;
using System.Globalization;

namespace Trainer_v5
{
	public static class Logger
	{
		public static void Log(this string str, bool withPropertyName = true)
		{
			if (withPropertyName)
			{
				ConsoleLogWithPropertyName(str);
			}
			else
			{
				ConsoleLog(str);
			}
		}

		public static void Log(this bool str) => ConsoleLogWithPropertyName(str.ToString());
		public static void Log(this int str) => ConsoleLogWithPropertyName(str.ToString());
		public static void Log(this float str) => ConsoleLogWithPropertyName(str.ToString(CultureInfo.InvariantCulture));
		public static void Log(this double str) => ConsoleLogWithPropertyName(str.ToString(CultureInfo.InvariantCulture));
		public static void Log(this object str) => ConsoleLogWithPropertyName(FormatValue(str));
		public static void LogException(this Exception ex) => ConsoleLog(FormatException(ex, null));
		public static void LogException(this Exception ex, string featureId) => ConsoleLog(FormatException(ex, featureId));

		// Exception.ToString() carries the type, message, stack trace and the inner exception chain.
		internal static string FormatException(Exception exception, string featureId)
		{
			var details = exception == null ? "null exception" : exception.ToString();

			if (featureId == null)
				return "Trainer Exception: " + details;

			return "Trainer feature '" + featureId + "' failed: " + details;
		}

		internal static string FormatValue(object value)
		{
			if (value == null)
				return "null";

			return value.ToString() ?? "null";
		}

		private static void ConsoleLogWithPropertyName(string str) => DevConsole.Console.Log($"Trainer: {str}");
		private static void ConsoleLog(string str) => DevConsole.Console.Log(str);
	}
}
