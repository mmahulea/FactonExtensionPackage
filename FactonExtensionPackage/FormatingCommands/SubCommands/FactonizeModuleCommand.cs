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
								   select match.Groups[1].Value).ToList();
			var providedServices = (from Match match in new Regex(@".RegisterInstance\<(?<element>[^\>]+)\>", RegexOptions).Matches(csFile)
									select match.Groups[1].Value).ToList();

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

			bool requiredServiceAdded = false;
			bool dependendServiceAdded = false;
			bool providedServiceAdded = false;
			var sb = new StringBuilder();
			foreach (var line in newText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Trim().StartsWith("<moduleConfiguration ", StringComparison.CurrentCulture))
				{
					sb.Append($"<moduleConfiguration type=\"{moduleConfig.ImplementationType}\"");
					sb.Append(Environment.NewLine);
					sb.Append(@"                     xmlns=""http://www.facton.com/infrastructure/modularity"">");
					sb.Append(Environment.NewLine);
					continue;
				}

				if (line.TrimStart().StartsWith("<requiredService ", StringComparison.CurrentCulture) && !requiredServiceAdded)
				{
					requiredServiceAdded = true;
					sb.Append(Environment.NewLine);
				}

				if (line.TrimStart().StartsWith("<dependingService ", StringComparison.CurrentCulture) && !dependendServiceAdded)
				{
					dependendServiceAdded = true;
					sb.Append(Environment.NewLine);
				}

				if (line.TrimStart().StartsWith("<providedService ", StringComparison.CurrentCulture) && !providedServiceAdded)
				{
					providedServiceAdded = true;
					sb.Append(Environment.NewLine);
				}

				if (line.Trim().Equals("</moduleConfiguration>"))
				{
					sb.Append(Environment.NewLine);
				}

				sb.Append(line);
				sb.Append(Environment.NewLine);
			}

			string file = sb.ToString();
			file = file.Replace(" requirementType=\"normal\"", string.Empty).Replace(" />", "/>").Replace("?>", " ?>").Trim();
			return file;
		}
	}
}
