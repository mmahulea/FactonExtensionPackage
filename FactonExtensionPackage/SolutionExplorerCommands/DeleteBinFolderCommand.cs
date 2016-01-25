namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Design;
	using System.IO;
	using System.Linq;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class DeleteBinFolderCommand
	{
		private const int CommandId = 256;
		private static readonly Guid CommandSet = new Guid("c79ec208-9741-471f-a093-08ccc52c1e7b");
		private readonly Package package;

		private DeleteBinFolderCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException("package");
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

		public static DeleteBinFolderCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new DeleteBinFolderCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			DeleteBinFoldersService.Delete();
		}
	}
}
