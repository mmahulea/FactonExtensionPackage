namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
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
			const RegexOptions RegexOptions = RegexOptions.Multiline | RegexOptions.Singleline;
			var services = (from Match match in new Regex(@".GetObject\<(?<element>[^\>]+)\>", RegexOptions).Matches(csFile)
							select match.Groups[1].Value).ToList();
			var runtimeServices = (from Match match in new Regex(@".GetRuntimeObject\<(?<element>[^\>]+)\>", RegexOptions).Matches(csFile)
								   select match.Groups[1].Value);
			var providedServices = (from Match match in new Regex(@".RegisterInstance\<(?<element>[^\>]+)\>", RegexOptions).Matches(csFile)
									select match.Groups[1].Value);

			var stubServices = (from Match match in new Regex(@"ModuleStub\<(?<element>[^\>]+)\>", RegexOptions).Matches(csFile)
								select match.Groups[1].Value).ToList();
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
			newString.Append($"<moduleConfiguration type=\"{moduleConfig.ImplementationType}\"" + Environment.NewLine);
			newString.Append(@"                     xmlns=""http://www.facton.com/infrastructure/modularity"">" + Environment.NewLine + Environment.NewLine);

			foreach (var line in lines.Where(l => l.StartsWith("<requiredService ", StringComparison.CurrentCulture)))
			{
				newString.Append(line);
			}
			newString.Append(Environment.NewLine);
			foreach (var line in lines.Where(l => l.StartsWith("<dependingService ", StringComparison.CurrentCulture)))
			{
				newString.Append(line);
			}
			newString.Append(Environment.NewLine);
			foreach (var line in lines.Where(l => l.StartsWith("<providedService ", StringComparison.CurrentCulture)))
			{
				newString.Append(line);
			}
			newString.Append(Environment.NewLine + "</moduleConfiguration>");

			string file = newString.ToString();
			file = file.Replace(" requirementType=\"normal\"", string.Empty).Replace(" />", "/>").Replace("?>", " ?>").Trim();
			return file;
		}
	}
}
