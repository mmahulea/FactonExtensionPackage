namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class FactonizePendingChangesInProjectCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("b6d6b651-74f2-415a-ad6d-a8ffefb0dc5c");

		private readonly Package package;

		private FactonizePendingChangesInProjectCommand(Package package)
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

		public static FactonizePendingChangesInProjectCommand Instance
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
			Instance = new FactonizePendingChangesInProjectCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			FactonizeService.FactonizePendingChanges();
		}
	}
}
