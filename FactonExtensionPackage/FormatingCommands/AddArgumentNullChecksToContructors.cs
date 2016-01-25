namespace FactonExtensionPackage.FormatingCommands
{
	using System;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
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

		private static void InternalExecute(ProjectItem projectItem)
		{
			foreach (CodeFunction constructor in projectItem.FindContructors().Where(m => m.Parameters.Count > 0))
			{
				var codeElement = constructor.As<CodeElement>();
				var editPoint = codeElement.AtTheFirstLineAfterTheOpeningBrakect();

				foreach (var param in constructor.Parameters().Reverse())
				{
					if (param.Type.CodeType.Kind != vsCMElement.vsCMElementStruct && !codeElement.Contains($"ArgumentNullException(\"{param.Name}\")"))
					{
						projectItem.AddLine(editPoint, param.Name.ToCtorNullCheck());
					}
				}

				var docComment = codeElement.GetDocComment();
				if ((codeElement.Contains("throw new ArgumentNullException") || codeElement.Contains("throw new System.ArgumentNullException"))
					&& (!docComment.Contains(exceptionMessage1) && !docComment.Contains(exceptionMessage2)))
				{
					codeElement.AppendToDocComment(exceptionMessage1);
				}

				if (projectItem.IsDirty)
				{
					if (!projectItem.Contains("using System;"))
					{
						projectItem.AddLine(projectItem.FindNameSpace().As<CodeElement>().AtTheFirstLineAfterTheOpeningBrakect(), "using System;");
					}

					var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
					dte.ExecuteCommand("Edit.FormatDocument");
				}
			}
		}
	}
}
