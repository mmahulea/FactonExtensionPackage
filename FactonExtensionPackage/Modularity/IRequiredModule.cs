namespace FactonExtensionPackage.Modularity
{
	public interface IRequiredModule
	{
		/// <summary>
		/// Gets the name of the module that is required.
		/// </summary>
		string ModuleName { get; }
	}
}