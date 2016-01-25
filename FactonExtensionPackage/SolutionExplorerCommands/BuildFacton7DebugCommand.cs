namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using System.IO;
	using EnvDTE;
	using EnvDTE80;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class BuildFacton7DebugCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("498f8d3a-f294-4c2a-8341-2dd8aa2ff6e6");

		private readonly Package package;

		private BuildFacton7DebugCommand(Package package)
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

		public static BuildFacton7DebugCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new BuildFacton7DebugCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
			var fileName = dte.Solution.FileName;
			var directory = fileName.Substring(0, fileName.LastIndexOf(@"\"));
			fileName = directory + @"\Build\Build-Facton7-Debug.cmd";

			if (File.Exists(fileName))
			{
				var process = new System.Diagnostics.Process();
				process.StartInfo.FileName = fileName;
				process.StartInfo.WorkingDirectory = directory;
				process.Start();
			}
			else
			{
				var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
				var outputWindow = (OutputWindow)window.Object;
				outputWindow.ActivePane.Activate();
				string msg = $"Couldn't find file: {fileName}";
				outputWindow.ActivePane.OutputString(Environment.NewLine + msg);
			}
		}
	}
}
