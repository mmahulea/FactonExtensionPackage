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

	internal sealed class TestCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("6abe100b-97b4-4737-8778-69419bb159aa");

		private readonly Package package;

		private TestCommand(Package package)
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

		public static TestCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new TestCommand(package);
		}

		private static List<string> GetBootStrappedServices(string text)
		{
			List<string> bootStrappedServices = new List<string> { "Facton.UI.Wpf.Clients.Main.IShell" };
			XElement factonBootstrapper = XDocument.Parse(text).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonBootstrapper");
			if (factonBootstrapper != null)
			{
				IEnumerable<XElement> entries = factonBootstrapper.Descendants().Where(p => p.Name.LocalName == "entry");
				bootStrappedServices.AddRange(entries.Select(entry => entry.Attribute("keyType").Value.Split(',')[0].Trim()));
			}
			return bootStrappedServices;
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

				Dictionary<string, ModuleProxy> projectItemDictionary = configFiles.GroupBy(t => t.ModuleName())
					.ToDictionary(grp => grp.Key, grp => new ModuleProxy(grp.First()));

				List<ModuleProxy> factonModules =
					this.GetProjectItems(dte.GetActiveDocumentText(), projectItemDictionary).ToList();

				var requiredServices = new List<RequiredService>();
				var providedServices = GetBootStrappedServices(dte.GetActiveDocumentText());

				if (dte.ActiveDocument.ProjectItem.ContainingProject.IsTestProject())
				{
					providedServices.Add("Facton.Infrastructure.Core.Logging.ILogManager");
					providedServices.Add("Facton.Infrastructure.Core.Logging.ICommonEventSource");
					providedServices.Add("Facton.Infrastructure.Settings.ISystemSettingsService");
					providedServices.Add("Facton.Infrastructure.Modularity.IModuleConfigurationLoader");
					providedServices.Add("Facton.Infrastructure.Modularity.IModuleConfiguration");
					providedServices.Add("Facton.Infrastructure.Modularity.IModuleInitializer");
					providedServices.Add("Facton.Infrastructure.Modularity.IModuleLoader");
					providedServices.Add("Facton.Infrastructure.Modularity.Events.IEventRegistry");
				}

				foreach (var module in factonModules)
				{
					requiredServices.AddRange(module.RequiredServices);
					providedServices.AddRange(module.ProvidedServices);
				}

				var missingServices = (from requiredService in requiredServices.Distinct()
									   let isProvided = providedServices.Any(p => p == requiredService.Name)
									   where !isProvided
									   select requiredService).ToList();

				if (missingServices.Any())
				{
					missingServices.Sort();
					var sb = new StringBuilder();
					sb.Append("Missing services: " + Environment.NewLine);
					missingServices.ForEach(
						missingService =>
							{
								var requiredModules = factonModules.Where(m => m.RequiredServices.Contains(missingService)).Select(m => m.ModuleName).Distinct();
								sb.Append($"{missingService}" + Environment.NewLine);

								var providedBy = projectItemDictionary.Where(item => item.Value.ProvidedServices.Contains(missingService.Name));
								if (providedBy.Any())
								{
									sb.Append("\tProvided by:" + Environment.NewLine);
									foreach (var module in providedBy)
									{
										sb.Append("\t\t" + module.Key + Environment.NewLine);
									}
								}

								sb.Append("\tRequired by:" + Environment.NewLine);
								foreach (var requiredModule in requiredModules)
								{
									sb.Append("\t\t" + requiredModule + Environment.NewLine);
								}
							});

					message = $"Found {missingServices.Count} missing service(s).";
					Window window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
					OutputWindow outputWindow = (OutputWindow)window.Object;
					outputWindow.ActivePane.Activate();
					outputWindow.ActivePane.OutputString(Environment.NewLine + sb);
				}
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				message = "All services are provided";
			}

			string title = "Find missing services command";

			VsShellUtilities.ShowMessageBox(
					this.ServiceProvider,
					message,
					title,
					OLEMSGICON.OLEMSGICON_INFO,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}

		private List<ModuleProxy> GetProjectItems(string text, Dictionary<string, ModuleProxy> projectItemDictionary)
		{
			var projectItems = new List<ModuleProxy>();
			var factonModules = XDocument.Parse(text).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonModules");
			var moduleEntryList = factonModules?.ToString().Deserialize<XmlModuleEntryList>();
			if (moduleEntryList != null)
			{
				foreach (IModuleEntry moduleEntry in moduleEntryList.ModuleEntries)
				{
					ModuleProxy projectItem;
					projectItemDictionary.TryGetValue(moduleEntry.ModuleName, out projectItem);
					if (projectItem == null)
					{
						throw new Exception($"{moduleEntry.ModuleName} not found");
					}
					projectItems.Add(projectItem);
				}

				foreach (var configFileName in moduleEntryList.ModuleRegions.Where(mr => mr.HasConfigFile).Select(mr => mr.ConfigFileName))
				{
					ModuleProxy xmlFile;
					projectItemDictionary.TryGetValue(configFileName, out xmlFile);
					if (xmlFile == null)
					{
						throw new Exception($"{configFileName} not found");
					}
					projectItems.AddRange(this.GetProjectItems(xmlFile.Text, projectItemDictionary));
				}
			}
			return projectItems;
		}
	}
}
