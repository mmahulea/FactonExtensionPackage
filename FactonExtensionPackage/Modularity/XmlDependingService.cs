namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public class XmlDependingService : IDependingService
	{
		[XmlAttribute("name")]
		public string ServiceName { get; set; }

		public override string ToString()
		{
			return this.ServiceName ?? base.ToString();
		}

		[XmlIgnore]
		public Service Service { get; set; }
	}
}