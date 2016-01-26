namespace FactonExtensionPackage.Commands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Design;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.FormatingCommands;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class AddInheritDocCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("fb2bf056-14a9-463a-a1f7-add539d25c73");

		private readonly Package package;

		private AddInheritDocCommand(Package package)
		{
			this.package = package;

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuItem);

				menuItem.BeforeQueryStatus += (sender, e) =>
					{
						bool visible = false;
						var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
						var classElem = SearchService.FindClass(dte);
						if (classElem != null)
						{
							List<CodeElement> codeElements = SearchService.FindCodeElements(dte);
							if (classElem.ImplementedInterfaces.Count > 0)
							{
								visible = true;
							}
							else
							{
								visible = codeElements.Any(elem => elem.IsInherited() || elem.OverridesSomething());
							}
						}
						((OleMenuCommand)sender).Visible = visible;
					};
			}
		}

		public static AddInheritDocCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new AddInheritDocCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx");
			try
			{
				AddInheritdocEverywhere.Execute(dte.ActiveDocument.ProjectItem);
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}
	}
}
