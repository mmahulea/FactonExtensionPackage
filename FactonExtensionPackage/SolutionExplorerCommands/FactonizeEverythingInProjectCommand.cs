namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class FactonizeEverythingInProjectCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("c86370ff-1e84-4acc-a36e-a6fc52a52247");

		private readonly Package package;

		private FactonizeEverythingInProjectCommand(Package package)
		{
			this.package = package;

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);
			}
		}

		public static FactonizeEverythingInProjectCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FactonizeEverythingInProjectCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			FactonizeService.FactonizeSolutionExplorerSelectedProjects();
		}
	}
}
