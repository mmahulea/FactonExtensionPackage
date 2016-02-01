namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Services;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class UpdateConfigCommand
	{
		private bool isModuleClass;

		private bool isConfigFile;

		public const int CommandId = 4129;

		public static readonly Guid CommandSet = new Guid("c56c7a6d-d1a4-4f7a-a62e-841eb59fd89e");

		private readonly Package package;

		private UpdateConfigCommand(Package package)
		{
			this.package = package;

			var commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
				commandService.AddCommand(menuCommand);
				menuCommand.BeforeQueryStatus += this.MenuItemBeforeQueryStatus;
			}
		}

		public static UpdateConfigCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new UpdateConfigCommand(package);
		}

		private void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			this.isModuleClass = dte.ActiveDocument.ProjectItem.IsModuleClass();
			if (!this.isModuleClass)
			{
				this.isConfigFile = dte.ActiveDocument.ProjectItem.IsModuleConfig();
			}
			((OleMenuCommand)sender).Visible = this.isModuleClass || this.isConfigFile;
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			ProjectItem module = null;
			ProjectItem config = null;
			if (this.isModuleClass)
			{
				module = dte.ActiveDocument.ProjectItem;
				config = SearchService.FindConfigFromModule(module);
			}
			else
			{
				config = dte.ActiveDocument.ProjectItem;
				module = SearchService.FindModuleFromConfig(config);
			}

			dte.UndoContext.Open("ctx");
			try
			{
				if (dte.ActiveDocument.ProjectItem.IsDirty)
				{
					dte.ActiveDocument.Save();
				}

				string existingConfigText = config.ReadAllText();
				string generatedConfigText = ModuleClassService.GenerateConfig(module, config);

				if (!ModuleClassService.CompareTwoConfigFiles(generatedConfigText, existingConfigText))
				{
					config.OpenInEditor();
					config.ClearAllText();
					config.SetText(generatedConfigText);
					dte.FormatDocument();
				}
				else if (!config.IsOpen)
				{
					config.OpenInEditor();
				}
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}
	}
}
