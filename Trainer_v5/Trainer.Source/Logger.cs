﻿using System;
using System.Globalization;

namespace Trainer_v5
{
	public static class Logger
	{
		// Simplified logging prefix
		private const string LogPrefix = "Trainer: ";

		// Simplified console log method
		private static void ConsoleLog(string message) => DevConsole.Console.Log($"{LogPrefix}{message}");

		// Updated Log extension for string (removed withPropertyName)
		public static void Log(this string message)
		{
			ConsoleLog(message);
		}
		
		// Overload for compatibility if Log(message, false) was used elsewhere, though unlikely now.
        // Consider removing if no external calls use the boolean flag.
        [Obsolete("Use Log(message) instead. The withPropertyName flag is deprecated.")]
        public static void Log(this string message, bool withPropertyName)
        {
             ConsoleLog(message); // Ignore the flag, just log the message
        }


		// Updated Log extensions for other types
		public static void Log(this bool value) => ConsoleLog(value.ToString());
		public static void Log(this int value) => ConsoleLog(value.ToString());
		public static void Log(this float value) => ConsoleLog(value.ToString(CultureInfo.InvariantCulture));
		public static void Log(this double value) => ConsoleLog(value.ToString(CultureInfo.InvariantCulture));
		public static void Log(this object obj) => ConsoleLog(obj?.ToString() ?? "null"); // Added null check

		// Enhanced LogException to include type and stack trace
		public static void LogException(this Exception ex)
		{
			if (ex == null)
			{
				ConsoleLog("LogException called with null exception.");
				return;
			}
			// Log type, message, and stack trace for better debugging
			ConsoleLog($"Exception Type: {ex.GetType().FullName}");
			ConsoleLog($"Message: {ex.Message}");
			ConsoleLog($"Stack Trace: {ex.StackTrace}");
			// Optionally log inner exception if it exists
			if (ex.InnerException != null)
			{
				ConsoleLog("--- Inner Exception ---");
				ex.InnerException.LogException(); // Recursively log inner exception
				ConsoleLog("--- End Inner Exception ---");
			}
		}
	}
}
