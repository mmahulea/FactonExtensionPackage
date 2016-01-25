namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;

	internal sealed class FactonizeCommand
	{
		public const int CommandId = 0x0100;

		public static readonly Guid CommandSet = new Guid("4986d6bb-e9d5-426c-aeea-22c34057aa21");

		private readonly Package package;

		private FactonizeCommand(Package package)
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
				var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);
			}
		}

		public static FactonizeCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new FactonizeCommand(package);
		}

		public void MenuItemCallback(object sender, EventArgs e)
		{
			FactonizeService.FactonizeCurrentOpenFile();
		}
	}
}