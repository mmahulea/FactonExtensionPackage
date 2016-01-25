namespace FactonExtensionPackage.Modularity
{
	using System.Collections.Generic;

	public interface IModuleEntry
	{
		string ModuleName { get; }

		string ConfigFileName { get; }

		bool IsStartup { get; }

		int StartupOrder { get; }

		IEnumerable<IRequiredModule> RequiredModules { get; }
	}
}