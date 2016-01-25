namespace FactonExtensionPackage.Modularity
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.UI.WebControls;
	using System.Xml.Serialization;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;

	[XmlRoot("moduleConfiguration", Namespace = "http://www.facton.com/infrastructure/modularity")]
	public class XmlModuleConfig : IModuleConfig
	{
		private List<IDependingService> dependingServices = new List<IDependingService>();

		private List<IProvidedService> providedServices = new List<IProvidedService>();

		private List<IRequiredService> requiredServices = new List<IRequiredService>();

		private List<string> requiredModulesNames;

		private List<string> dependingModulesNames;

		public XmlModuleConfig()
		{
			//RequiredModules = new List<IModuleConfig>();
			//DependingModules = new List<IModuleConfig>();
		}

		[XmlElement("requiredService", typeof(XmlRequiredService))]
		public IList RequiredServices
		{
			get
			{
				return this.requiredServices;
			}
			set
			{
				this.requiredServices = value.OfType<IRequiredService>().ToList();
			}
		}

		[XmlElement("dependingService", typeof(XmlDependingService))]
		public IList DependingServices
		{
			get
			{
				return this.dependingServices;
			}
			set
			{
				this.dependingServices = value.OfType<IDependingService>().ToList();
			}
		}

		[XmlElement("providedService", typeof(XmlProvidedService))]
		public IList ProvidedServices
		{
			get
			{
				return this.providedServices;
			}
			set
			{
				this.providedServices = value.OfType<IProvidedService>().ToList();
			}
		}

		[XmlAttribute("type")]
		public string ImplementationType { get; set; }

		[XmlIgnore]
		public string CsFileName => !string.IsNullOrWhiteSpace(this.ImplementationType)
										? this.ImplementationType.Split(',')[0].Trim().Split('.').ToList().LastOrDefault() + ".cs"
										: default(string);

		[XmlIgnore]
		public string AssemblyName
			=> !string.IsNullOrWhiteSpace(this.ImplementationType) ? this.ImplementationType.Split(',')[1].Trim() : default(string);

		[XmlIgnore]
		public string ModuleName { get; set; }

		IEnumerable<IDependingService> IModuleConfig.DependingServices
		{
			get
			{
				return this.dependingServices;
			}
		}

		IEnumerable<IProvidedService> IModuleConfig.ProvidedServices
		{
			get
			{
				return this.providedServices;
			}
		}

		IEnumerable<IRequiredService> IModuleConfig.RequiredServices
		{
			get
			{
				return this.requiredServices;
			}
		}

		//[XmlIgnore]
		//public List<IModuleConfig> RequiredModules { get; set; }

		//[XmlIgnore]
		//public List<IModuleConfig> DependingModules { get; set; }

		[XmlIgnore]
		public List<string> RequiredModulesNames
		{
			get
			{
				if (this.requiredModulesNames == null || !this.requiredModulesNames.Any())
				{
					var query = this.requiredServices.Select(GetServiceName).Distinct().OrderBy(s => s);
					this.requiredModulesNames = query.ToList();
				}
				return this.requiredModulesNames;
			}
		}

		[XmlIgnore]
		public List<string> DependingModulesNames
		{
			get
			{
				if (this.dependingModulesNames == null || !this.dependingModulesNames.Any())
				{
					var query = this.dependingServices.Select(GetServiceName).Distinct().OrderBy(s => s);
					this.dependingModulesNames = query.ToList();
				}
				return this.dependingModulesNames;
			}
		}

		private static string GetServiceName(IDependingService service)
		{
			string name = GetServiceName(service.Service);
			if (string.IsNullOrWhiteSpace(name))
			{
				name = service.ServiceName;
			}
			return name;
		}

		private static string GetServiceName(IRequiredService service)
		{
			string name = GetServiceName(service.Service);
			if (string.IsNullOrWhiteSpace(name))
			{
				name = service.ServiceName;
			}
			if (service.RequirementType == RequirementType.RuntimeOnly)
			{
				name += " - runtimeOnly";
			}
			return name;
		}

		private static string GetServiceName(Service service)
		{
			if (service != null)
			{
				return service.ProvidedByModule != null ? service.ProvidedByModule.ModuleName : service.Name;
			}

			return string.Empty;
		}

		//[XmlIgnore]
		//public List<IModuleConfig> DependingModules
		//{
		//	get
		//	{
		//		return this.dependingServices.Select(m => m.Service.ProvidedByModule).ToList();
		//	}
		//}

		//[XmlIgnore]
		//public List<IModuleConfig> RequiredModules
		//{
		//	get
		//	{
		//		return this.requiredServices.Select(m => m.Service.ProvidedByModule).ToList();
		//	}
		//}

		//[XmlIgnore]
		//public List<IModuleConfig> RequirementTree
		//{
		//	get
		//	{
		//		return this.GetRequiredModulesRecursively(this).ToList();
		//	}
		//}

		//public IEnumerable<IModuleConfig> GetRequiredModulesRecursively(IModuleConfig moduleConfig)
		//{
		//	List<IModuleConfig> modules = new List<IModuleConfig>();
		//	foreach (var requiredService in moduleConfig.RequiredServices)
		//	{
		//		var requiredModule = requiredService.Service.ProvidedByModule;
		//		if (requiredModule != null)
		//		{
		//			modules.Add(requiredModule);
		//			if (requiredService.RequirementType == RequirementType.Normal)
		//			{
		//				foreach (var service in requiredModule.RequiredServices)
		//				{
		//					if (service?.Service?.ProvidedByModule != null)
		//					{
		//						modules.AddRange(this.GetRequiredModulesRecursively(service.Service.ProvidedByModule));
		//					}
		//				}
		//			}
		//		}
		//	}
		//	return modules;
		//}
	}

	public class Service
	{
		public Service()
		{
			this.RequiredInModules = new List<IModuleConfig>();
			this.DependingByModules = new List<IModuleConfig>();
		}

		public string Name { get; set; }

		public IModuleConfig ProvidedByModule { get; set; }

		public List<IModuleConfig> RequiredInModules { get; private set; }

		public List<IModuleConfig> DependingByModules { get; private set; }
	}
}