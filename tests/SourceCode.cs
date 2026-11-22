using System.Text;

namespace Trainer_v5.Tests
{
	// Normalises trainer source for the few rules that are about source shape. Line endings are normalised (CI may check
	// out CRLF), comments are removed, and every run of whitespace becomes one space.
	internal static class SourceCode
	{
		public static string Normalize(string source)
		{
			var stripped = SourceText.Strip(source.Replace("\r\n", "\n").Replace('\r', '\n'), keepStrings: true);
			var result = new StringBuilder(stripped.Length);
			var space = false;
			foreach (var c in stripped)
			{
				if (char.IsWhiteSpace(c))
				{
					space = result.Length > 0;
					continue;
				}

				if (space)
				{
					result.Append(' ');
					space = false;
				}
				result.Append(c);
			}
			return result.ToString();
		}
	}
}
