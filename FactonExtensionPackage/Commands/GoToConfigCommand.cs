namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class GoToConfigCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("30711e6c-3e6b-4033-82f2-31c356d88968");

		private readonly Package package;

		private GoToConfigCommand(Package package)
		{
			this.package = package;

			var commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuCommand = new OleMenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuCommand);
				menuCommand.BeforeQueryStatus += MenuItemBeforeQueryStatus;
			}
		}

		public static GoToConfigCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new GoToConfigCommand(package);
		}

		private static void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			((OleMenuCommand)sender).Visible = dte.ActiveDocument.ProjectItem.IsModuleClass();
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var projectItem = SearchService.FindConfigFromModule(dte.ActiveDocument.ProjectItem);
			projectItem?.OpenInEditor();
		}
	}
}