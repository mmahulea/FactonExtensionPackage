namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using FactonExtensionPackage.Modularity;
	using static System.Char;

	public static class Extensions
	{
		private static readonly string BoolReturnText =
"<returns>" + Environment.NewLine +
"\t\t{0}" + Environment.NewLine +
"</returns>";

		private static readonly string Summary =
"<DOC>" + Environment.NewLine +
"<summary>" + Environment.NewLine +
"{0}" + Environment.NewLine +
"</summary>" + Environment.NewLine +
"</DOC>";

		public static string ToBoolReturnText(this string txt)
		{
			return string.Format(BoolReturnText, txt);
		}

		public static string ToReturnText(this string txt)
		{
			if (!txt.EndsWith("."))
			{
				txt += ".";
			}
			return $"/// <returns>{txt}</returns>\r\n";
		}

		public static bool FirstWordIs(this string input, string word)
		{
			return (input.StartsWith(word) && input.Replace(word, "").Length > 0);
		}

		public static string GetArticle(this string input)
		{
			return input.StartsWithAVowel() ? "An" : "A";
		}

		public static string UppercaseFirst(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}
			return ToUpper(input[0]) + input.Substring(1);
		}

		public static string CamelCaseSplit(this string input, string toRemove)
		{
			input = input.Replace(toRemove, string.Empty);
			return Regex.Replace(input.ClearInterfaceName(), @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})", " ", RegexOptions.Compiled).ToLower().Trim();
		}

		public static string CamelCaseSplit(this string input)
		{
			return Regex.Replace(input.ClearInterfaceName(), @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})", " ", RegexOptions.Compiled).ToLower().Trim();
		}

		public static string ToDocComment(this string intput)
		{
			if (string.IsNullOrWhiteSpace(intput))
			{
				return "<DOC></DOC>";
			}
			return string.Format(Summary, intput);
		}

		public static bool StartsWithAVowel(this string intput)
		{
			return "aeiouAEIOU".IndexOf(intput[0]) >= 0;
		}

		public static string TryRemoveFirstChar(this string input, string c)
		{
			if (input.StartsWith(c))
			{
				input = input.Remove(0, 1);
			}
			return input;
		}

		public static string ClearInterfaceName(this string input)
		{
			if (input.Length > 2 && input.StartsWith("I") && input[1].ToString() == input[1].ToString().ToUpper())
			{
				input = input.Remove(0, 1);
			}
			return input;
		}

		public static bool IsEmpty(this string line)
		{
			return string.IsNullOrWhiteSpace(line.Trim());
		}

		public static string Remove(this string txt, string toRemove)
		{
			return txt.Replace(toRemove, string.Empty);
		}

		public static string AddArticle(this string txt)
		{
			if (txt.StartsWithAVowel())
			{
				return "An " + txt;
			}
			return "A " + txt;
		}

		public static string ToCtorNullCheck(this string text)
		{
			var sb = new StringBuilder();
			sb.Append($"if({text} == null)");
			sb.Append(Environment.NewLine);
			sb.Append("{");
			sb.Append(Environment.NewLine);
			sb.Append("\t");
			sb.Append($"throw new ArgumentNullException(\"{text}\");");
			sb.Append(Environment.NewLine);
			sb.Append("}");
			return sb.ToString();
		}

		public static bool EqualsExcludingWhitespace(this string a, string b)
		{
			return a.Where(c => !IsWhiteSpace(c))
			   .SequenceEqual(b.Where(c => !IsWhiteSpace(c)));
		}

		public static IEnumerable<string> Matches(this string txt, string regex)
		{
			return from Match match in new Regex(regex, RegexOptions.Multiline | RegexOptions.Singleline).Matches(txt)
				   select match.Groups[1].Value;
		}

		public static bool CompareAsConfigFile(this string file1, string file2)
		{
			var config1 = file1.Deserialize<XmlModuleConfig>();
			var config2 = file2.Deserialize<XmlModuleConfig>();

			return config1.Equals(config2);
		}
	}
}
