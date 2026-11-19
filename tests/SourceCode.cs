using System;
using System.IO;
using System.Text;
using Xunit;

namespace Trainer_v5.Tests
{
	// Reads trainer source for the few rules that are about source shape. Line endings are normalised (CI may check out
	// CRLF), comments are removed, and every run of whitespace becomes one space, so a test states what the code says and
	// not how it is laid out. A missing marker fails with its name instead of an ArgumentOutOfRangeException.
	internal static class SourceCode
	{
		public static string Read(params string[] pathInSourceDirectory)
		{
			var path = RepositoryFiles.SourceDirectory;
			foreach (var part in pathInSourceDirectory)
			{
				path = Path.Combine(path, part);
			}

			return Normalize(System.IO.File.ReadAllText(path));
		}

		// The text from startMarker up to (not including) endMarker, or to the end of the file when endMarker is null.
		// Markers are written the way Normalize leaves the code: one space between tokens.
		public static string Span(string source, string startMarker, string endMarker = null)
		{
			var start = source.IndexOf(startMarker, StringComparison.Ordinal);
			Assert.True(start >= 0, "Marker not found in the source: \"" + startMarker + "\". It was renamed or removed; update the test with it.");
			if (endMarker == null)
			{
				return source.Substring(start);
			}

			var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
			Assert.True(end >= 0, "End marker not found after \"" + startMarker + "\": \"" + endMarker + "\". It was renamed or removed; update the test with it.");
			return source.Substring(start, end - start);
		}

		// The member that starts at the marker: its signature and its braced body up to the matching closing brace,
		// however the surrounding members are ordered or formatted.
		public static string Body(string source, string signatureMarker)
		{
			var start = source.IndexOf(signatureMarker, StringComparison.Ordinal);
			Assert.True(start >= 0, "Marker not found in the source: \"" + signatureMarker + "\". It was renamed or removed; update the test with it.");
			var open = source.IndexOf('{', start);
			Assert.True(open >= 0, "\"" + signatureMarker + "\" has no braced body.");

			var depth = 0;
			for (var i = open; i < source.Length; i++)
			{
				if (source[i] == '{')
				{
					depth++;
				}
				else if (source[i] == '}' && --depth == 0)
				{
					return source.Substring(start, i + 1 - start);
				}
			}

			throw new Xunit.Sdk.XunitException("The body of \"" + signatureMarker + "\" is not closed.");
		}

		// Passes when both texts occur and the first occurrence of the first comes before the first occurrence of the second,
		// so a later repeat of the second cannot hide a wrong order.
		public static void AssertInOrder(string source, string first, string second, string because)
		{
			var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
			Assert.True(firstIndex >= 0, "Not found: \"" + first + "\" (" + because + ")");
			var secondIndex = source.IndexOf(second, StringComparison.Ordinal);
			Assert.True(secondIndex >= 0, "Not found: \"" + second + "\" (" + because + ")");
			Assert.True(firstIndex < secondIndex, "\"" + first + "\" must come before \"" + second + "\" (" + because + ")");
		}

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
