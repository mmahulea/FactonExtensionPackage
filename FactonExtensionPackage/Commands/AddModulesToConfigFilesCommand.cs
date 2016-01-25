namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using System.Globalization;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class AddModulesToConfigFilesCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("747e95e4-8ab2-422f-b60f-0329cbc2fce6");

		private readonly Package package;

		private AddModulesToConfigFilesCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			this.package = package;

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuCommand);
				menuCommand.BeforeQueryStatus += MenuItemBeforeQueryStatus;
			}
		}

		public static AddModulesToConfigFilesCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new AddModulesToConfigFilesCommand(package);
		}

		private static void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var codeClass = dte.FindCodeElemet<CodeClass>();
			OleMenuCommand menuCommand = sender as OleMenuCommand;
			if (menuCommand != null)
			{
				menuCommand.Visible = codeClass.GetImplementedInterfaces().Any(m => m.FullName == "Facton.Infrastructure.Modularity.IModule");
			}
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var moduleProxy = new ModuleProxy(dte.ActiveDocument.ProjectItem);

			string title = "AddModulesToConfigFilesCommand";

			VsShellUtilities.ShowMessageBox(
				this.ServiceProvider,
				"Proviced services: " + moduleProxy.ProvidedServices.Aggregate((x, y) => x + ", " + y),
				title,
				OLEMSGICON.OLEMSGICON_INFO,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}
	}
}
