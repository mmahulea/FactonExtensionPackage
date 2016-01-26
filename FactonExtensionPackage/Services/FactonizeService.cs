namespace FactonExtensionPackage.Services
{
	using System;
	using System.Linq;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.FormatingCommands;
	using FactonExtensionPackage.FormatingCommands.SubCommands;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public class FactonizeService
	{
		public static void FactonizeCurrentOpenFile()
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx");
			try
			{
				var txtSel = (TextSelection)dte.ActiveDocument.Selection;
				var line = txtSel.ActivePoint.Line;
				var column = txtSel.ActivePoint.VirtualDisplayColumn;
				//try
				//{
				//	dte.ExecuteCommand("ReSharper_SilentCleanupCode");
				//}
				//catch (Exception)
				//{
				//	// ignored
				//}
				Execute(dte.ActiveDocument.ProjectItem);				
				try
				{
					txtSel.MoveTo(line, column);
				}
				catch { }
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}

		public static void FactonizeSolutionExplorerSelectedProjects()
		{
			var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx");
			try
			{
				var projects = dte.GetSelectedProjectInSolutionExplorer();
				foreach (var project in projects)
				{
					var projectItems = project.FindProjectItems(p => p.Name.EndsWith(".cs") || p.Name.EndsWith(".config"));
					foreach (var projectItem in projectItems)
					{
						Execute(projectItem);
					}
				}
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}

		public static void FactonizePendingChanges()
		{
			var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx");
			try
			{
				var pendingChanges = TfsService.GetPendingChanges();
				if (pendingChanges.Any())
				{
					var projects = dte.GetSelectedProjectInSolutionExplorer();
					foreach (var projectItem in pendingChanges.Where(p => projects.Contains(p.ContainingProject)))
					{
						Execute(projectItem);
					}
				}
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}

		private static void Execute(ProjectItem projectItem)
		{
			if ((projectItem.Name.EndsWith(".cs", StringComparison.CurrentCulture) && projectItem.FindNameSpace() != null)
				|| (projectItem.Name.EndsWith(".config", StringComparison.CurrentCulture)))
			{
				bool opened = projectItem.TryOpen();
				InternalExecute(projectItem);
				projectItem.TryClose(opened);
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
			if (projectItem.Name.EndsWith(".cs", StringComparison.CurrentCulture))
			{
				AddCopyrightCommand.Execute(projectItem);
				AddArgumentNullChecksToContructors.InternalExecute(projectItem);
				FormatUsingStatements.Execute(projectItem);
				FactonizeCurrentProjectCommand.Execute(projectItem);
				FormalLineOneByOneCommand.Execute(projectItem);
				AddCommentsToCodeElements.Execute(projectItem);
				if (projectItem.IsModuleClass())
				{
					ModuleClassService.VerifyConfiguration(projectItem);
				}
			}
			if (projectItem.Name.EndsWith(".config", StringComparison.CurrentCulture))
			{
				FactonizeModuleCommand.Execute(projectItem);
			}

			if (!projectItem.ContainingProject.IsTestProject())
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