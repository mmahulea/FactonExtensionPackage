namespace FactonExtensionPackage.Modularity
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;

	public class ModuleDefinition
	{
		private readonly IModuleConfig moduleConfiguration;

		private readonly IModuleEntry moduleEntry;

		private readonly string comparableImplementationType;

		public ModuleDefinition(IModuleEntry moduleEntry, IModuleConfig moduleConfiguration)
		{
			if (moduleEntry == null)
			{
				throw new ArgumentNullException(nameof(moduleEntry));
			}

			if (moduleConfiguration == null)
			{
				throw new ArgumentNullException(nameof(moduleConfiguration));
			}

			this.moduleEntry = moduleEntry;
			this.moduleConfiguration = moduleConfiguration;

			if (moduleConfiguration.ImplementationType == null)
			{
				this.comparableImplementationType = string.Empty;
			}
			else
			{
				this.comparableImplementationType = Regex.Replace(moduleConfiguration.ImplementationType, @"\s", string.Empty);
			}
		}

		public string ImplementationType => this.moduleConfiguration.ImplementationType;

		public List<IProvidedService> ProvidedServices => this.moduleConfiguration.ProvidedServices as List<IProvidedService>;

		public List<IDependingService> DependingServices => this.moduleConfiguration.DependingServices as List<IDependingService>;

		public List<IRequiredService> RequiredServices => this.moduleConfiguration.RequiredServices as List<IRequiredService>;

		public string ModuleName => this.moduleEntry.ModuleName;

		public string ConfigFileName => this.moduleEntry.ConfigFileName;

		public bool IsStartup => this.moduleEntry.IsStartup;

		public int StartupOrder => this.moduleEntry.StartupOrder;

		public List<IRequiredModule> RequiredModules => this.moduleEntry.RequiredModules as List<IRequiredModule>;

		public new string ToString()
		{
			if (this.ImplementationType != null)
			{
				return
					string.Format(
						CultureInfo.CurrentCulture,
						"{0} ({1})",
						this.ModuleName,
						this.ImplementationType.Split(',')[0].Split('.').LastOrDefault());
			}
			return this.ModuleName ?? string.Empty;
		}

		public new bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}

			ModuleDefinition other = obj as ModuleDefinition;
			return Equals(this.comparableImplementationType, other.comparableImplementationType);
		}

		public new int GetHashCode()
		{
			return this.comparableImplementationType.GetHashCode();
		}
	}
}