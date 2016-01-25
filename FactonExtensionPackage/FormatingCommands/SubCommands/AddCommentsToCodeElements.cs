namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class AddCommentsToCodeElements
	{
		private static readonly string exceptionMessage1 = "<exception cref=\"System.ArgumentNullException\">If any parameter is <c>null</c>.</exception>";

		private static readonly string exceptionMessage2 = "<exception cref=\"ArgumentNullException\">If any parameter is <c>null</c>.</exception>";

		private static readonly List<vsCMElement> CodeElementsWithChildren = new List<vsCMElement> {
			vsCMElement.vsCMElementClass,
			vsCMElement.vsCMElementEnum,
			vsCMElement.vsCMElementEvent,
			vsCMElement.vsCMElementInterface,
			vsCMElement.vsCMElementStruct,
			vsCMElement.vsCMElementNamespace,
			vsCMElement.vsCMElementVariable
		};

		private static readonly List<vsCMElement> CodeElementsThatNeedDocComment = new List<vsCMElement> {
			vsCMElement.vsCMElementClass,
			vsCMElement.vsCMElementEnum,
			vsCMElement.vsCMElementEvent,
			vsCMElement.vsCMElementFunction,
			vsCMElement.vsCMElementInterface,
			vsCMElement.vsCMElementProperty,
			vsCMElement.vsCMElementVariable,
			vsCMElement.vsCMElementStruct
		};

		private static bool usingSystemFound;

		public static void Execute(ProjectItem projectItem)
		{
			TextSelection txtSel = (TextSelection)projectItem.Document.Selection;
			usingSystemFound = false;
			txtSel.StartOfDocument();
			if (txtSel.FindText("using System;"))
			{
				usingSystemFound = true;
			}

			foreach (var codeElement in projectItem.FileCodeModel.CodeElements)
			{
				if ((codeElement as CodeElement) != null)
				{
					AddCommentsTo(codeElement as CodeElement);
				}

			}
		}

		private static void AddCommentsTo(CodeElement codeElement)
		{
			if (CodeElementsThatNeedDocComment.Contains(codeElement.Kind))
			{
				if (string.IsNullOrWhiteSpace(codeElement.GetDocComment()))
				{
					if (codeElement.IsInherited() || codeElement.OverridesSomething())
					{
						codeElement.SetDocComment(@"<DOC><inheritdoc/></DOC>");
					}
					else
					{
						switch (codeElement.Kind)
						{
							case vsCMElement.vsCMElementProperty: AddDocCommentToProperty(codeElement as CodeProperty); break;
							case vsCMElement.vsCMElementFunction: AddDocCommentToFunction(codeElement as CodeFunction); break;
							case vsCMElement.vsCMElementEnum: AddGhostDogCommand(codeElement); break;
							case vsCMElement.vsCMElementStruct: AddGhostDogCommand(codeElement); break;
							case vsCMElement.vsCMElementInterface: AddDocCommentToInterface(codeElement as CodeInterface); break;
							case vsCMElement.vsCMElementEvent: AddGhostDogCommand(codeElement); break;
							case vsCMElement.vsCMElementClass: AddDocCommentToClass(codeElement as CodeClass); break;
							case vsCMElement.vsCMElementVariable: AddGhostDogCommand(codeElement); break;
						}
					}
				}
			}

			if (CodeElementsWithChildren.Contains(codeElement.Kind))
			{
				foreach (CodeElement child in codeElement.Children)
				{
					AddCommentsTo(child);
				}
			}
		}

		private static void AddDocCommentToFunction(CodeFunction codeFunction)
		{

			if (codeFunction.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
			{
				AddDocCommentToCtor(codeFunction);
			}
			else if (IsInitializeMethodOfAModule(codeFunction))
			{
				AddDocCommentToInitializeMethodOfAModule(codeFunction);
			}
			else
			{
				AddGhostDogCommand(codeFunction as CodeElement);

				if (codeFunction.Type.CodeType.Name != "Void" && codeFunction.DocComment.Contains("<returns></returns>"))
				{
					string newReturnText = MethodReturnCommentFactory.CreateCommnent(codeFunction);
					if (!string.IsNullOrWhiteSpace(newReturnText))
					{
						string newDocComment = codeFunction.DocComment;
						newDocComment = newDocComment.Replace(@"<returns></returns>", newReturnText);
						codeFunction.DocComment = newDocComment;
					}
				}
			}
		}

		private static void AddDocCommentToInitializeMethodOfAModule(CodeFunction codeFunction)
		{
			var sb = new StringBuilder();
			sb.AppendLine("<DOC>");
			sb.AppendLine("<summary>");
			sb.AppendLine("This method is called, when the module shall initialize itself.");
			sb.AppendLine("</summary>");
			sb.AppendLine("<param name=\"typeRegistry\">The type registry for service requests or registrations.</param>");
			sb.AppendLine("<remarks>");
			sb.AppendLine("It's not guaranteed that the Initialize Method is called in the UI Thread!");
			sb.AppendLine("</remarks>");
			sb.AppendLine("<exception cref=\"Facton.Infrastructure.Modularity.Exceptions.ModuleInitializationException\">");
			sb.AppendLine("If something goes wrong while initializing the module.");
			sb.AppendLine("</exception>");
			sb.AppendLine("</DOC>");
			codeFunction.DocComment = sb.ToString();
		}

		private static void AddDocCommentToCtor(CodeFunction codeFunction)
		{
			AddGhostDogCommand(codeFunction as CodeElement);
			var docComment = codeFunction.DocComment;
			var text = @"<exception cref=""System.ArgumentNullException";
			if (docComment.Contains(text))
			{
				string replaceText = usingSystemFound ? exceptionMessage2 : exceptionMessage1;
				var index1 = docComment.IndexOf(text);
				var index2 = docComment.IndexOf("</exception>") + 12;
				var substring = docComment.Substring(index1, index2 - index1);
				docComment = docComment.Replace(substring, replaceText);
				codeFunction.DocComment = docComment;
			}
			//else
			//{
			//	text = @"<exception cref=""ArgumentNullException";
			//	if (docComment.Contains(text))
			//	{
			//		string replaceText = usingSystemFound ? exceptionMessage2 : exceptionMessage1;
			//		var index1 = docComment.IndexOf(text);
			//		var index2 = docComment.IndexOf("</exception>") + 12;
			//		var substring = docComment.Substring(index1, index2 - index1);
			//		docComment = docComment.Replace(substring, replaceText);
			//	}
			//}

			//var sb = new StringBuilder();
			//sb.AppendLine("<DOC>");
			//sb.AppendLine("<summary>");
			//sb.AppendLine($"Initializes a new instance of the <see cref=\"{codeFunction.Name}\"/> class.");
			//sb.AppendLine("</summary>");
			//if (codeFunction.Parameters.Count > 0)
			//{
			//	foreach (CodeElement parameter in codeFunction.Parameters)
			//	{
			//		sb.AppendLine($"<param name= \"{parameter.Name}\">The {parameter.Name.CamelCaseSplit()}.</param>");
			//	}
			//}
			//sb.AppendLine("</DOC>");

			//codeFunction.DocComment = sb.ToString();
		}

		private static void AddDocCommentToProperty(CodeProperty codeProperty)
		{
			if (AddGhostDogCommand(codeProperty as CodeElement))
			{
				RemoveValueTagFromDocComment(codeProperty);
			}
		}

		private static void RemoveValueTagFromDocComment(CodeProperty codeProperty)
		{
			var editPpoint = codeProperty.StartPoint.CreateEditPoint();
			editPpoint.LineUp();
			if (editPpoint.GetLineText().Trim() == "/// </value>")
			{
				var endPoint = codeProperty.StartPoint.CreateEditPoint();

				endPoint.StartOfLine();

				editPpoint.LineUp(2);
				editPpoint.StartOfLine();
				editPpoint.Delete(endPoint);
			}
		}

		private static void AddDocCommentToInterface(CodeInterface codeInterface)
		{
			codeInterface.DocComment = string.Format("The {0}.", codeInterface.Name.CamelCaseSplit()).ToDocComment();
		}

		private static void AddDocCommentToClass(CodeClass codeClass)
		{
			codeClass.DocComment = string.Format("Straightforward {0} implementation.", codeClass.Name.CamelCaseSplit()).ToDocComment();
		}

		private static bool AddGhostDogCommand(CodeElement codeElement)
		{
			var txtSel = (TextSelection)codeElement.ProjectItem.Document.Selection;
			txtSel.MoveToPoint(codeElement.StartPoint, false);
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			try
			{
				dte.ExecuteCommand("Tools.SubMain.GhostDoc.DocumentThis", "");
				return true;
			}
			catch (Exception)
			{
				try
				{
					dte.ExecuteCommand("Weigelt.GhostDoc.AddIn.DocumentThis", "");
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		private static bool IsInitializeMethodOfAModule(CodeFunction codeFunction)
		{
			if (codeFunction.Name == "Initialize" && codeFunction.Parameters.Count == 1)
			{
				foreach (CodeElement parameter in codeFunction.Parameters)
				{
					if (parameter.InnerText().StartsWith("ITypeRegistry "))
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
