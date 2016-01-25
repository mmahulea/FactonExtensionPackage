namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using EnvDTE;
	using FactonExtensionPackage.Services;
	using TextSelection = EnvDTE.TextSelection;

	public static class ProjectItemExtensions
	{
		private static Guid VsPhysicalFolder = Guid.Parse("6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C");
		private static Guid VsVirtualFolder = Guid.Parse("6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C");
		private static Guid VsSubproject = Guid.Parse("6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C");

		public static string ReadAllText(this ProjectItem projectItem)
		{
			var fullPath = Convert.ToString(projectItem.Properties.Item("FullPath").Value);
			if (File.Exists(fullPath))
			{
				return File.ReadAllText(Convert.ToString(projectItem.Properties.Item("FullPath").Value));
			}
			return string.Empty;
		}

		public static void ClearAllText(this ProjectItem projectItem)
		{
			var objTextDoc = (TextDocument)projectItem.Document.Object("TextDocument");
			var startPoint = objTextDoc.StartPoint.CreateEditPoint();
			var endPoint = objTextDoc.EndPoint.CreateEditPoint();

			var editPoint = startPoint.CreateEditPoint();
			editPoint.Delete(endPoint);
		}

		public static void SetText(this ProjectItem projectItem, string text)
		{
			var objTextDoc = (TextDocument)projectItem.Document.Object("TextDocument");
			var startPoint = objTextDoc.StartPoint.CreateEditPoint();
			startPoint.Insert(text);
		}

		public static T Deserialize<T>(this ProjectItem projectItem)
		{
			return projectItem.ReadAllText().Deserialize<T>();
		}

		public static void OpenInEditor(this ProjectItem projectItem)
		{
			var dte = projectItem.DTE;
			var fullPath = projectItem.Properties.Item("FullPath").Value.ToString();
			dte.ItemOperations.OpenFile(fullPath, Constants.vsViewKindTextView);
			dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
		}

		public static string ModuleName(this ProjectItem projectItem)
		{
			string moduleName;
			if (!projectItem.ContainingProject.IsTestProject())
			{
				moduleName = projectItem.Name;
			}
			else
			{
				//var path = projectItem.ContainingProject.ProjectItems.FindProjectItemPath(p => p.Name == projectItem.Name, null);
				var path = FindPath(projectItem, String.Empty);
				moduleName = path + projectItem.Name;
			}
			if (moduleName != null && moduleName.EndsWith(".config"))
			{
				moduleName = moduleName.Replace(".config", "");
			}
			return moduleName;
		}

		public static bool TryOpen(this ProjectItem projectItem)
		{
			if (!projectItem.IsOpen && (projectItem.Name.EndsWith(".cs") || projectItem.Name.EndsWith(".config")))
			{
				projectItem.Open();
				if (!projectItem.Name.EndsWith(".config"))
				{
					projectItem.Document?.Activate();
				}
				return true;
			}
			return false;
		}

		public static void TryClose(this ProjectItem projectItem, bool opened)
		{
			if (opened && !projectItem.IsDirty)
			{
				projectItem.Document?.Close();
			}
		}

		private static string FindPath(ProjectItem projectItem, string path)
		{
			var parent = projectItem.Collection.Parent as ProjectItem;
			if (parent != null)
			{
				if (parent.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}")
				{
					return FindPath(parent, parent.Name + "." + path);
				}
			}
			return path;
		}

		public static bool Contains(this ProjectItem projectItem, string text)
		{
			var objTextDoc = (TextDocument)projectItem.Document.Object("TextDocument");
			var endPoint = objTextDoc.EndPoint.CreateEditPoint();
			var startPoint = objTextDoc.StartPoint.CreateEditPoint();
			var body = startPoint.CreateEditPoint().GetText(endPoint);
			return body.ToLower().Contains(text.ToLower());
		}

		public static CodeNamespace FindNameSpace(this ProjectItem projectItem)
		{
			return SearchService.FindNameSpace(projectItem);
		}

		public static IEnumerable<CodeFunction> FindContructors(this ProjectItem projectItem)
		{
			return SearchService.FindContructors(projectItem);
		}

		public static void AddLine(this ProjectItem projectItem, EditPoint editPoint, string text)
		{
			var txtSel = (TextSelection)projectItem.Document.Selection;
			txtSel.MoveToPoint(editPoint);
			txtSel.EndOfLine();
			txtSel.NewLine();
			txtSel.Insert(text);
		}

		public static bool HasValidSubProjectItems(this ProjectItem projectItem)
		{
			var kind = Guid.Parse(projectItem.Kind);
			return (projectItem.ProjectItems != null) && (kind == VsVirtualFolder || kind == VsSubproject || kind == VsPhysicalFolder);
		}

		public static bool IsModuleConfig(this ProjectItem projectItem)
		{
			return projectItem.Name.EndsWith(".config", StringComparison.CurrentCulture) && projectItem.Contains("<moduleConfiguration");
		}

		public static bool IsModuleClass(this ProjectItem projectItem)
		{
			return projectItem.Name.EndsWith(".cs", StringComparison.CurrentCulture) && projectItem.Contains(": IModule");
		}
	}
}