namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using System.Linq;
	using System.Text;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public class FactonizeModuleCommand
	{
		public static void Execute(ProjectItem projectItem)
		{
			if (projectItem.Name.EndsWith(".config", StringComparison.CurrentCulture) && projectItem.Contains("<moduleConfiguration") && !projectItem.ContainingProject.IsTestProject())
			{
				var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
				string configFile = projectItem.ReadAllText();

				var moduleConfig = configFile.Deserialize<XmlModuleConfig>();
				if (moduleConfig != null)
				{
					var project = dte.Solution.FindProject(p => p.Name == moduleConfig.AssemblyName);
					var moduleProjectItem = project.FindProjectItem(p => p.Name == moduleConfig.CsFileName);

					string csFile = moduleProjectItem.ReadAllText();

					string newConfig = DetermineNewConfig(configFile, csFile);

					if (!(configFile.EqualsExcludingWhitespace(newConfig)))
					{
						projectItem.Document?.Activate();
						projectItem.ClearAllText();
						projectItem.SetText(newConfig);
					}
				}
			}
		}

		public static string DetermineNewConfig(string configFile, string csFile)
		{
			var services = csFile.Matches(@".GetObject\<(?<element>[^\>]+)\>").ToList();
			var runtimeServices = csFile.Matches(@".GetRuntimeObject\<(?<element>[^\>]+)\>").ToList();
			var providedServices = csFile.Matches(@".RegisterInstance\<(?<element>[^\>]+)\>").ToList();
			var stubServices = csFile.Matches(@"ModuleStub\<(?<element>[^\>]+)\>").ToList();
			if (stubServices.Any())
			{
				services.Add("IModuleLoader");
				services.Add("IActionDispatcherService");
			}

			var moduleConfig = configFile.Deserialize<XmlModuleConfig>();

			var newModuleConfig = new XmlModuleConfig
			{
				ImplementationType = moduleConfig.ImplementationType,
				RequiredServices =
					moduleConfig.RequiredServices.OfType<IRequiredService>()
					.Where(s => services.Union(runtimeServices).Union(stubServices).Any(t => s.ServiceName.EndsWith(t, StringComparison.CurrentCulture)))
					.OrderBy(s => s.ServiceName)
					.ToList(),
				DependingServices =
					moduleConfig.DependingServices.OfType<IDependingService>()
					.Where(s => services.Union(runtimeServices).Union(stubServices).Any(t => s.ServiceName.EndsWith(t, StringComparison.CurrentCulture)))
					.OrderBy(s => s.ServiceName)
					.ToList(),
				ProvidedServices =
					moduleConfig.ProvidedServices.OfType<IProvidedService>()
					.Where(s => providedServices.Any(t => s.ServiceName.EndsWith(t, StringComparison.CurrentCulture)))
					.OrderBy(s => s.ServiceName)
					.ToList(),
			};

			var newText = newModuleConfig.Serialize();

			var lines = newText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Distinct().ToList();
			var newString = new StringBuilder();
			newString.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
			newString.AppendLine($"<moduleConfiguration type=\"{moduleConfig.ImplementationType}\"");
			newString.AppendLine(@"                     xmlns=""http://www.facton.com/infrastructure/modularity"">" + Environment.NewLine);

			foreach (var line in lines.Where(l => l.StartsWith("<requiredService ")))
			{
				newString.AppendLine(line);
			}
			if (lines.Any(l => l.StartsWith("<requiredService "))) newString.AppendLine();

			foreach (var line in lines.Where(l => l.StartsWith("<dependingService ")))
			{
				newString.AppendLine(line);
			}
			if (lines.Any(l => l.StartsWith("<dependingService "))) newString.AppendLine();

			foreach (var line in lines.Where(l => l.StartsWith("<providedService ")))
			{
				newString.AppendLine(line);
			}
			if (lines.Any(l => l.StartsWith("<providedService "))) newString.AppendLine();

			newString.Append("</moduleConfiguration>");

			string file = newString.ToString();
			file = file.Replace(" requirementType=\"normal\"", string.Empty).Replace(" />", "/>").Replace("?>", " ?>").Trim();
			return file;
		}
	}
}
