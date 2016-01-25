namespace FactonExtensionPackage.Modularity
{
	public interface IProvidedService
	{
		string ServiceName { get; }

		Service Service { get; set; }
	}
}