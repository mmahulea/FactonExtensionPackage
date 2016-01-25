namespace FactonExtensionPackage.Commands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Design;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class FindNotRequiredModulesCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("d65355bb-ef74-4b89-bfb7-6b6539593652");

		private readonly Package package;

		private FindNotRequiredModulesCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException("package");
			}

			this.package = package;

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
				commandService.AddCommand(menuItem);
			}
		}

		public static FindNotRequiredModulesCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FindNotRequiredModulesCommand(package);
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			string message = null;
			if (!GlobalVariables.ProjectsLoaded)
			{
				message = "Solution isn't loaded completely.";
			}
			else
			{
				var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

				List<ProjectItem> configFiles = dte.Solution.FindProjectItems(
					p => p.Name.Contains("Facton"),
					p => (p.Name.EndsWith(".config") || p.Name.EndsWith(".xml")) && p.Name != "CodeAnalysisDictionary.xml" && p.Name != "App.config");

				Dictionary<string, ProjectItem> projectItemDictionary = configFiles.GroupBy(t => t.ModuleName())
					.ToDictionary(grp => grp.Key, grp => grp.First());

				List<ModuleProxy> factonModules =
					this.GetProjectItems(dte.GetActiveDocumentText(), projectItemDictionary)
					.Select(p => new ModuleProxy(p)).ToList();

				var notRequiredModules = new List<ModuleProxy>();
				foreach (var factonModule in factonModules)
				{
					if (!factonModules.Where(m => m != factonModule)
						.Any(m => m.RequiredServices.Any(s => factonModule.ProvidedServices.Contains(s.Name))))
					{
						notRequiredModules.Add(factonModule);
					}
				}

				if (notRequiredModules.Any())
				{
					message = $"Found {notRequiredModules.Count} not required modules.";

					var sb = new StringBuilder();
					sb.Append("Not required modules: " + Environment.NewLine);
					notRequiredModules.ForEach(
						s =>
						{
							sb.Append($"{s.ModuleName}" + Environment.NewLine);
						});

					Window window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
					OutputWindow outputWindow = (OutputWindow)window.Object;
					outputWindow.ActivePane.Activate();
					outputWindow.ActivePane.OutputString(Environment.NewLine + sb);
				}
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				message = "Didn't find any not required modules.";
			}
			string title = "Find not required command";

			VsShellUtilities.ShowMessageBox(
					this.ServiceProvider,
					message,
					title,
					OLEMSGICON.OLEMSGICON_INFO,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}

		private List<ProjectItem> GetProjectItems(string text, Dictionary<string, ProjectItem> projectItemDictionary)
		{
			var projectItems = new List<ProjectItem>();
			var factonModules = XDocument.Parse(text).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonModules");
			var moduleEntryList = factonModules?.ToString().Deserialize<XmlModuleEntryList>();
			if (moduleEntryList != null)
			{
				foreach (IModuleEntry moduleEntry in moduleEntryList.ModuleEntries)
				{
					ProjectItem projectItem;
					projectItemDictionary.TryGetValue(moduleEntry.ModuleName, out projectItem);
					if (projectItem == null)
					{
						throw new Exception($"{moduleEntry.ModuleName} not found");
					}
					projectItems.Add(projectItem);
				}

				foreach (var configFileName in moduleEntryList.ModuleRegions.Where(mr => mr.HasConfigFile).Select(mr => mr.ConfigFileName))
				{
					ProjectItem xmlFile;
					projectItemDictionary.TryGetValue(configFileName, out xmlFile);
					if (xmlFile == null)
					{
						throw new Exception($"{configFileName} not found");
					}
					projectItems.AddRange(this.GetProjectItems(xmlFile.ReadAllText(), projectItemDictionary));
				}
			}
			return projectItems;
		}
	}
}
