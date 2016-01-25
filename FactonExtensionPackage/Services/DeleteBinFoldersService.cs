namespace FactonExtensionPackage.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.Extensions;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	static class DeleteBinFoldersService
	{
		public static void Delete()
		{
			var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
			dte.UndoContext.Open("ctx");
			try
			{
				var projects = dte.GetSelectedProjectInSolutionExplorer();

				var deleted = new List<string>();
				var failures = new List<string>();
				foreach (var project in projects)
				{
					var path = project.FullName;
					path = path.Replace(project.Name + ".csproj", "bin");

					if (Directory.Exists(path))
					{
						try
						{
							var di = new DirectoryInfo(path);
							di.Attributes &= ~FileAttributes.ReadOnly;
							di.Delete(true);
							deleted.Add(path);
						}
						catch
						{
							failures.Add(path);
						}
					}
				}

				var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
				var outputWindow = (OutputWindow)window.Object;
				outputWindow.ActivePane.Activate();

				deleted.Sort();
				failures.Sort();
				foreach (var txt in deleted)
				{
					outputWindow.ActivePane.OutputString(Environment.NewLine + "Deleted: " + txt);
				}
				outputWindow.ActivePane.OutputString(Environment.NewLine + $"============Deleted {deleted.Count} folder(s)============");
				foreach (var failure in failures)
				{
					outputWindow.ActivePane.OutputString(Environment.NewLine + "Could not delete: " + failure);
				}
			}
			finally
			{
				dte.UndoContext.Close();
			}
		}
	}
}
