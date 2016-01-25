namespace FactonExtensionPackage.Services
{
	using System.Collections.Generic;
	using EnvDTE;
	using Microsoft.TeamFoundation.Client;
	using Microsoft.TeamFoundation.VersionControl.Client;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class TfsService
	{
		public static List<ProjectItem> GetPendingChanges()
		{
			var projectItems = new List<ProjectItem>();
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var projectCollections = new List<RegisteredProjectCollection>(RegisteredTfsConnections.GetProjectCollections());
			foreach (var registeredProjectCollection in projectCollections)
			{
				var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(registeredProjectCollection);
				var versionControl = projectCollection.GetService<VersionControlServer>();
				var workspace = versionControl.GetWorkspace(dte.Solution.FileName);

				foreach (var pendingChange in workspace.GetPendingChangesEnumerable())
				{
					if (pendingChange.FileName != null && (pendingChange.FileName.EndsWith(".cs") || pendingChange.FileName.EndsWith(".config")))
					{
						var projectItem = dte.Solution.FindProjectItem(pendingChange.FileName);
						if (projectItem != null)
						{
							projectItems.Add(projectItem);
						}
					}
				}
			}
			return projectItems;
		}
	}
}