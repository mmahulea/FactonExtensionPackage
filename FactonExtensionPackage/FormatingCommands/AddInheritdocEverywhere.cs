namespace FactonExtensionPackage.FormatingCommands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Services;

	public class AddInheritdocEverywhere
	{
		public static void Execute(ProjectItem projectItem)
		{
			if (projectItem.Name.EndsWith(".cs") && SearchService.FindNameSpace(projectItem) != null)
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
			List<CodeElement> codeElements = SearchService.FindCodeElements(projectItem);
			string statement = $"<doc>{Environment.NewLine}<inheritdoc/>{Environment.NewLine}</doc>";
			string statement2 = $"<doc>{Environment.NewLine}<inheritdoc />{Environment.NewLine}</doc>";
			foreach (var codeElement in codeElements.Where(p => (p.IsInherited() || p.OverridesSomething())))
			{
				var docComment = codeElement.GetDocComment().Trim().ToLower();
				if (!(docComment == statement || docComment == statement2))
				{
					codeElement.SetDocComment(statement);
				}
			}
		}
	}
}