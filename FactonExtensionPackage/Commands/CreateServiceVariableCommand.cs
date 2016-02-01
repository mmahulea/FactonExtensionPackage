
namespace FactonExtensionPackage.Commands
{
	using System;
	using System.ComponentModel.Design;
	using EnvDTE;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class CreateServiceVariableCommand
	{
		private string serviceName;

		public const int CommandId = 256;

		public static readonly Guid CommandSet = new Guid("cfab47f2-15d3-4cda-b9bc-d9cf0fb9ccd5");

		private readonly Package package;

		private CreateServiceVariableCommand(Package package)
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

		public static CreateServiceVariableCommand Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider => this.package;

		public static void Initialize(Package package)
		{
			Instance = new CreateServiceVariableCommand(package);
		}

		private void MenuItemBeforeQueryStatus(object sender, EventArgs e)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			var txtSel = (TextSelection)dte.ActiveDocument.Selection;
			this.serviceName = txtSel.Text;
			var cmd = (OleMenuCommand)sender;
			if (this.serviceName != null && this.serviceName.StartsWith("I") && this.serviceName.Length > 2)
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
			var txtSel = (TextSelection)dte.ActiveDocument.Selection;
			string variableName = this.serviceName[1].ToString().ToLower() + this.serviceName.Substring(2);

			txtSel.ReplaceText(this.serviceName, $"var {variableName} = typeRegistry.GetObject<{this.serviceName}>();");
		}
	}
}
