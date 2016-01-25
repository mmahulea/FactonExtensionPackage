namespace FactonExtensionPackage.Modularity
{
	using System.Collections.Generic;

	public interface IModuleConfig
	{
		IEnumerable<IDependingService> DependingServices { get; }

		string ImplementationType { get; }

		IEnumerable<IProvidedService> ProvidedServices { get; }

		string ModuleName { get; set; }

		IEnumerable<IRequiredService> RequiredServices { get; }

		//List<IModuleConfig> RequiredModules { get; }

		//List<IModuleConfig> DependingModules { get; }

		List<string> RequiredModulesNames { get; }

		List<string> DependingModulesNames { get; }
	}
}