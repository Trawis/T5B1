using System;
using System.Globalization;

namespace Trainer_v5
{
	public static class Logger
	{
		private static void ConsoleLogWithPropertyName(string str) => DevConsole.Console.Log($"Trainer Property {nameof(str)}: {str}");
		private static void ConsoleLog(string str) => DevConsole.Console.Log(str);

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
		public static void Log(this object str) => ConsoleLogWithPropertyName(str.ToString());
		public static void LogException(this Exception ex) => ConsoleLog($"Trainer Exception: {ex.Message}");
	}
}
