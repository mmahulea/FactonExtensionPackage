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

	internal sealed class GoToModuleCommand
	{
		private string moduleName;

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

			var commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
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
			bool visible = this.menuCommand.Visible = dte.ActiveDocument.ProjectItem.IsModuleConfig();
			if (!visible)
			{
				var lineText = dte.GetLineText();
				this.moduleName = lineText.Matches(@"<module name=\""(?<element>[^\>]+)\""").FirstOrDefault();
				visible = !string.IsNullOrWhiteSpace(this.moduleName);
			}
			this.menuCommand.Visible = visible;
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			ProjectItem condigProjectItem = dte.ActiveDocument.ProjectItem;
			if (!string.IsNullOrWhiteSpace(this.moduleName))
			{
				condigProjectItem = dte.Solution.FindProjectItem(this.moduleName + ".config");
			}
			if (condigProjectItem != null)
			{
				var projectItem = SearchService.FindModuleFromConfig(condigProjectItem);
				projectItem?.OpenInEditor();
			}
		}
	}
}