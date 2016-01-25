namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;

	public static class AddCopyrightCommand
	{
		private static readonly string Text =
"// <copyright file=\"{0}\" company=\"Facton GmbH\">" + Environment.NewLine +
"// All rights are reserved. Reproduction or transmission in whole or in part, in" + Environment.NewLine +
"// any form or by any means, electronic, mechanical or otherwise, is prohibited" + Environment.NewLine +
"// without the prior written consent of the copyright owner." + Environment.NewLine +
"// </copyright>" + Environment.NewLine;

		private static readonly string RegExText =
"// <copyright file=\"(?<file>.+?)\" company=\"Facton GmbH\">" + Environment.NewLine +
"// All rights are reserved. Reproduction or transmission in whole or in part, in" + Environment.NewLine +
"// any form or by any means, electronic, mechanical or otherwise, is prohibited" + Environment.NewLine +
"// without the prior written consent of the copyright owner." + Environment.NewLine +
"// </copyright>";

		public static void Execute(ProjectItem projectItem)
		{
			string textToInsert = string.Format(Text, projectItem.Name);			

			var objTextDoc = (TextDocument)projectItem.Document.Object("TextDocument");
			EditPoint editPoint = objTextDoc.StartPoint.CreateEditPoint();
			while (string.IsNullOrWhiteSpace(editPoint.GetLineText()))
			{
				editPoint.DeleteCurrentLine();				
			}
		
			if (projectItem.ReadAllText().TrimStart().StartsWith(textToInsert, StringComparison.InvariantCulture))
			{
				return;
			}

			var txtSel = (TextSelection)projectItem.Document.Selection;
			txtSel.MoveTo(0, 0);

			TextRanges ranges = null;
			int findOptions = (int)(vsFindOptions.vsFindOptionsRegularExpression | vsFindOptions.vsFindOptionsFromStart);
			if (txtSel.FindPattern(RegExText, findOptions, ref ranges))
			{
				var firstRange = ranges.OfType<TextRange>().FirstOrDefault();
				if (firstRange != null)
				{
					var rangeText = firstRange.StartPoint.GetText(firstRange.EndPoint);
					if (rangeText.Trim().Equals(textToInsert.Trim(), StringComparison.InvariantCulture))
					{
						return;
					}
					firstRange.StartPoint.Delete(firstRange.EndPoint);
				}
			}

			txtSel.GotoLine(1, true);
			txtSel.StartOfLine();
			txtSel.NewLine();
			txtSel.GotoLine(1, true);
			txtSel.Insert(textToInsert);
		}
	}
}
