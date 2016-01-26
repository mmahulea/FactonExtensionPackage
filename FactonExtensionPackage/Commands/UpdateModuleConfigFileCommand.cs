namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class UpdateModuleConfigFileCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("c56c7a6d-d1a4-4f7a-a62e-841eb59fd89e");

		private readonly Package package;

		private UpdateModuleConfigFileCommand(Package package)
		{
			this.package = package;

			var commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);

				menuItem.BeforeQueryStatus += MenuItemBeforeQueryStatus;
			}
		}

		private static void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var codeClass = dte.FindCodeElemet<CodeClass>();
			var menuCommand = sender as OleMenuCommand;
			if (menuCommand != null)
			{
				menuCommand.Visible = codeClass.GetImplementedInterfaces().Any(m => m.FullName == "Facton.Infrastructure.Modularity.IModule");
			}
		}

		public static UpdateModuleConfigFileCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new UpdateModuleConfigFileCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var projectItem = dte.ActiveDocument.ProjectItem;
			ModuleSystemService.GetModuleInfo(SearchService.FindConfigFromModule(projectItem).Name);
			ConfigFileEditorService.UpdateConfigFile(projectItem);
		}
	}
}
