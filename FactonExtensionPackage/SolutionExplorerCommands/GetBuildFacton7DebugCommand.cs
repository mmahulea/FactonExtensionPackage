namespace FactonExtensionPackage.SolutionExplorerCommands
{
	using System;
	using System.ComponentModel.Design;
	using System.IO;
	using EnvDTE;
	using EnvDTE80;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class GetBuildFacton7DebugCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("f2eb0f74-0a5d-4299-90b8-981f9ba1bc3f");

		private readonly Package package;

		private GetBuildFacton7DebugCommand(Package package)
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

		public static GetBuildFacton7DebugCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new GetBuildFacton7DebugCommand(package);
		}

		private static void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
			var fileName = dte.Solution.FileName;
			var directory = fileName.Substring(0, fileName.LastIndexOf(@"\"));
			fileName = directory + @"\Build\Get-Build-Facton7-Debug.cmd";

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
