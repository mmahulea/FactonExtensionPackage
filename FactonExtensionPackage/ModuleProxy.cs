namespace FactonExtensionPackage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;

	public class ModuleProxy
	{
		public Dictionary<RequiredService, ModuleProxy> Children = new Dictionary<RequiredService, ModuleProxy>();

		public void AddChild(ModuleProxy child, RequiredService requiredService)
		{
			if (!this.Children.ContainsKey(requiredService))
			{
				this.Children.Add(requiredService, child);
			}
		}

		public List<ModuleProxy> Parents = new List<ModuleProxy>();

		public List<ModuleProxy> Dependents = new List<ModuleProxy>();

		private readonly ProjectItem projectItem;

		private string key;

		private string moduleName;

		private string text;

		private XmlModuleConfig moduleConfig;

		private List<string> providedServices;

		private List<RequiredService> requiredServices;

		private List<string> dependingServices;

		private bool loaded;

		public ModuleProxy(ProjectItem projectItem)
		{
			this.projectItem = projectItem;
		}

		public ModuleProxy(ProjectItem projectItem, string text, XmlModuleConfig moduleConfig)
		{
			this.projectItem = projectItem;
			this.text = text;
			this.moduleConfig = moduleConfig;
		}

		public string Key
		{
			get
			{
				if (this.key == null)
				{
					if (this.projectItem.ContainingProject.IsTestProject())
					{
						this.key = this.projectItem.ContainingProject.Name + "-" + this.ModuleName;
					}
					else
					{
						this.key = this.ModuleName;
					}
				}
				return this.key;
			}
		}

		public string ModuleName => this.moduleName ?? (this.moduleName = this.projectItem.ModuleName());

		public string ConfigFileName => this.projectItem.Name;

		public string AssemblyName => this.projectItem.ContainingProject.Name;

		public string Text => this.text ?? (this.text = this.projectItem.ReadAllText());

		public List<string> ProvidedServices
		{
			get
			{
				this.Load();
				if (this.providedServices == null)
				{
					if (this.moduleConfig == null)
					{
						this.providedServices = new List<string>();
					}
					else
					{
						this.providedServices = this.moduleConfig.ProvidedServices.Cast<IProvidedService>().Select(p => p.ServiceName).ToList();
					}
				}
				return this.providedServices;
			}
		}

		public List<RequiredService> RequiredServices
		{
			get
			{
				this.Load();
				if (this.requiredServices == null)
				{
					if (this.moduleConfig == null)
					{
						this.requiredServices = new List<RequiredService>();
					}
					else
					{
						this.requiredServices =
							this.moduleConfig.RequiredServices.Cast<IRequiredService>()
								.Select(p => new RequiredService(p.ServiceName, p.RequirementType))
								.ToList();
					}
				}
				return this.requiredServices;
			}
		}

		public List<string> DependingServices
		{
			get
			{
				this.Load();
				if (this.dependingServices == null)
				{
					if (this.moduleConfig == null)
					{
						this.dependingServices = new List<string>();
					}
					else
					{
						this.dependingServices = this.moduleConfig.DependingServices.Cast<IDependingService>().Select(p => p.ServiceName).ToList();
					}
				}
				return this.dependingServices;
			}
		}

		public string FirstPath
		{
			get
			{
				var splits = this.ConfigFileName.Split('.');
				if (splits.Count() >= 2)
				{
					return splits[1];
				}
				return splits[0];
			}
		}

		public void Load()
		{
			if (!this.loaded)
			{
				try
				{
					if (this.projectItem.Name.EndsWith(".config"))
					{
						this.moduleConfig = this.projectItem.Deserialize<XmlModuleConfig>();
					}
				}
				catch (Exception)
				{
					// ignore										
				}
				finally
				{
					this.loaded = true;
				}
			}
		}

		public void OpenInEditor()
		{
			this.projectItem.OpenInEditor();
		}
	}

	public class RequiredService
	{
		public RequiredService(string name, RequirementType requirementType)
		{
			this.Name = name;
			switch (requirementType)
			{
				case RequirementType.Normal:
					this.Type = "Normal"; break;
				case RequirementType.OnDemand:
					this.Type = "OnDemand"; break;
				case RequirementType.RuntimeOnly:
					this.Type = "RuntimeOnly"; break;
			}
		}
		public string Name { get; set; }

		public string Type { get; set; }

		public override string ToString()
		{
			if (this.Type != "Normal")
			{
				return this.Name + " - " + this.Type;
			}
			return this.Name;
		}

		public override bool Equals(object obj)
		{
			if (obj is RequiredService)
			{
				return ((RequiredService)obj).Name == this.Name;
			}
			return base.Equals(obj);
		}
	}
}