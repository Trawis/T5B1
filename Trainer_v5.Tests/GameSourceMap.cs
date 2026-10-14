using System;
using System.Collections.Generic;

namespace Trainer_v5.Tests
{
	internal sealed class OriginalPosition
	{
		public OriginalPosition(int line, int column, bool exact, string description)
		{
			Line = line;
			Column = column;
			Exact = exact;
			Description = description;
		}

		// 1-based line and column in the file as it exists in the repository.
		public int Line { get; }

		public int Column { get; }

		// True when the injector left that line untouched, so line and column are precise.
		public bool Exact { get; }

		public string Description { get; }
	}

	// Maps a position in the injected source back to the repository file. The injector only adds text
	// (whole lines for the try and catch blocks, and it splits or rejoins lines around a body it wraps),
	// so lines are aligned with a longest-common-subsequence over the injector's own input; lines that
	// were rewritten are matched by prefix, and anything left is code the injector generated.
	internal sealed class GameSourceMap
	{
		private readonly InjectedSource _source;
		private readonly string[] _strippedLines;
		private readonly string[] _injectedLines;
		private int[] _strippedForInjected;
		private bool[] _exact;

		public GameSourceMap(InjectedSource source)
		{
			_source = source;
			_strippedLines = Lines(source.Stripped);
			_injectedLines = Lines(source.Injected);
		}

		public OriginalPosition Locate(int injectedLine, int injectedColumn)
		{
			if (_strippedForInjected == null)
			{
				Align();
			}

			var originalLines = Lines(_source.Original);
			var index = Math.Min(Math.Max(injectedLine, 1), _injectedLines.Length) - 1;
			var stripped = _strippedForInjected[index];
			if (stripped < _source.PrefixLines)
			{
				return new OriginalPosition(1, 1, false, "the game's own #define lines, which it prepends to every file");
			}

			var line = stripped - _source.PrefixLines + 1;
			var text = line <= originalLines.Length ? originalLines[line - 1].Trim() : string.Empty;
			if (_exact[index])
			{
				return new OriginalPosition(line, injectedColumn, true, text);
			}

			return new OriginalPosition(line, 1, false, "near this line (the game rewrote or generated the code at the reported position): " + text);
		}

		private void Align()
		{
			var s = _strippedLines;
			var t = _injectedLines;
			var lengths = new int[s.Length + 1, t.Length + 1];
			for (var i = s.Length - 1; i >= 0; i--)
			{
				for (var j = t.Length - 1; j >= 0; j--)
				{
					lengths[i, j] = Same(s[i], t[j]) ? lengths[i + 1, j + 1] + 1 : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
				}
			}

			_strippedForInjected = new int[t.Length];
			_exact = new bool[t.Length];
			for (var j = 0; j < t.Length; j++)
			{
				_strippedForInjected[j] = -1;
			}

			var gaps = new List<int[]>();
			var a = 0;
			var b = 0;
			var lastS = -1;
			var lastT = -1;
			while (a < s.Length && b < t.Length)
			{
				if (Same(s[a], t[b]) && lengths[a, b] == lengths[a + 1, b + 1] + 1)
				{
					gaps.Add(new[] { lastS + 1, a, lastT + 1, b });
					_strippedForInjected[b] = a;
					_exact[b] = true;
					lastS = a;
					lastT = b;
					a++;
					b++;
				}
				else if (lengths[a + 1, b] >= lengths[a, b + 1])
				{
					a++;
				}
				else
				{
					b++;
				}
			}
			gaps.Add(new[] { lastS + 1, s.Length, lastT + 1, t.Length });

			foreach (var gap in gaps)
			{
				AssignGap(gap[0], gap[1], gap[2], gap[3]);
			}
		}

		// Between two aligned lines: a rewritten line is a prefix of the original it came from; the rest was generated.
		private void AssignGap(int sFrom, int sTo, int tFrom, int tTo)
		{
			var next = sFrom;
			var anchor = sFrom - 1;
			for (var j = tFrom; j < tTo; j++)
			{
				var text = _injectedLines[j].Trim();
				var matched = -1;
				if (text.Length > 0)
				{
					for (var k = next; k < sTo; k++)
					{
						if (_strippedLines[k].Trim().StartsWith(text, StringComparison.Ordinal))
						{
							matched = k;
							break;
						}
					}
				}

				if (matched >= 0)
				{
					_strippedForInjected[j] = matched;
					_exact[j] = false;
					next = matched + 1;
					anchor = matched;
				}
				else
				{
					_strippedForInjected[j] = anchor;
					_exact[j] = false;
				}
			}
		}

		private static bool Same(string a, string b)
		{
			return string.Equals(a.TrimEnd(), b.TrimEnd(), StringComparison.Ordinal);
		}

		private static string[] Lines(string text)
		{
			return text.Split('\n');
		}
	}
}
