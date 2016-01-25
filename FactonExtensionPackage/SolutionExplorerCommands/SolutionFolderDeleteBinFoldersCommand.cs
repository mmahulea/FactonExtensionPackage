namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class SolutionFolderDeleteBinFoldersCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("3046e72d-17b0-4976-9029-07e917846149");

		private readonly Package package;

		private SolutionFolderDeleteBinFoldersCommand(Package package)
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

		public static SolutionFolderDeleteBinFoldersCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new SolutionFolderDeleteBinFoldersCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			DeleteBinFoldersService.Delete();
		}
	}
}
