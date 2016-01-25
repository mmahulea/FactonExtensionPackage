namespace FactonExtensionPackage.Services
{
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class SearchService
	{
		public static List<CodeElement> FindUsings(DTE dte)
		{
			var nameSpace = FindNameSpace(dte);
			List<CodeElement> outsideNameSpace = FindUsings(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
			List<CodeElement> insideNamespace = FindUsings(nameSpace.Members);

			outsideNameSpace.AddRange(insideNamespace);
			return outsideNameSpace;
		}

		public static List<CodeElement> FindUsings(CodeElements codeElements)
		{
			List<CodeElement> usings = new List<CodeElement>();
			foreach (CodeElement codeElement in codeElements)
			{
				if (codeElement.Kind == vsCMElement.vsCMElementImportStmt)
				{
					usings.Add(codeElement);
				}
			}
			return usings;
		}

		public static List<CodeVariable> FindVariables(DTE dte)
		{
			return SearchInCodeElements<CodeVariable>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static CodeNamespace FindNameSpace(DTE dte)
		{
			return SearchSingleInCodeElements<CodeNamespace>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static CodeNamespace FindNameSpace(ProjectItem projectItem)
		{
			return SearchSingleInCodeElements<CodeNamespace>(projectItem.FileCodeModel.CodeElements);
		}

		public static CodeClass FindClass(DTE dte)
		{
			if (dte.ActiveDocument.ProjectItem.FileCodeModel == null)
				return null;
			return SearchSingleInCodeElements<CodeClass>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static CodeClass FindClass(ProjectItem projectItem)
		{
			return projectItem?.FileCodeModel == null ? null : SearchSingleInCodeElements<CodeClass>(projectItem.FileCodeModel.CodeElements);
		}

		public static List<CodeClass> FindClasses(DTE dte)
		{
			return SearchInCodeElements<CodeClass>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static CodeInterface FindInterface(DTE dte)
		{
			return SearchSingleInCodeElements<CodeInterface>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static List<CodeFunction> FindMethods(DTE dte)
		{
			return SearchInCodeElements<CodeFunction>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static List<CodeElement> FindCodeElements(DTE dte)
		{
			if (dte.ActiveDocument.ProjectItem.FileCodeModel == null) return new List<CodeElement>();
			return SearchInCodeElements<CodeElement>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static List<CodeElement> FindCodeElements(ProjectItem projectItem)
		{
			return SearchInCodeElements<CodeElement>(projectItem.FileCodeModel.CodeElements);
		}

		public static List<CodeFunction> FindContructors(DTE dte)
		{
			if (dte.ActiveDocument.ProjectItem.FileCodeModel == null)
			{
				return new List<CodeFunction>();
			}
			return SearchInCodeElements<CodeFunction>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements)
				.Where(f => f.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
				.ToList();
		}

		public static List<CodeFunction> FindContructors(ProjectItem projectItem)
		{
			return SearchInCodeElements<CodeFunction>(projectItem.FileCodeModel.CodeElements)
				.Where(f => f.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
				.ToList();
		}

		public static List<CodeProperty> FindProperties(DTE dte)
		{
			return SearchInCodeElements<CodeProperty>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
		}

		public static T SearchSingleInCodeElements<T>(CodeElements codeElements)
		{
			foreach (CodeElement codeElement in codeElements)
			{
				T codeClass = SearchSingleInCodeElement<T>(codeElement);
				if (codeClass != null)
				{
					return codeClass;
				}
			}
			return default(T);
		}

		public static List<T> SearchInCodeElements<T>(CodeElements codeElements)
		{
			var list = new List<T>();
			foreach (CodeElement codeElement in codeElements)
			{
				List<T> resultList = SearchInCodeElement<T>(codeElement);
				if (resultList != null && resultList.Any())
				{
					list.AddRange(resultList);
				}
			}
			return list;
		}

		public static List<T> SearchInCodeElements<T>(IEnumerable<CodeElement> codeElements)
		{
			var list = new List<T>();
			foreach (CodeElement codeElement in codeElements)
			{
				List<T> resultList = SearchInCodeElement<T>(codeElement);
				if (resultList != null && resultList.Any())
				{
					list.AddRange(resultList);
				}
			}
			return list;
		}

		public static ProjectItem FindModuleFromConfig(ProjectItem configProjectItem)
		{
			var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
			var configFile = configProjectItem.ReadAllText();
			var moduleConfig = configFile.Deserialize<XmlModuleConfig>();
			var project = dte.Solution.FindProject(p => p.Name == moduleConfig.AssemblyName);
			return project.FindProjectItem(p => p.Name == moduleConfig.CsFileName);
		}

		public static ProjectItem FindConfigFromModule(ProjectItem moduleProjectItem)
		{
			CodeClass codeClass = FindClass(moduleProjectItem);
			if (codeClass != null)
			{
				var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
				string type = $"type=\"{codeClass.FullName}, {moduleProjectItem.ContainingProject.Name}\"";
				return dte.Solution.FindProjectItem(p => p.Name.EndsWith(".config") && p.ReadAllText().Contains(type));				
			}
			return null;
		}

		private static T SearchSingleInCodeElement<T>(CodeElement codeElement)
		{
			var objCodeNamespace = codeElement as CodeNamespace;
			var objCodeType = codeElement as CodeType;

			if (codeElement is T)
			{
				return (T)codeElement;
			}

			if (objCodeNamespace != null)
			{
				return SearchSingleInCodeElements<T>(objCodeNamespace.Members);
			}

			if (objCodeType != null)
			{
				return SearchSingleInCodeElements<T>(objCodeType.Members);
			}

			return default(T);
		}

		private static List<T> SearchInCodeElement<T>(CodeElement codeElement)
		{
			List<T> list = new List<T>();
			var objCodeNamespace = codeElement as CodeNamespace;
			var objCodeType = codeElement as CodeType;

			if (codeElement is T)
			{
				list.Add((T)codeElement);
			}

			if (objCodeNamespace != null)
			{
				list.AddRange(SearchInCodeElements<T>(objCodeNamespace.Members));
			}

			if (objCodeType != null)
			{
				list.AddRange(SearchInCodeElements<T>(objCodeType.Members));
			}

			return list;
		}
	}
}
