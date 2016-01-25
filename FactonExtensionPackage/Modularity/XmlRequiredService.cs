namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public class XmlRequiredService : IRequiredService
	{
		[XmlAttribute("name")]
		public string ServiceName { get; set; }

		[XmlAttribute("requirementType")]
		public XmlRequirementType RequirementType { get; set; }

		RequirementType IRequiredService.RequirementType
		{
			get
			{
				return (RequirementType)this.RequirementType;
			}
		}

		public override string ToString()
		{
			return (this.ServiceName ?? base.ToString()) + " (" + this.RequirementType + ")";
		}

		[XmlIgnore]
		public Service Service { get; set; }
	}

	public enum XmlRequirementType
	{
		[XmlEnum(Name = "normal")]
		Normal = 0,

		[XmlEnum(Name = "onDemand")]
		OnDemand = 1,

		[XmlEnum(Name = "runtimeOnly")]
		RuntimeOnly = 2
	}
}