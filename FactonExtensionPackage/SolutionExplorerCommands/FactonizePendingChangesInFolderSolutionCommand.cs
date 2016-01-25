namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class FactonizePendingChangesInFolderSolutionCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("bfeabf51-c647-4cc8-bcad-21b9ebf77fcd");

		private readonly Package package;

		private FactonizePendingChangesInFolderSolutionCommand(Package package)
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
				var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
				commandService.AddCommand(menuItem);
			}
		}

		public static FactonizePendingChangesInFolderSolutionCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		public static void Initialize(Package package)
		{
			Instance = new FactonizePendingChangesInFolderSolutionCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			FactonizeService.FactonizePendingChanges();
		}
	}
}
