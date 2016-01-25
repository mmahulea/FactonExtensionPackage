namespace FactonExtensionPackage.Modularity
{
	using System.Collections.Generic;

	public interface IModuleEntryList
	{
		/// <summary>
		/// Gets the module entries of this configuration.
		/// </summary>
		IEnumerable<IModuleEntry> ModuleEntries { get; }

		/// <summary>
		/// Gets the module regions of this configuration.
		/// </summary>
		IEnumerable<IModuleRegion> ModuleRegions { get; }
	}
}