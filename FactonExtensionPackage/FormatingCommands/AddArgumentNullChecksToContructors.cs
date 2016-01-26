namespace FactonExtensionPackage.FormatingCommands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.FormatingCommands.SubCommands;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class AddArgumentNullChecksToContructors
	{
		private static readonly string exceptionMessage1 = "<exception cref=\"System.ArgumentNullException\">If any parameter is <c>null</c>.</exception>";
		private static readonly string exceptionMessage2 = "<exception cref=\"ArgumentNullException\">If any parameter is <c>null</c>.</exception>";

		public static void Execute(ProjectItem projectItem)
		{
			if (projectItem.Name.EndsWith(".cs", StringComparison.Ordinal) && SearchService.FindNameSpace(projectItem) != null)
			{
				bool opend = projectItem.TryOpen();
				InternalExecute(projectItem);
				projectItem.TryClose(opend);
			}

			if (projectItem.HasValidSubProjectItems())
			{
				foreach (ProjectItem item in projectItem.ProjectItems)
				{
					Execute(item);
				}
			}
		}

		public static void InternalExecute(ProjectItem projectItem)
		{
			var ctors = new List<CodeFunction>();

			foreach (CodeFunction constructor in projectItem.FindContructors().Where(m => m.Parameters.Count > 0))
			{
				var codeElement = constructor.As<CodeElement>();
				string ctorText = codeElement.InnerText();
				var editPoint = codeElement.AtTheFirstLineAfterTheOpeningBrakect();

				bool lineAdded = false;
				foreach (var param in constructor.Parameters().Reverse())
				{
					if (param.Type.CodeType.Kind != vsCMElement.vsCMElementStruct && !ctorText.Contains($"ArgumentNullException(\"{param.Name}\")"))
					{
						lineAdded = true;
						projectItem.AddLine(editPoint, param.Name.ToCtorNullCheck());
					}
				}

				if (lineAdded)
				{
					ctors.Add(constructor);
				}
			}

			if (ctors.Any())
			{
				if (!projectItem.Contains("using System;"))
				{
					projectItem.AddLine(projectItem.FindNameSpace().As<CodeElement>().AtTheFirstLineAfterTheOpeningBrakect(), "using System;");
				}
			}

			foreach (CodeFunction constructor in projectItem.FindContructors().Where(m => m.Parameters.Count > 0))
			{
				if (string.IsNullOrWhiteSpace(constructor.DocComment))
				{
					AddCommentsToCodeElements.AddDocCommentToCtor(constructor);
				}

				var docComment = constructor.DocComment;
				var codeElement = constructor.As<CodeElement>();
				if ((codeElement.Contains("throw new ArgumentNullException") || codeElement.Contains("throw new System.ArgumentNullException"))
					&& (!docComment.Contains(exceptionMessage1) && !docComment.Contains(exceptionMessage2)))
				{
					codeElement.AppendToDocComment(exceptionMessage1);
				}
			}

			if (ctors.Any())
			{
				var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
				try
				{
					dte.ExecuteCommand("Edit.FormatDocument");
				}
				catch { }
			}
		}
	}
}
