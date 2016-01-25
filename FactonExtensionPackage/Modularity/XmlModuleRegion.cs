namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public class XmlModuleRegion : XmlModuleEntryList, IModuleRegion
	{
		/// <summary>
		/// Gets or sets the name of the config file used to get additional entries of the region.
		/// </summary>
		[XmlAttribute("configFile")]
		public string ConfigFileName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this module region represents the plug-in configuration.
		/// </summary>
		[XmlAttribute("isPluginConfiguration")]
		public bool IsPluginConfiguration { get; set; }

		/// <summary>
		/// Gets the name of the module region.
		/// </summary>
		[XmlIgnore]
		public string RegionName
		{
			get { return this.Name; }
		}

		/// <summary>
		/// Gets a value indicating whether the region has an external additional config file.
		/// </summary>
		[XmlIgnore]
		public bool HasConfigFile
		{
			get { return !string.IsNullOrEmpty(this.ConfigFileName); }
		}
	}
}