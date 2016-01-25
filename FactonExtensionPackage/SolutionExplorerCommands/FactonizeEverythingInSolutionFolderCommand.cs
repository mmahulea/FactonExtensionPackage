namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class FactonizeEverythingInSolutionFolderCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("aec2b311-1ac5-43c6-a41c-b1d5eeac9cfb");

		private readonly Package package;

		private FactonizeEverythingInSolutionFolderCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException("package");
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

		public static FactonizeEverythingInSolutionFolderCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FactonizeEverythingInSolutionFolderCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			FactonizeService.FactonizeSolutionExplorerSelectedProjects();
		}
	}
}
