namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Serialization;
	using EnvDTE;
	using EnvDTE80;
	using FactonExtensionPackage.Modularity;

	public static class DteExtensions
	{
		public static T Deserialize<T>(this DTE dte)
		{
			var txt = dte.GetActiveDocumentText();
			return txt.Deserialize<T>();
		}

		public static T Deserialize<T>(this string txt)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(T));
				using (TextReader reader = new StringReader(txt))
				{
					return (T)serializer.Deserialize(reader);
				}
			}
			catch (Exception ex)
			{
				// ignored;				
			}
			return default(T);
		}

		public static string Serialize<T>(this T obj)
		{
			var xmlSerializer = new XmlSerializer(typeof(T));
			var ns = new XmlSerializerNamespaces();
			using (Utf8StringWriter textWriter = new Utf8StringWriter())
			{
				xmlSerializer.Serialize(textWriter, obj, ns);
				return textWriter.ToString();
			}
		}

		public static string GetActiveDocumentText(this DTE dte)
		{
			TextDocument objTextDoc = (TextDocument)dte.ActiveDocument.Object("TextDocument");
			EditPoint endPoint = objTextDoc.EndPoint.CreateEditPoint();
			EditPoint startPoint = objTextDoc.StartPoint.CreateEditPoint();
			string message = startPoint.CreateEditPoint().GetText(endPoint);
			return message;
		}

		public static string GetLineText(this DTE dte)
		{
			TextSelection txtSel = (TextSelection)dte.ActiveDocument.Selection;
			var startPoint = txtSel.ActivePoint.CreateEditPoint();
			startPoint.StartOfLine();
			var endPoint = txtSel.ActivePoint.CreateEditPoint();
			endPoint.EndOfLine();
			var text = startPoint.GetText(endPoint).Trim();
			return text;
		}

		public static string GetTextBetweenQuatationsFromSelectionOrCurrentLine(this DTE dte)
		{
			var regex = new Regex(@"([""'])(?:(?=(\\?))\2.)*?\1");
			string moduleName = null;

			TextSelection txtSel = (TextSelection)dte.ActiveDocument.Selection;
			Match match = regex.Match(txtSel.Text.Trim());

			if (!match.Success)
			{
				match = regex.Match(dte.GetLineText());
			}

			if (match.Success)
			{
				moduleName = (from object capture in match.Groups select capture.ToString().Replace(@"""", "")).FirstOrDefault();
			}

			return moduleName;
		}

		public static string GetConfigFileNameFromCurrentLine(this DTE dte)
		{
			string moduleName = null;

			var configRegex = new Regex(@"configFile=([""'])(?:(?=(\\?))\2.)*?\1");
			var regex = new Regex(@"([""'])(?:(?=(\\?))\2.)*?\1");

			var line = dte.GetLineText();

			Match match = configRegex.Match(line);
			if (!match.Success)
			{
				match = regex.Match(line);
			}

			if (match.Success)
			{
				moduleName = (from object capture in match.Groups select capture.ToString().Replace(@"""", "")).FirstOrDefault();
				moduleName = moduleName.Replace("configFile=", "");
				if (moduleName != null && !moduleName.EndsWith(".config") && !moduleName.EndsWith(".xml"))
				{
					moduleName += ".config";
				}
			}

			return moduleName;
		}

		public static T FindCodeElemet<T>(this DTE dte)
		{
			if (dte?.ActiveDocument?.ProjectItem?.FileCodeModel?.CodeElements != null)
			{
				return SearchSingleInCodeElements<T>(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);
			}
			return default(T);
		}

		public static XmlModuleEntryList GetModuleEntryList(this DTE dte)
		{
			XmlModuleEntryList moduleEntryList = null;

			var text = dte.GetActiveDocumentText();
			XElement factonModules = XDocument.Parse(text).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonModules");
			XElement factonBootstrapper = XDocument.Parse(text).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonBootstrapper");
			if (factonBootstrapper != null)
			{
				IEnumerable<XElement> entries = factonBootstrapper.Descendants().Where(p => p.Name.LocalName == "entry");
			}

			if (factonModules != null)
			{
				moduleEntryList = factonModules.ToString().Deserialize<XmlModuleEntryList>();
			}


			return moduleEntryList;
		}

		public static XmlModuleEntryList GetModuleEntryList(this DTE dte, ProjectItem projectItem)
		{
			XmlModuleEntryList moduleEntryList = null;

			XElement element = XDocument.Parse(projectItem.ReadAllText()).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonModules");
			if (element != null)
			{
				moduleEntryList = element.ToString().Deserialize<XmlModuleEntryList>();
				if (moduleEntryList != null)
				{
					moduleEntryList.Name = projectItem.Name;
				}
			}
			return moduleEntryList;
		}

		private static T SearchSingleInCodeElements<T>(CodeElements codeElements)
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

		public static List<Project> GetSelectedProjectInSolutionExplorer(this DTE2 dte)
		{
			object[] selectedItems = (object[])dte.ToolWindows.SolutionExplorer.SelectedItems;
			var projects = new List<Project>();
			foreach (UIHierarchyItem selectedUiHierarchyItem in selectedItems)
			{
				if (selectedUiHierarchyItem.Object is Solution)
				{
					projects.AddRange(dte.Solution.FindProjects(p => p.FullName.EndsWith(".csproj")));
				}
				else if (selectedUiHierarchyItem.Object is Project)
				{
					var project = (Project)selectedUiHierarchyItem.Object;
					if (project.FileName.EndsWith(".csproj"))
					{
						projects.Add(project);
					}
					else
					{
						var subProjects = project.FindProjects(p => p.FullName.EndsWith(".csproj"));
						projects.AddRange(subProjects);
					}
				}
			}
			return projects;
		}

		public static bool FormatDocument(this DTE dte)
		{
			try
			{
				dte.ActiveDocument?.Activate();
				dte.ExecuteCommand("Edit.FormatDocument");
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	public sealed class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding { get { return Encoding.UTF8; } }
	}
}