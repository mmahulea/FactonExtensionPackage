namespace FactonExtensionPackage.Modularity
{
	public interface IDependingService
	{
		string ServiceName { get; }

		Service Service { get; set; }
	}
}