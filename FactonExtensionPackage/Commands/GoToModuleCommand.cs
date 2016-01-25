﻿namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class GoToModuleCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("c9321ebf-d413-4d66-99c4-59cea4a010f4");

		private readonly OleMenuCommand menuCommand;

		private readonly Package package;

		private GoToModuleCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			this.package = package;

			OleMenuCommandService commandService =
				this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				this.menuCommand = new OleMenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(this.menuCommand);

				this.menuCommand.BeforeQueryStatus += this.MenuItemBeforeQueryStatus;
			}
		}

		public static GoToModuleCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new GoToModuleCommand(package);
		}

		private void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			this.menuCommand.Visible = dte.ActiveDocument.ProjectItem.IsModuleConfig();
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var projectItem = SearchService.FindModuleFromConfig(dte.ActiveDocument.ProjectItem);
			projectItem?.OpenInEditor();
			//var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			//if (this.moduleConfig == null)
			//{
			//	var configFileName = dte.GetConfigFileNameFromCurrentLine();
			//	if (!string.IsNullOrWhiteSpace(configFileName))
			//	{
			//		var projectItem = dte.Solution.FindProjectItem(p => p.Name == configFileName);
			//		if (projectItem != null)
			//		{
			//			this.moduleConfig = projectItem.Deserialize<XmlModuleConfig>();
			//		}
			//	}
			//}

			//if (this.moduleConfig != null)
			//{
			//	var project = dte.Solution.FindProject(p => p.Name == this.moduleConfig.AssemblyName);
			//	var projectItem = project.FindProjectItem(p => p.Name == this.moduleConfig.CsFileName);
			//	projectItem?.OpenInEditor();
			//}
		}
	}
}