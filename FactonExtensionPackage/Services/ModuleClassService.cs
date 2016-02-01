namespace FactonExtensionPackage.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class ModuleClassService
	{
		private static readonly Dictionary<string, string> serviceFullNameDictionary = new Dictionary<string, string>();

		public static bool VerifyConfiguration(ProjectItem projectItem)
		{
			var configProjectItem = SearchService.FindConfigFromModule(projectItem);
			if (configProjectItem == null)
			{
				throw new Exception($"Configuration file not found for module {projectItem.Name}");
			}
			string configText = configProjectItem.ReadAllText();

			var moduleText = projectItem.ReadAllText();
			var serviceNames =
				moduleText.Matches(@".GetObject\<(?<element>[^\>]+)\>")
					.Union(moduleText.Matches(@".GetRuntimeObject\<(?<element>[^\>]+)\>"))
					.Union(moduleText.Matches(@".RegisterInstance\<(?<element>[^\>]+)\>"));

			foreach (var serviceName in serviceNames)
			{
				var fullName = FindServiceFullName(serviceName);

				if (!configText.Contains(fullName))
				{
					throw new Exception($"{fullName} not found in the configuration file. Model: {projectItem.Name}");
				}
			}

			return false;
		}

		public static string GenerateConfig(ProjectItem module, ProjectItem config)
		{
			string moduleFileTxt = module.ReadAllText();
			string configFileTxt = config.ReadAllText();

			var requiredServices = GetReguiredServices(moduleFileTxt);
			var dependingServices = GetDependingServices(moduleFileTxt, configFileTxt);
			var providedServices = GetProvidedServices(moduleFileTxt);
			string classType = GetClassType(module);

			requiredServices = requiredServices.Where(s => dependingServices.All(d => d.ServiceName != s.ServiceName)).ToList();

			return GenerateConfigFileText(classType, requiredServices, dependingServices, providedServices);
		}

		private static List<IRequiredService> GetReguiredServices(string fileTxt)
		{
			var requiredServices = new List<IRequiredService>();

			var services = fileTxt.Matches(@".GetObject\<(?<element>[^\>]+)\>").Distinct();
			var runtimeServices = fileTxt.Matches(@".GetRuntimeObject\<(?<element>[^\>]+)\>").Distinct();

			foreach (var service in services)
			{
				var fullName = FindServiceFullName(service);
				requiredServices.Add(new XmlRequiredService { ServiceName = fullName, RequirementType = XmlRequirementType.Normal });
			}

			foreach (var service in runtimeServices)
			{
				var fullName = FindServiceFullName(service);
				requiredServices.Add(new XmlRequiredService { ServiceName = fullName, RequirementType = XmlRequirementType.RuntimeOnly });
			}

			return requiredServices;
		}

		private static List<IDependingService> GetDependingServices(string fileTxt, string configFileTxt)
		{
			var dependingServices = new List<IDependingService>();

			var currenRequiredServices = configFileTxt.Matches(@"\<requiredService name=\""(?<element>[^\""]+)\""").Distinct().ToList();
			var currentDependingServices = configFileTxt.Matches(@"\<dependingService name=\""(?<element>[^\""]+)\""").Distinct().ToList();

			var services = fileTxt.Matches(@".GetObject\<(?<element>[^\>]+)\>").Distinct();
			foreach (var service in services)
			{
				var fullName = FindServiceFullName(service);

				if (!currenRequiredServices.Contains(fullName) && (currentDependingServices.Contains(fullName) || service.EndsWith("Registry")))
				{
					dependingServices.Add(new XmlDependingService { ServiceName = fullName });
				}
			}

			return dependingServices;
		}

		private static List<IProvidedService> GetProvidedServices(string fileTxt)
		{
			var providedServices = new List<IProvidedService>();

			var registeredServices = fileTxt.Matches(@".RegisterInstance\<(?<element>[^\>]+)\>").Distinct();
			foreach (var registeredService in registeredServices)
			{
				var fullName = FindServiceFullName(registeredService);
				providedServices.Add(new XmlProvidedService { ServiceName = fullName });

			}
			return providedServices;
		}

		public static string GenerateConfigFileText(
			string classType,
			List<IRequiredService> requiredServices,
			List<IDependingService> dependingServices,
			List<IProvidedService> providedServices)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
			stringBuilder.AppendLine($"<moduleConfiguration type=\"{classType}\"");
			stringBuilder.AppendLine(@"                     xmlns=""http://www.facton.com/infrastructure/modularity"">" + Environment.NewLine);

			foreach (var service in requiredServices.OrderBy(s => s.ServiceName))
			{
				if (service.RequirementType == RequirementType.Normal)
				{
					stringBuilder.AppendLine($"<requiredService name=\"{service.ServiceName}\"/>");
				}
				else
				{
					stringBuilder.AppendLine($"<requiredService name=\"{service.ServiceName}\" requirementType=\"runtimeOnly\"/>");
				}
			}
			if (requiredServices.Any()) stringBuilder.AppendLine();

			foreach (var service in dependingServices.OrderBy(s => s.ServiceName))
			{
				stringBuilder.AppendLine($"<dependingService name=\"{service.ServiceName}\"/>");
			}
			if (dependingServices.Any()) stringBuilder.AppendLine();

			foreach (var service in providedServices.OrderBy(s => s.ServiceName))
			{
				stringBuilder.AppendLine($"<providedService name=\"{service.ServiceName}\"/>");
			}
			if (providedServices.Any()) stringBuilder.AppendLine();

			stringBuilder.Append("</moduleConfiguration>");

			var file = stringBuilder.ToString();
			file = file.Replace(" requirementType=\"normal\"", string.Empty).Replace(" />", "/>").Replace("?>", " ?>").Trim();
			return file;
		}

		private static string GetClassType(ProjectItem projectItem)
		{
			var name = projectItem.Name.Remove(".cs");
			var nameSpaceName = SearchService.FindNameSpace(projectItem).Name;
			var projectName = projectItem.ContainingProject.Name;

			return $"{nameSpaceName}.{name}, {projectName}";
		}

		private static string FindServiceFullName(string serviceName)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var interfaceName = serviceName.Split('.').Last();

			if (!serviceFullNameDictionary.ContainsKey(interfaceName))
			{
				var interfaceProjectItem = dte.Solution.FindProjectItem(interfaceName + ".cs");

				if (interfaceProjectItem == null)
				{
					throw new Exception($"Service {serviceName} not found in solution.");
				}
				var nameSpace = SearchService.FindNameSpace(interfaceProjectItem);
				var fullName = nameSpace.Name + "." + interfaceName;

				if (!fullName.EndsWith(serviceName))
				{
					throw new Exception($"Service {serviceName} not found in solution.");
				}

				serviceFullNameDictionary.Add(interfaceName, fullName);
			}

			return serviceFullNameDictionary[interfaceName];
		}

		public static bool CompareTwoConfigFiles(string file1, string file2)
		{
			return file1.CompareAsConfigFile(file2);
		}
	}
}