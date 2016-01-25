namespace FactonExtensionPackage.Services
{
	using System.Linq;
	using System.Text;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public class ConfigFileEditorService
	{
		public static void UpdateConfigFile(ProjectItem projectItem)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			ProjectItem module = null;
			ProjectItem config = null;

			if (projectItem.IsModuleConfig())
			{
				module = SearchService.FindModuleFromConfig(projectItem);
				config = projectItem;
			}
			else if (projectItem.IsModuleClass())
			{
				module = projectItem;
				config = SearchService.FindConfigFromModule(projectItem);
			}

			string moduleText = module.ReadAllText();
			string configText = config.ReadAllText();
			string updatedConfigText = string.Empty;

			var services = moduleText.Matches(@".GetObject\<(?<element>[^\>]+)\>");
			var runtimeServices = moduleText.Matches(@".GetRuntimeObject\<(?<element>[^\>]+)\>");
			var providedServices = moduleText.Matches(@".RegisterInstance\<(?<element>[^\>]+)\>");

			var configFiles = dte.Solution.FindProjectItems(
				p => p.Name.Contains("Facton") && !p.IsTestProject(),
				p => p.Name.EndsWith(".config") && p.Name != "CodeAnalysisDictionary.xml" && p.Name != "App.config");

			var modules =
				configFiles.GroupBy(t => t.ModuleName())
					.ToDictionary(grp => grp.Key, grp => new ModuleProxy(grp.First()))
					.Select(m => m.Value)
					.OrderBy(m => m.ConfigFileName);


			foreach (var service in services)
			{
				ModuleProxy moduleProxy = modules.FirstOrDefault(m => m.ProvidedServices.Any(s => s == service));
				string moduleName = moduleProxy.ModuleName;
			}






			updatedConfigText =
				updatedConfigText.Replace(" requirementType=\"normal\"", string.Empty).Replace(" />", "/>").Replace("?>", " ?>").Trim();
			if (!(configText.EqualsExcludingWhitespace(updatedConfigText)))
			{
				config.SetText(updatedConfigText);
			}
		}
	}
}