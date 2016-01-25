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

	internal sealed class InheritDocInSolutionExplorerCommand
	{
		public const int CommandId = 4129;

		public static readonly Guid CommandSet = new Guid("fb2bf056-14a9-463a-a1f7-add539d25c73");

		private readonly Package package;

		private InheritDocInSolutionExplorerCommand(Package package)
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
				var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);
			}
		}

		public static InheritDocInSolutionExplorerCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new InheritDocInSolutionExplorerCommand(package);
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
					AddInheritdocEverywhere.Execute(projectItem);
				}
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}
	}
}
