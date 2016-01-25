namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.FormatingCommands;
	using FactonExtensionPackage.Services;

	public static class CodeElementExtensions
	{
		public static string GetFirstLineAfterElement(this CodeElement codeElement)
		{
			var endPoint = codeElement.EndPoint.CreateEditPoint();
			endPoint.LineDown();
			endPoint.StartOfLine();
			var text = endPoint.GetText(endPoint.LineLength);
			return text;
		}

		public static void AddBlankLineAfterElement(this CodeElement codeElement)
		{
			var endPoint = codeElement.EndPoint.CreateEditPoint();
			TextSelection txtSel = (TextSelection)codeElement.DTE.ActiveDocument.Selection;
			endPoint.LineDown();
			endPoint.StartOfLine();
			txtSel.MoveToPoint(endPoint);
			txtSel.NewLine(1);
		}

		public static bool HasMultipleBlackLinesAfterIt(this CodeElement codeElement)
		{
			var endPoint = codeElement.EndPoint.CreateEditPoint();
			endPoint.LineDown();
			endPoint.StartOfLine();
			string text = endPoint.GetText(endPoint.LineLength); ;
			int count = 0;
			while (String.IsNullOrWhiteSpace(text.Trim()))
			{
				count += 1;
				endPoint.LineDown();
				endPoint.StartOfLine();
				text = endPoint.GetText(endPoint.LineLength); ;
			}
			return count > 1;
		}

		public static string GetDocComment(this CodeElement codeElement)
		{
			switch (codeElement.Kind)
			{
				case vsCMElement.vsCMElementProperty: return (codeElement as CodeProperty).DocComment;
				case vsCMElement.vsCMElementFunction: return (codeElement as CodeFunction).DocComment;
				case vsCMElement.vsCMElementEnum: return (codeElement as CodeEnum).DocComment;
				case vsCMElement.vsCMElementStruct: return (codeElement as CodeStruct).DocComment;
				case vsCMElement.vsCMElementInterface: return (codeElement as CodeInterface).DocComment;
				case vsCMElement.vsCMElementEvent: return (codeElement as CodeEvent).DocComment;
				case vsCMElement.vsCMElementClass: return (codeElement as CodeClass).DocComment;
				case vsCMElement.vsCMElementVariable: return (codeElement as CodeVariable).DocComment;
			}
			return default(string);
		}

		public static string SetDocComment(this CodeElement codeElement, string docComment)
		{
			switch (codeElement.Kind)
			{
				case vsCMElement.vsCMElementProperty: (codeElement as CodeProperty).DocComment = docComment; break;
				case vsCMElement.vsCMElementFunction: (codeElement as CodeFunction).DocComment = docComment; break;
				case vsCMElement.vsCMElementEnum: (codeElement as CodeEnum).DocComment = docComment; break;
				case vsCMElement.vsCMElementStruct: (codeElement as CodeStruct).DocComment = docComment; break;
				case vsCMElement.vsCMElementInterface: (codeElement as CodeInterface).DocComment = docComment; break;
				case vsCMElement.vsCMElementEvent: (codeElement as CodeEvent).DocComment = docComment; break;
				case vsCMElement.vsCMElementClass: (codeElement as CodeClass).DocComment = docComment; break;
				case vsCMElement.vsCMElementVariable: (codeElement as CodeVariable).DocComment = docComment; break;
			}
			return default(string);
		}

		public static bool IsInherited(this CodeElement codeElement)
		{
			if (codeElement is CodeFunction)
			{
				CodeFunction codefuction = (CodeFunction)codeElement;
				if (HasParent(codeElement))
				{
					CodeClass codeClass = codefuction.Parent as CodeClass;
					if (codeClass != null)
					{
						if (CheckIfCodeClassInheritsFunction(codeClass, codefuction))
						{
							return true;
						}
					}
				}
			}
			else if (codeElement is CodeProperty)
			{
				CodeProperty codeProperty = (CodeProperty)codeElement;
				if (HasParent(codeElement))
				{
					CodeClass codeClass = codeProperty.Parent;
					if (codeClass != null)
					{
						if (CheckIfCodeClassInheritsProperty(codeClass, codeProperty))
						{
							return true;
						}
					}
				}
			}
			else if (codeElement is CodeEvent)
			{
				CodeEvent codeEvent = (CodeEvent)codeElement;
				if (HasParent(codeElement))
				{
					CodeClass codeClass = codeEvent.Parent as CodeClass;
					if (codeClass != null)
					{
						if (CheckIfCodeClassInheritsEvent(codeClass, codeEvent))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool HasParent(CodeElement codeElement)
		{
			try
			{
				var codefuction = codeElement as CodeFunction;
				if (codefuction != null) return codefuction.Parent != null;

				var codeProperty = codeElement as CodeProperty;
				if (codeProperty != null) return codeProperty.Parent != null;

				var codeEvent = codeElement as CodeEvent;
				if (codeEvent != null) return codeEvent.Parent != null;

				return false;
			}
			catch (Exception)
			{

				return false;
			}
		}

		private static bool CheckIfCodeClassInheritsFunction(CodeClass codeClass, CodeFunction codeFunction)
		{
			List<CodeInterface> implementedInterfaces = codeClass.GetImplementedInterfaces();
			if (
				SearchService.SearchInCodeElements<CodeFunction>(implementedInterfaces.OfType<CodeElement>())
					.Any(
						p =>
						p.Name == codeFunction.Name && p.Kind == codeFunction.Kind && p.Type.AsFullName == codeFunction.Type.AsFullName
						&& p.Access == codeClass.Access))
			{
				return true;
			}
			return false;
		}

		private static bool CheckIfCodeClassInheritsProperty(CodeClass codeClass, CodeProperty codeProperty)
		{
			List<CodeInterface> implementedInterfaces = codeClass.GetImplementedInterfaces();
			if (
				SearchService.SearchInCodeElements<CodeProperty>(implementedInterfaces.OfType<CodeElement>())
					.Any(
						p =>
						p.Name == codeProperty.Name && p.Kind == codeProperty.Kind && p.Type.AsFullName == codeProperty.Type.AsFullName
						&& p.Access == codeClass.Access))
			{
				return true;
			}
			return false;
		}

		private static bool CheckIfCodeClassInheritsEvent(CodeClass codeClass, CodeEvent codeEvent)
		{
			List<CodeInterface> implementedInterfaces = codeClass.GetImplementedInterfaces();
			if (
				SearchService.SearchInCodeElements<CodeEvent>(implementedInterfaces.OfType<CodeElement>())
					.Any(
						p =>
						p.Name == codeEvent.Name && p.Kind == codeEvent.Kind && p.Type.AsFullName == codeEvent.Type.AsFullName
						&& p.Access == codeClass.Access))
			{
				return true;
			}
			return false;
		}

		public static bool OverridesSomething(this CodeElement codeElement)
		{
			if (codeElement is CodeFunction2)
			{
				return (codeElement as CodeFunction2).OverrideKind == vsCMOverrideKind.vsCMOverrideKindOverride
					|| (codeElement as CodeFunction2).OverrideKind == (vsCMOverrideKind.vsCMOverrideKindOverride | vsCMOverrideKind.vsCMOverrideKindSealed);
			}

			if (codeElement is CodeProperty2)
			{
				return (codeElement as CodeProperty2).OverrideKind == vsCMOverrideKind.vsCMOverrideKindOverride
					|| (codeElement as CodeProperty2).OverrideKind == (vsCMOverrideKind.vsCMOverrideKindOverride | vsCMOverrideKind.vsCMOverrideKindSealed);
			}

			return false;
		}

		public static void Delete(this CodeElement codeElement)
		{
			var startPoint = codeElement.StartPoint.CreateEditPoint();
			var endPoint = codeElement.EndPoint.CreateEditPoint();
			endPoint.LineDown();
			endPoint.StartOfLine();
			startPoint.Delete(endPoint);
		}

		public static string InnerText(this CodeElement codeElement)
		{
			return codeElement.GetStartPoint().CreateEditPoint().GetText(codeElement.EndPoint);
		}

		public static EditPoint AtTheFirstLineAfterTheOpeningBrakect(this CodeElement codeElement)
		{
			var editPoint = codeElement.GetStartPoint().CreateEditPoint();
			while (editPoint.GetLineText().Trim() != "{")
			{
				editPoint.LineDown();
			}

			editPoint.StartOfLine();
			return editPoint;
		}

		public static bool Contains(this CodeElement codeElement, string text)
		{
			return codeElement.InnerText().ToLower().Contains(text.ToLower());
		}

		public static void AppendToDocComment(this CodeElement codeElement, string text)
		{
			var docComment = codeElement.GetDocComment();
			docComment = docComment.Replace("<DOC>", string.Empty);
			docComment = docComment.Replace("<doc>", string.Empty);
			docComment = docComment.Replace("</DOC>", string.Empty);
			docComment = docComment.Replace("</doc>", string.Empty);
			docComment = "<doc>" + Environment.NewLine + docComment.Trim() + Environment.NewLine + text + Environment.NewLine + "</doc>";
			codeElement.SetDocComment(docComment);
		}
	}
}
