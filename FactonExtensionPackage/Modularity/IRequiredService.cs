namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public interface IRequiredService
	{
		RequirementType RequirementType { get; }

		string ServiceName { get; }

		Service Service { get; set; }
	}
}

public enum RequirementType
{
	Normal = 0,

	OnDemand = 1,

	RuntimeOnly = 2
}

