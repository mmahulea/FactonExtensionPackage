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

	internal sealed class GoToConfigCommand
	{
		private string moduleName;

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
				var menuCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuCommand);
				menuCommand.BeforeQueryStatus += this.MenuItemBeforeQueryStatus;
			}
		}

		public static GoToConfigCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new GoToConfigCommand(package);
		}

		private void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			bool visible = dte.ActiveDocument.ProjectItem.IsModuleClass();
			if (!visible)
			{
				var lineText = dte.GetLineText();
				this.moduleName = lineText.Matches(@"<module name=\""(?<element>[^\>]+)\""").FirstOrDefault();
				visible = !string.IsNullOrWhiteSpace(this.moduleName);
			}
			((OleMenuCommand)sender).Visible = visible;
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			ProjectItem condigProjectItem = null;
			if (!string.IsNullOrWhiteSpace(this.moduleName))
			{
				condigProjectItem = dte.Solution.FindProjectItem(this.moduleName + ".config");
			}
			if (condigProjectItem != null)
			{
				condigProjectItem.OpenInEditor();
			}
			else
			{
				var projectItem = SearchService.FindConfigFromModule(dte.ActiveDocument.ProjectItem);
				projectItem?.OpenInEditor();
			}
		}
	}
}