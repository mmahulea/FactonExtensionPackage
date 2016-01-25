namespace FactonExtensionPackage.Extensions
{
	public static class ObjectExtensions
	{
		public static T As<T>(this object val)
		{
			return (T)val;
		}

	}
}