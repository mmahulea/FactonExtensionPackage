namespace FactonExtensionPackage.Modularity
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml.Serialization;

	[XmlRoot("factonModules", Namespace = "http://www.facton.com/infrastructure/modularity")]
	public class XmlModuleEntryList : XmlModuleElement, IModuleEntryList
	{
		private readonly List<XmlModuleElement> modules = new List<XmlModuleElement>();

		[XmlElement("module", typeof(XmlModuleEntry))]
		[XmlElement("moduleRegion", typeof(XmlModuleRegion))]
		public IList ModuleElements
		{
			get
			{
				return this.modules;
			}
		}

		[XmlIgnore]
		public IEnumerable<IModuleEntry> ModuleEntries
		{
			get
			{
				foreach (var element in this.modules)
				{
					var module = element as IModuleEntry;
					if (module != null)
					{
						yield return module;
					}
					else
					{
						var region = element as IModuleRegion;
						if (region != null)
						{
							foreach (var subModules in region.ModuleEntries)
							{
								yield return subModules;
							}
						}
					}
				}
			}
		}

		[XmlIgnore]
		public IEnumerable<IModuleRegion> ModuleRegions
		{
			get
			{
				foreach (var element in this.modules)
				{
					var region = element as IModuleRegion;
					if (region != null)
					{
						yield return region;
						foreach (var subRegion in region.ModuleRegions)
						{
							yield return subRegion;
						}
					}
				}
			}
		}

		////public List<ModuleDefinition> GetAllModuleDefinitions(DTE dte)
		//{
		//	List<ModuleDefinition> moduleDefinitions = new List<ModuleDefinition>();

		//	List<Project> projects = dte.GetProjects().ToList();

		//	Parallel.ForEach(this.ModuleEntries,
		//		moduleEntry =>
		//			{
		//				var projectItem = DteHelper.FindItemByName(dte, moduleEntry.ConfigFileName, projects);
		//				if (projectItem != null)
		//				{
		//					var moduleConfig = projectItem.Deserialize<XmlModuleConfig>();
		//					moduleDefinitions.Add(new ModuleDefinition(moduleEntry, moduleConfig));
		//				}
		//			});

		//	//foreach (var moduleEntry in this.ModuleEntries)
		//	//{
		//	//	var projectItem = DteHelper.FindItemByName(dte, moduleEntry.ConfigFileName, projects);
		//	//	if (projectItem != null)
		//	//	{
		//	//		var moduleConfig = projectItem.Deserialize<XmlModuleConfig>();
		//	//		moduleDefinitions.Add(new ModuleDefinition(moduleEntry, moduleConfig));
		//	//	}

		//	//	//var moduleConfiguration = this.ReadModuleConfiguration(moduleEntry);
		//	//	//moduleDefinitions.Add(new ModuleDefinition(moduleEntry, moduleConfiguration));
		//	//}

		//	return moduleDefinitions;
		//}
	}
}