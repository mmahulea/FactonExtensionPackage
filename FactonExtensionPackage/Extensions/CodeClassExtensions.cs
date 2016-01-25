namespace FactonExtensionPackage.Extensions
{
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.FormatingCommands;

	public static class CodeClassExtensions
	{
		public static List<CodeInterface> GetImplementedInterfaces(this CodeClass codeClass)
		{
			List<CodeInterface> interfaces = new List<CodeInterface>();
			if (codeClass != null)
			{
				foreach (CodeInterface currentInterface in codeClass.ImplementedInterfaces)
				{
					interfaces.AddRange(GetImplementedInterfacesRecursivly(currentInterface));
				}
			}
			return interfaces;
		}		

		private static IEnumerable<CodeInterface> GetImplementedInterfacesRecursivly(CodeInterface currentInterface)
		{
			if (currentInterface != null)
			{
				var interfaces = new List<CodeInterface> { currentInterface };
				foreach (var baseElement in currentInterface.Bases)
				{
					if (baseElement is CodeInterface)
					{
						interfaces.AddRange(GetImplementedInterfacesRecursivly(baseElement as CodeInterface));
					}
				}
				return interfaces;
			}
			return Enumerable.Empty<CodeInterface>();
		}

		public static List<CodeClass> GetBaseAbstractClasses(this CodeClass codeClass)
		{
			if (codeClass != null)
			{
				var bases = new List<CodeClass>();
				foreach (var baseElement in codeClass.Bases)
				{
					if (baseElement is CodeClass)
					{
						var test = (baseElement as CodeClass2);
						if (test.IsAbstract)
						{
							bases.Add(test);
							bases.AddRange(GetBaseAbstractClasses(baseElement as CodeClass));
						}
					}
				}
				return bases;
			}
			return Enumerable.Empty<CodeClass>().ToList();
		}
	}
}