using System.Text;

namespace Trainer_v5.Tests
{
	// Dependency-free scanner for architecture tests: drops comments and, optionally, string
	// contents, so a name mentioned in a comment or text is not mistaken for a code reference.
	internal static class SourceText
	{
		public static string Strip(string source, bool keepStrings)
		{
			var result = new StringBuilder(source.Length);
			var i = 0;
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
					var end = source.IndexOf("*/", i + 2, System.StringComparison.Ordinal);
					i = end < 0 ? source.Length : end + 2;
					result.Append(' ');
				}
				else if (c == '"' || c == '\'')
				{
					var verbatim = c == '"' && i > 0 && (source[i - 1] == '@' || (i > 1 && source[i - 1] == '$' && source[i - 2] == '@'));
					i = CopyLiteral(source, i, c, verbatim, keepStrings, result);
				}
				else
				{
					result.Append(c);
					i++;
				}
			}
			return result.ToString();
		}

		private static int CopyLiteral(string source, int start, char quote, bool verbatim, bool keepContent, StringBuilder result)
		{
			result.Append(quote);
			var i = start + 1;
			while (i < source.Length)
			{
				var c = source[i];
				if (c == quote)
				{
					if (verbatim && i + 1 < source.Length && source[i + 1] == quote)
					{
						if (keepContent)
						{
							result.Append(quote).Append(quote);
						}
						i += 2;
						continue;
					}
					break;
				}

				if (c == '\\' && !verbatim && i + 1 < source.Length)
				{
					if (keepContent)
					{
						result.Append(c).Append(source[i + 1]);
					}
					i += 2;
					continue;
				}

				if (keepContent)
				{
					result.Append(c);
				}
				i++;
			}

			result.Append(quote);
			return i + 1;
		}
	}
}
