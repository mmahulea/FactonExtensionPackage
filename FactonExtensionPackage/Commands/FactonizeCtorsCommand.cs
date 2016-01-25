namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.FormatingCommands;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class FactonizeCtorsCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("12e3c432-79da-43b0-9210-a7dab48db188");

		private readonly Package package;

		private FactonizeCtorsCommand(Package package)
		{
			this.package = package;

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);

				menuItem.BeforeQueryStatus += (sender, e) =>
					{
						var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
						((OleMenuCommand)sender).Visible =
							SearchService.FindContructors(dte).Any(m => m.Parameters.Count > 0);
					};
			}
		}

		public static FactonizeCtorsCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FactonizeCtorsCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx", false);
			try
			{
				AddArgumentNullChecksToContructors.Execute(dte.ActiveDocument.ProjectItem);
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}
	}
}
