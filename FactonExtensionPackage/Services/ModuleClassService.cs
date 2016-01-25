namespace FactonExtensionPackage.Services
{
	using System;
	using System.Linq;
	using System.Windows;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class ModuleClassService
	{
		public static bool VerifyConfiguration(ProjectItem projectItem)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			var configProjectItem = SearchService.FindConfigFromModule(projectItem);
			if (configProjectItem == null)
			{
				throw new Exception($"Configuration file not found for module {projectItem.Name}");
			}
			string configText = configProjectItem.ReadAllText();

			var moduleText = projectItem.ReadAllText();
			var serviceNames =
				moduleText.Matches(@".GetObject\<(?<element>[^\>]+)\>")
					.Union(moduleText.Matches(@".GetRuntimeObject\<(?<element>[^\>]+)\>"))
					.Union(moduleText.Matches(@".RegisterInstance\<(?<element>[^\>]+)\>"));


			foreach (var serviceName in serviceNames)
			{
				var interfaceProjectItem = dte.Solution.FindProjectItem(serviceName + ".cs");
				if (interfaceProjectItem == null)
				{
					throw new Exception($"Service {serviceName} not found in solution.");
				}
				var nameSpace = SearchService.FindNameSpace(interfaceProjectItem);
				var fullName = nameSpace.Name + "." + serviceName;

				if (!configText.Contains(fullName))
				{
					throw new Exception($"{fullName} mot found in the configuration file. Model: {projectItem.Name}");
				}
			}

			return false;
		}
	}
}