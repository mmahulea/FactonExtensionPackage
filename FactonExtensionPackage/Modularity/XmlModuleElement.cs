namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public class XmlModuleElement
	{
		/// <summary>
		/// Gets or sets the name of the element.
		/// </summary>
		[XmlAttribute("name")]
		public string Name { get; set; }
	}
}