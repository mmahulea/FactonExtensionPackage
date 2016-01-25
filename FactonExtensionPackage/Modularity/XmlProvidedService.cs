namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public class XmlProvidedService : IProvidedService
	{
		[XmlAttribute("name")]
		public string ServiceName { get; set; }

		[XmlIgnore]
		public Service Service { get; set; }

		public override string ToString()
		{
			return this.ServiceName ?? base.ToString();
		}
	}
}