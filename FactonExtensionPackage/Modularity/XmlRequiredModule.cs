namespace FactonExtensionPackage.Modularity
{
	using System.Xml.Serialization;

	public class XmlRequiredModule : IRequiredModule
	{
		/// <summary>
		/// Gets or sets the name of the module.
		/// </summary>
		[XmlAttribute("name")]
		public string ModuleName { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return this.ModuleName ?? base.ToString();
		}
	}
}