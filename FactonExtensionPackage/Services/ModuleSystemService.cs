namespace FactonExtensionPackage.Services
{
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class ModuleSystemService
	{
		public static IModuleConfig GetModuleInfo(string name)
		{
			var modules = LoadModules();
			var module = modules.FirstOrDefault(m => m.ModuleName == name);
			return module;
		}

		public static List<IModuleConfig> LoadModules()
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

			var modules = new List<IModuleConfig>();
			var services = new List<Service>();
			var query = dte.Solution.FindProjectItems(
				p => p.Name.Contains("Facton") && !p.IsTestProject(),
				p => p.Name.EndsWith(".config") && p.Name != "App.config");

			foreach (var projectItem in query.OrderBy(p => p.Name))
			{
				var module = projectItem.Deserialize<XmlModuleConfig>();
				if (module != null)
				{
					module.ModuleName = projectItem.Name;
					foreach (var providedService in module.As<IModuleConfig>().ProvidedServices)
					{
						var service = GetOrAddService(services, providedService.ServiceName);
						service.ProvidedByModule = module;
						providedService.Service = service;
					}
					foreach (var requiredService in module.As<IModuleConfig>().RequiredServices)
					{
						var service = GetOrAddService(services, requiredService.ServiceName);
						service.RequiredInModules.Add(module);
						requiredService.Service = service;
					}
					foreach (var dependingService in module.As<IModuleConfig>().DependingServices)
					{
						var service = GetOrAddService(services, dependingService.ServiceName);
						service.DependingByModules.Add(module);
						dependingService.Service = service;
					}
				}
				modules.Add(module);
			}

			return modules;
		}

		private static Service GetOrAddService(List<Service> services, string name)
		{
			var service = services.FirstOrDefault(s => s.Name == name);
			if (service == null)
			{
				service = new Service { Name = name };
				services.Add(service);
			}
			return service;
		}
	}
}