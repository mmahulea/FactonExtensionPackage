namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;

	public static class CodeFunctionExtensions
	{
		public static string GetTextFromTheFirstLineAbove(this CodeFunction codeElement)
		{
			var editPpoint = codeElement.StartPoint.CreateEditPoint();
			editPpoint.LineUp();
			return editPpoint.GetLineText();
		}
		
		public static void RemoveFirstLineAbove(this CodeFunction codeElement)
		{
			var editPpoint = codeElement.StartPoint.CreateEditPoint();
			editPpoint.StartOfLine();
			var mark = editPpoint.CreateEditPoint();

			editPpoint.LineUp();
			editPpoint.StartOfLine();
			editPpoint.Delete(mark);
		}

		public static bool HasAParameterNamed(this CodeFunction codeFunction, string name)
		{
			foreach (CodeElement parameter in codeFunction.Parameters)
			{
				if (parameter.Name == name)
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasReturnType(this CodeFunction codeFunction, string type)
		{
			string returnType = codeFunction.Type.CodeType.Name.ToLower();
			return type.ToLower() == returnType;
		}

		public static bool HasName(this CodeFunction codeFunction, string name)
		{
			return codeFunction.Name == name;
		}

		public static bool HasTwoParameters(this CodeFunction codeFunction)
		{
			return codeFunction.Parameters.Count == 2;
		}

		public static bool HasParameterCount(this CodeFunction codeFunction, int count)
		{
			return codeFunction.Parameters.Count == count;
		}

		public static bool HasParametertsOfType(this CodeFunction codeFunction, string type)
		{
			return codeFunction.Parameters.Cast<CodeParameter>()
				.All(parameter => parameter.Type.CodeType.Name.ToLower() == type.ToLower());
		}

		public static bool NameStartsWith(this CodeFunction codeFunction, string name)
		{
			return codeFunction.Name != name && codeFunction.Name.StartsWith(name);
		}

		public static bool NameStartsWithAVerb(this CodeFunction codeFunction, List<Tuple<string, string>> verbs)
		{
			return verbs.Any(v => codeFunction.NameStartsWith(v.Item1));
		}

		public static bool StartsWithAVerb(this string text, List<Tuple<string, string>> verbs)
		{
			return verbs.Any(v => text.StartsWith(v.Item1));
		}

		public static string GetParameterName(this CodeFunction codeFunction, int index)
		{
			int i = 1;
			foreach (CodeElement parameter in codeFunction.Parameters)
			{
				if (i == index)
				{
					return parameter.Name;
				}
				i += 1;

			}
			return null;
		}

		public static string InnerText(this CodeFunction codeElement)
		{
			return (codeElement as CodeElement).InnerText();
		}

		public static IEnumerable<CodeParameter> Parameters(this CodeFunction codeFunction)
		{
			return codeFunction.Parameters.OfType<CodeParameter>();			
		}
	}
}