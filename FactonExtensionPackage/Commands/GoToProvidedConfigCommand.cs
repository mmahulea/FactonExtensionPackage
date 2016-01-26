namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class GoToProvidedConfigCommand
	{
		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("31896876-0e72-4bc6-8547-d797e20f19b8");

		private readonly Package package;

		private GoToProvidedConfigCommand(Package package)
		{
			this.package = package;

			var commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandId = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
				menuItem.BeforeQueryStatus += MenuItemBeforeQueryStatus;
				commandService.AddCommand(menuItem);
			}
		}

		public static GoToProvidedConfigCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new GoToProvidedConfigCommand(package);
		}

		private static void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			TextSelection txtSel = (TextSelection)dte.ActiveDocument.Selection;
			var selectedText = txtSel.Text;
			var cmd = (OleMenuCommand)sender;
			if (selectedText != null && selectedText.StartsWith("I") && selectedText.Length > 2)
			{
				cmd.Visible = true;
			}
			else
			{
				cmd.Visible = false;
			}
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			TextSelection txtSel = (TextSelection)dte.ActiveDocument.Selection;
			var selectedText = txtSel.Text;

			if (selectedText.StartsWith("I") && selectedText.Length > 2)
			{
				var searchedProjectItem =
					dte.Solution.FindProjectItem(
						project =>
						!project.Name.Contains(".Test") && !project.Name.Contains(".TestTool") && !project.Name.Contains(".IntegrationTesting.")
						&& project.Name.Contains("Facton"),
						projectItem =>
							{
								if (projectItem.Name.EndsWith(".config"))
								{
									var moduleConfig = projectItem.Deserialize<XmlModuleConfig>();
									if (moduleConfig != null)
									{
										return moduleConfig.ProvidedServices.Cast<IProvidedService>().Any(s => s.ServiceName.EndsWith("." + selectedText));
									}
								}
								return false;
							});
				if (searchedProjectItem != null)
				{
					searchedProjectItem.OpenInEditor();
				}
				else
				{
					VsShellUtilities.ShowMessageBox(
						this.ServiceProvider,
						 $"'{selectedText}' not found!",
						"Facton extension package",
						OLEMSGICON.OLEMSGICON_INFO,
						OLEMSGBUTTON.OLEMSGBUTTON_OK,
						OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				}
			}
		}
	}
}
