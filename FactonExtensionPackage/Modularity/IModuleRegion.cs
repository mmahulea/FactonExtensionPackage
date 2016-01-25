namespace FactonExtensionPackage.Modularity
{
	public interface IModuleRegion : IModuleEntryList
	{
		/// <summary>
		/// Gets the name of the module region.
		/// </summary>
		string RegionName { get; }

		/// <summary>
		/// Gets a value indicating whether the region has an external additional config file.
		/// </summary>
		bool HasConfigFile { get; }

		/// <summary>
		/// Gets the name of the config file used to get additional entries of the region.
		/// </summary>
		string ConfigFileName { get; }

		/// <summary>
		/// Gets a value indicating whether this module region represents the plug-in configuration.
		/// </summary>
		bool IsPluginConfiguration { get; }
	}
}