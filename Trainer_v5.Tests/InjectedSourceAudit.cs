using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Trainer_v5.Tests
{
	internal sealed class AuditResult
	{
		public AuditResult(int tryBlocks, IList<string> misplaced)
		{
			TryBlocks = tryBlocks;
			Misplaced = misplaced;
		}

		public int TryBlocks { get; }

		public IList<string> Misplaced { get; }
	}

	// Text-level checks on the injector's output and on source shapes its compiler mishandles.
	internal static class InjectedSourceAudit
	{
		private static readonly Regex InjectedOpen = new Regex(@"\{[ \t]*\r?\n[ \t]*try \{[ \t]*\r?\n", RegexOptions.Compiled);
		private static readonly Regex InjectedCatch = new Regex(@"catch\(ModException\) \{throw;\}", RegexOptions.Compiled);
		private static readonly Regex MemberHeader = new Regex(@"\)\s*(:\s*(base|this)\s*\(.*\))?\s*(where\s.*)?$", RegexOptions.Compiled);
		private static readonly Regex ContainerHeader = new Regex(@"\b(class|struct|interface|enum|namespace)\b", RegexOptions.Compiled);
		private static readonly Regex Return = new Regex(@"\breturn\b", RegexOptions.Compiled);
		private static readonly Regex Throw = new Regex(@"^throw\b", RegexOptions.Compiled);

		// Every injected try must follow the opening brace of a method or constructor (#222).
		public static AuditResult Audit(InjectedSource source)
		{
			var misplaced = new List<string>();
			var map = new GameSourceMap(source);
			foreach (Match match in InjectedOpen.Matches(source.Injected))
			{
				var before = source.Injected.Substring(0, match.Index);
				var header = Regex.Replace(before.Substring(before.LastIndexOfAny(new[] { ';', '{', '}' }) + 1), @"\s+", " ").Trim();
				if (!MemberHeader.IsMatch(header))
				{
					var line = before.Count(c => c == '\n') + 1;
					var original = map.Locate(line, 1);
					misplaced.Add(source.Name + "(" + original.Line + "): injected try block after `" + Shorten(header) + "`, which is not a method or constructor");
				}
			}
			return new AuditResult(InjectedCatch.Matches(source.Injected).Count, misplaced);
		}

		// Bodies with no return that end in a throw compile to a branch past the method end (invalid IL) once wrapped.
		public static IList<string> FindBodiesEndingInThrow(string name, string original)
		{
			var code = SourceText.Strip(original, keepStrings: false);
			var found = new List<string>();
			foreach (var body in MemberBodies(code))
			{
				if (!Return.IsMatch(body.Body) && Throw.IsMatch(LastStatement(body.Body)))
				{
					found.Add(name + ": " + Shorten(body.Header));
				}
			}
			return found;
		}

		private sealed class MemberBody
		{
			public string Header;
			public string Body;
		}

		private static IEnumerable<MemberBody> MemberBodies(string code)
		{
			var kinds = new Stack<char>();
			var opens = new Stack<int>();
			var headers = new Stack<string>();
			var headerStart = 0;
			for (var i = 0; i < code.Length; i++)
			{
				var c = code[i];
				if (c == '{')
				{
					var header = Regex.Replace(code.Substring(headerStart, i - headerStart), @"\s+", " ").Trim();
					var parent = kinds.Count == 0 ? 'c' : kinds.Peek();
					char kind;
					if (parent == 'c' && ContainerHeader.IsMatch(header))
					{
						kind = 'c';
					}
					else if (parent == 'c' && MemberHeader.IsMatch(header))
					{
						kind = 'm';
					}
					else
					{
						kind = 'o';
					}

					kinds.Push(kind);
					opens.Push(i);
					headers.Push(header);
					headerStart = i + 1;
				}
				else if (c == '}')
				{
					if (kinds.Count > 0)
					{
						var kind = kinds.Pop();
						var open = opens.Pop();
						var header = headers.Pop();
						if (kind == 'm')
						{
							yield return new MemberBody { Header = header, Body = code.Substring(open + 1, i - open - 1) };
						}
					}
					headerStart = i + 1;
				}
				else if (c == ';')
				{
					headerStart = i + 1;
				}
			}
		}

		private static string LastStatement(string body)
		{
			var text = body.TrimEnd();
			if (text.Length == 0)
			{
				return string.Empty;
			}

			var depth = 0;
			var boundaries = new List<int>();
			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] == '{')
				{
					depth++;
				}
				else if (text[i] == '}')
				{
					depth--;
					if (depth == 0)
					{
						boundaries.Add(i);
					}
				}
				else if (text[i] == ';' && depth == 0)
				{
					boundaries.Add(i);
				}
			}

			var last = text.Length - 1;
			if (boundaries.Count == 0 || boundaries[boundaries.Count - 1] != last)
			{
				return text.Trim();
			}

			var previous = boundaries.Count > 1 ? boundaries[boundaries.Count - 2] : -1;
			return text.Substring(previous + 1, last - previous).Trim();
		}

		private static string Shorten(string text)
		{
			return text.Length <= 100 ? text : text.Substring(text.Length - 100);
		}
	}
}
