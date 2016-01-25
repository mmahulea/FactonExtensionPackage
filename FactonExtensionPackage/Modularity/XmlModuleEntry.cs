namespace FactonExtensionPackage.Modularity
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml.Serialization;

	public class XmlModuleEntry : XmlModuleElement, IModuleEntry
	{
		private readonly List<IRequiredModule> requiredModules = new List<IRequiredModule>();

		[XmlAttribute("configFile")]
		public string XmlConfigFileName { get; set; }

		[XmlAttribute("startUp")]
		public bool IsStartup { get; set; }

		[XmlAttribute("startOrder")]
		public int XmlStartOrder { get; set; }

		[XmlElement("requiredModule", typeof(XmlRequiredModule))]
		public IList RequiredModules
		{
			get { return this.requiredModules; }
		}

		[XmlIgnore]
		public string ModuleName
		{
			get { return this.Name; }
		}

		[XmlIgnore]
		public string ConfigFileName
		{
			get { return string.IsNullOrEmpty(this.XmlConfigFileName) ? this.ModuleName + ".config" : this.XmlConfigFileName; }
		}

		[XmlIgnore]
		public int StartupOrder
		{
			get { return this.XmlStartOrder == 0 ? 50 : this.XmlStartOrder; }
		}

		IEnumerable<IRequiredModule> IModuleEntry.RequiredModules
		{
			get { return this.requiredModules; }
		}
	}
}