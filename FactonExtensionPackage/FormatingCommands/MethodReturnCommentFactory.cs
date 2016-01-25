namespace FactonExtensionPackage.FormatingCommands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;

	public static class MethodReturnCommentFactory
	{
		private static readonly List<Tuple<string, string>> Verbs = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Add", "Added"),
			new Tuple<string, string>("Adjust", "Adjusted"),
			new Tuple<string, string>("Build", "Built"),
			new Tuple<string, string>("Create", "Created"),
			new Tuple<string, string>("Determine", "Determined"),
			new Tuple<string, string>("Insert", "Inserted")
		};

		public static string CreateCommnent(CodeFunction codeFunction)
		{
			if (codeFunction.HasReturnType("boolean")
				&& codeFunction.HasName("AreEqual")
				&& codeFunction.HasTwoParameters()
				&& codeFunction.HasParametertsOfType("IPropertyValueProvider"))
			{
				return "<c>true</c> if the two entities are equal; otherwise, <c>false</c>".ToBoolReturnText();
			}

			if (codeFunction.HasReturnType("boolean")
				&& codeFunction.NameStartsWith("Try")
				&& codeFunction.Name.Remove("Try").StartsWithAVerb(Verbs))
			{
				string name = codeFunction.Name.Remove("Try");
				var verb = Verbs.First(v => name.StartsWith(v.Item1));
				return $"<c>true</c>, if the {name.CamelCaseSplit()} could be {verb.Item2}; <c>false</c>, otherwise.".ToBoolReturnText();
			}

			if (codeFunction.HasReturnType("boolean")
				&& codeFunction.Name == "Contains"
				&& codeFunction.HasParameterCount(1))
			{
				string paramName = codeFunction.GetParameterName(1);
				return $"<c>true</c> if this instance contains the specified {paramName.CamelCaseSplit()}; otherwise, <c>false</c>";
			}

			if (codeFunction.NameStartsWith("TryGet")
				&& codeFunction.HasReturnType("boolean"))
			{
				return
					$"<c>true</c> if getting the {codeFunction.Name.CamelCaseSplit("TryGet")} was successful; otherwise, <c>false</c>"
						.ToBoolReturnText();
			}

			if (codeFunction.NameStartsWith("Get"))
			{
				return ("The " + codeFunction.Name.CamelCaseSplit("Get")).ToReturnText();
			}

			if (codeFunction.NameStartsWith("Can")
				&& codeFunction.HasReturnType("boolean"))
			{
				return
					$"<c>true</c> if this instance {codeFunction.Name.CamelCaseSplit()}; otherwise, <c>false</c>".ToBoolReturnText();
			}

			if (codeFunction.NameStartsWithAVerb(Verbs))
			{
				var verb = Verbs.First(v => codeFunction.NameStartsWith(v.Item1));
				return ((verb.Item2.ToLower() + " " + codeFunction.Name.Remove(verb.Item1).CamelCaseSplit()).AddArticle()).ToReturnText();
			}


			if (codeFunction.HasReturnType("boolean"))
			{
				return "<c>true</c> if ..........; otherwise, <c>false</c>".ToBoolReturnText();
			}
			return null;
		}
	}

}
