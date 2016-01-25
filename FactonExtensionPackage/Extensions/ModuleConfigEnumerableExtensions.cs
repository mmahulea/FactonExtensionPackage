namespace FactonExtensionPackage.Extensions
{
	using System.Collections.Generic;
	using System.Linq;
	using FactonExtensionPackage.Modularity;

	public static class ModuleConfigEnumerableExtensions
	{
		public static List<IRequiredService> DetermineMissingServices(this IEnumerable<XmlModuleConfig> modules, List<string> bootStrappedServices)
		{
			var providedServices = new List<IProvidedService>();
			var requiredServices = new Dictionary<IRequiredService, XmlModuleConfig>();

			foreach (var module in modules)
			{
				foreach (IRequiredService requiredService in module.RequiredServices)
				{
					if (requiredServices.All(r => r.Key.ServiceName != requiredService.ServiceName))
					{
						requiredServices.Add(requiredService, module);
					}
				}

				foreach (IProvidedService providedService in module.ProvidedServices)
				{
					if (providedServices.All(r => r.ServiceName != providedService.ServiceName))
					{
						providedServices.Add(providedService);
					}
				}
			}

			var query =
				requiredServices.Where(
					requiredService =>
					providedServices.All(s => s.ServiceName != requiredService.Key.ServiceName)
					&& bootStrappedServices.All(s => s != requiredService.Key.ServiceName));

			return query.Select(s => s.Key).ToList();
		}

		public static List<XmlModuleConfig> DetermineMissingModules(
			this IEnumerable<XmlModuleConfig> modules,
			List<string> bootStrappedServices)
		{
			var providedServices = new List<IProvidedService>();
			var requiredServices = new Dictionary<IRequiredService, XmlModuleConfig>();

			foreach (var module in modules)
			{
				foreach (IRequiredService requiredService in module.RequiredServices)
				{
					if (requiredServices.All(r => r.Key.ServiceName != requiredService.ServiceName))
					{
						requiredServices.Add(requiredService, module);
					}
				}

				foreach (IProvidedService providedService in module.ProvidedServices)
				{
					if (providedServices.All(r => r.ServiceName != providedService.ServiceName))
					{
						providedServices.Add(providedService);
					}
				}
			}

			return
				requiredServices.Where(
					requiredService =>
					providedServices.All(s => s.ServiceName != requiredService.Key.ServiceName)
					&& bootStrappedServices.All(s => s != requiredService.Key.ServiceName))
					.Select(k => k.Value).ToList();
		}
	}
}