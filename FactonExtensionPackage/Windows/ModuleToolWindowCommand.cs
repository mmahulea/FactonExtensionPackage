namespace FactonExtensionPackage.Windows
{
	using System;
	using System.ComponentModel.Design;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class ModuleToolWindowCommand
	{
		public const int CommandId = 4129;

		public static readonly Guid CommandSet = new Guid("4986d6bb-e9d5-426c-aeea-22c34057aa21");

		private readonly Package package;

		private ModuleToolWindowCommand(Package package)
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
				var menuItem = new MenuCommand(this.ShowToolWindow, menuCommandId);
				commandService.AddCommand(menuItem);
			}
		}

		public static ModuleToolWindowCommand Instance
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
			Instance = new ModuleToolWindowCommand(package);
		}

		private void ShowToolWindow(object sender, EventArgs e)
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = this.package.FindToolWindow(typeof(ModuleToolWindow), 0, true);
			if ((null == window) || (null == window.Frame))
			{
				throw new NotSupportedException("Cannot create tool window");
			}

			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
		}
	}
}
