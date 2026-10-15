using System;
using System.Text;

namespace Trainer_v5.Tests
{
	// Dependency-free scanner that drops comments and optionally string text; interpolated-string expressions are always kept.
	internal static class SourceText
	{
		public static string Strip(string source, bool keepStrings)
		{
			var result = new StringBuilder(source.Length);
			var i = 0;
			ScanCode(source, ref i, false, keepStrings, result);
			return result.ToString();
		}

		private static void ScanCode(string source, ref int i, bool inHole, bool keepStrings, StringBuilder result)
		{
			var depth = 0;
			while (i < source.Length)
			{
				var c = source[i];
				var next = i + 1 < source.Length ? source[i + 1] : '\0';

				if (c == '/' && next == '/')
				{
					while (i < source.Length && source[i] != '\n')
					{
						i++;
					}
				}
				else if (c == '/' && next == '*')
				{
					var end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
					i = end < 0 ? source.Length : end + 2;
					result.Append(' ');
				}
				else if (c == '"' || c == '\'')
				{
					var prefix = Prefix(source, i);
					var verbatim = c == '"' && prefix.IndexOf('@') >= 0;
					var interpolated = c == '"' && prefix.IndexOf('$') >= 0;
					CopyLiteral(source, ref i, c, verbatim, interpolated, keepStrings, result);
				}
				else if (inHole && c == '{')
				{
					depth++;
					result.Append(c);
					i++;
				}
				else if (inHole && c == '}')
				{
					if (depth == 0)
					{
						return;
					}

					depth--;
					result.Append(c);
					i++;
				}
				else
				{
					result.Append(c);
					i++;
				}
			}
		}

		private static string Prefix(string source, int quote)
		{
			var start = quote;
			while (start > 0 && (source[start - 1] == '$' || source[start - 1] == '@') && quote - start < 2)
			{
				start--;
			}
			return source.Substring(start, quote - start);
		}

		private static void CopyLiteral(string source, ref int i, char quote, bool verbatim, bool interpolated, bool keepContent, StringBuilder result)
		{
			result.Append(quote);
			i++;
			while (i < source.Length)
			{
				var c = source[i];
				var next = i + 1 < source.Length ? source[i + 1] : '\0';

				if (c == quote)
				{
					if (verbatim && next == quote)
					{
						Text(result, keepContent, quote, quote);
						i += 2;
						continue;
					}
					break;
				}

				if (c == '\\' && !verbatim && i + 1 < source.Length)
				{
					Text(result, keepContent, c, next);
					i += 2;
					continue;
				}

				if (interpolated && (c == '{' || c == '}') && next == c)
				{
					Text(result, keepContent, c, c);
					i += 2;
					continue;
				}

				if (interpolated && c == '{')
				{
					result.Append(' ');
					i++;
					ScanCode(source, ref i, true, keepContent, result);
					result.Append(' ');
					if (i < source.Length)
					{
						i++;
					}
					continue;
				}

				Text(result, keepContent, c, '\0');
				i++;
			}

			result.Append(quote);
			i++;
		}

		private static void Text(StringBuilder result, bool keep, char first, char second)
		{
			if (!keep)
			{
				return;
			}

			result.Append(first);
			if (second != '\0')
			{
				result.Append(second);
			}
		}
	}
}
