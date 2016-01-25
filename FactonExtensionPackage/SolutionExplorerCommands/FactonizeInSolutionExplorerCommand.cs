namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class FactonizeInSolutionExplorerCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("8535b993-9f6f-4a8a-b8c0-a5f4b45bfdad");

		private readonly Package package;

		private FactonizeInSolutionExplorerCommand(Package package)
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

		public static FactonizeInSolutionExplorerCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FactonizeInSolutionExplorerCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			FactonizeService.FactonizePendingChanges();
		}
	}
}
