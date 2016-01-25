namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Services;

	public static class FormatUsingStatements
	{
		public static void Execute(ProjectItem projectItem)
		{
			var txtSel = (TextSelection)projectItem.Document.ActiveWindow.Selection;
			CodeNamespace nameSpace = SearchService.FindNameSpace(projectItem);

			MoveUsingsInsideNameSpace(projectItem, txtSel, nameSpace);

			var usings = SearchService.FindUsings(nameSpace.Members);
			var systemUsings = new List<string>();

			if (!AreUsingStatementsOrdered(usings))
			{
				try
				{
					projectItem.DTE.ExecuteCommand("ProjectandSolutionContextMenus.Project.PowerCommands.RemoveandSortUsings");
				}
				catch (Exception)
				{
					try
					{
						projectItem.DTE.ExecuteCommand("Edit.RemoveAndSort");
					}
					catch
					{
						try
						{
							projectItem.DTE.ExecuteCommand("Edit.SortUsings");
						}
						catch { }
					}
				}



				foreach (var systemUsing in usings)
				{
					string txt = systemUsing.InnerText();
					if (IsSystemUsingStatement(txt))
					{
						systemUsings.Add(txt);
						systemUsing.Delete();
					}
				}

				if (systemUsings.Any())
				{
					var editPoint = nameSpace.GetStartPoint().CreateEditPoint();
					editPoint.FindPattern("{", (int)vsFindOptions.vsFindOptionsNone, ref editPoint);
					txtSel.MoveToPoint(editPoint);
					foreach (var line in systemUsings)
					{
						txtSel.NewLine();
						txtSel.Insert(line);
					}
				}
			}
		}

		private static bool AreUsingStatementsOrdered(List<CodeElement> codeElements)
		{
			bool foundOtherUsing = false;
			foreach (var codeElement in codeElements)
			{
				string txt = codeElement.InnerText();
				if (!IsSystemUsingStatement(txt))
				{
					foundOtherUsing = true;
				}
				else
				{
					if (foundOtherUsing)
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool IsSystemUsingStatement(string txt)
		{
			return txt.StartsWith("using System.") || txt.StartsWith("using System;");
		}

		private static void MoveUsingsInsideNameSpace(ProjectItem projectItem, TextSelection txtSel, CodeNamespace nameSpace)
		{
			var usingsOutsideNameSpace = SearchService.FindUsings(projectItem.FileCodeModel.CodeElements);
			if (usingsOutsideNameSpace.Any())
			{
				List<string> linesToMove = new List<string>();
				for (int i = 0; i < usingsOutsideNameSpace.Count(); i++)
				{
					linesToMove.Add(usingsOutsideNameSpace[i].InnerText());
					usingsOutsideNameSpace[i].Delete();
				}

				var editPoint = nameSpace.GetStartPoint().CreateEditPoint();
				editPoint.FindPattern("{", (int)vsFindOptions.vsFindOptionsNone, ref editPoint);
				txtSel.MoveToPoint(editPoint);
				txtSel.NewLine();
				foreach (var line in linesToMove)
				{
					txtSel.Insert(line);
					txtSel.NewLine();
				}
			}
		}
	}
}
