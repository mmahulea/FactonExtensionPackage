// <copyright file="ShowModuleWindowCommand.cs" company="Facton GmbH">
// All rights are reserved. Reproduction or transmission in whole or in part, in
// any form or by any means, electronic, mechanical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// </copyright>

namespace FactonExtensionPackage
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Design;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Forms;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class ShowModuleWindowCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("5cb59896-ecd2-4e36-ba74-36e5e5c2ebc5");

		private readonly Package package;

		private ShowModuleWindowCommand(Package package)
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

		public static ShowModuleWindowCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		public static void Initialize(Package package)
		{
			Instance = new ShowModuleWindowCommand(package);
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			if (!GlobalVariables.ProjectsLoaded)
			{
				var message = "Solution isn't loaded completely.";
				VsShellUtilities.ShowMessageBox(
					this.ServiceProvider,
					message,
					"Show module window",
					OLEMSGICON.OLEMSGICON_INFO,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				return;
			}

			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			List<ProjectItem> configFiles = dte.Solution.FindProjectItems(
				p => p.Name.Contains("Facton"),
				p => (p.Name.EndsWith(".config") || p.Name.EndsWith(".xml")) && p.Name != "CodeAnalysisDictionary.xml" && p.Name != "App.config");

			Dictionary<string, ProjectItem> projectItemDictionary = configFiles.GroupBy(t => t.ModuleName())
				.ToDictionary(grp => grp.Key, grp => grp.First());

			List<ModuleProxy> factonModules =
				this.GetProjectItems(dte.GetActiveDocumentText(), projectItemDictionary).Select(p => new ModuleProxy(p)).ToList();

			using (ModuleForm moduleForm = new ModuleForm(projectItemDictionary, factonModules))
			{
				moduleForm.ShowDialog();
			}
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
