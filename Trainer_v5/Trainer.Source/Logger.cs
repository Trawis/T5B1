﻿﻿﻿using System;
using System.Globalization;

namespace Trainer_v5
{
	public static class Logger
	{
		private const string LogPrefix = "Trainer: ";

		private static void ConsoleLog(string message) => DevConsole.Console.Log($"{LogPrefix}{message}");

		public static void Log(this string message)
		{
			ConsoleLog(message);
		}

		public static void Log(this bool value) => ConsoleLog(value.ToString());
		public static void Log(this int value) => ConsoleLog(value.ToString());
		public static void Log(this float value) => ConsoleLog(value.ToString(CultureInfo.InvariantCulture));
		public static void Log(this double value) => ConsoleLog(value.ToString(CultureInfo.InvariantCulture));
		public static void Log(this object obj) => ConsoleLog(obj?.ToString() ?? "null");

		public static void LogException(this Exception ex)
		{
			if (ex == null)
			{
				ConsoleLog("LogException called with null exception.");
				return;
			}
			ConsoleLog($"Exception Type: {ex.GetType().FullName}");
			ConsoleLog($"Message: {ex.Message}");
			ConsoleLog($"Stack Trace: {ex.StackTrace}");
			if (ex.InnerException != null)
			{
				ConsoleLog("--- Inner Exception ---");
				ex.InnerException.LogException();
				ConsoleLog("--- End Inner Exception ---");
			}
		}
	}
}
