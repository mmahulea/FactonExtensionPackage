namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Design;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.FormatingCommands;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class FactonizeCtorsInSolutionExplorerCommand
	{
		public const int CommandId = 256;
		public static readonly Guid CommandSet = new Guid("4cdce755-cb5b-4449-b80b-67ee689d7fdd");
		private readonly Package package;

		private FactonizeCtorsInSolutionExplorerCommand(Package package)
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
				var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);
			}
		}

		public static FactonizeCtorsInSolutionExplorerCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FactonizeCtorsInSolutionExplorerCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx", false);
			try
			{
				List<ProjectItem> projectItems = TfsService.GetPendingChanges();
				foreach (var projectItem in projectItems)
				{
					AddArgumentNullChecksToContructors.Execute(projectItem);
				}
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}
	}
}
