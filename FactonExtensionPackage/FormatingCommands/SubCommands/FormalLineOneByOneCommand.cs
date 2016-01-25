namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;

	public static class FormalLineOneByOneCommand
	{
		public static void Execute(ProjectItem projectItem)
		{
			var objTextDoc = (TextDocument)projectItem.Document.Object("TextDocument");

			EditPoint editPoint = objTextDoc.StartPoint.CreateEditPoint();
			editPoint.LineDown();

			bool doEncountered = false;
			while (editPoint.LessThan(objTextDoc.EndPoint))
			{
				editPoint.StartOfLine();
				var start = editPoint.CreateEditPoint();

				string previousLine = start.GetPreviousLineText();
				string line = start.GetLineText();
				string nextLine = start.GetNextLineText();

				if (line.Trim() == "do")
				{
					doEncountered = true;
				}

				if (line.Trim().StartsWith("namespace ") && previousLine.EndsWith("</copyright>"))
				{
					start.StartOfLine();
					start.Insert(Environment.NewLine);
					continue;
				}

				if (line.Trim().StartsWith("using ") 
					&& !line.Trim().StartsWith("using (")
					&& !nextLine.Trim().StartsWith("using ") 
					&& !string.IsNullOrWhiteSpace(nextLine))
				{
					start.EndOfLine();
					start.Insert(Environment.NewLine);
					continue;
				}

				//if ((line.Trim() == "return" || line.TrimStart().StartsWith("return "))
				//	&& !string.IsNullOrWhiteSpace(previousLine)
				//	&& previousLine.Trim() != "{"
				//	&& !previousLine.Trim().StartsWith("case ")
				//	&& !previousLine.Trim().StartsWith("default:")
				//	&& !previousLine.Trim().StartsWith(@"// "))
				//{
				//	start.StartOfLine();
				//	start.Insert(Environment.NewLine);
				//	continue;
				//}

				if (line.Trim() == "}" 
					&& !nextLine.Trim().StartsWith("else")
					&& nextLine.Trim() != "}" && nextLine.Trim() != "};" && nextLine.Trim() != "});" && nextLine.Trim() != "},"
					&& nextLine.Trim() != "finally" && nextLine.Trim() != "#endif"
					&& !nextLine.Trim().StartsWith("catch (") && !nextLine.Trim().StartsWith("#endregion") && nextLine.Trim() != ("catch")
					&& !(nextLine.Trim().StartsWith("while (") && doEncountered)
					&& !string.IsNullOrWhiteSpace(nextLine))
				{
					start.EndOfLine();
					start.Insert(Environment.NewLine);
					continue;
				}

				if ((string.IsNullOrWhiteSpace(line) && string.IsNullOrWhiteSpace(previousLine))
					|| (string.IsNullOrWhiteSpace(line) && previousLine.Trim() == "{")
					|| (string.IsNullOrWhiteSpace(line) && previousLine.TrimStart().StartsWith("using ") && nextLine.TrimStart().StartsWith("using ")))
				{
					start.DeleteCurrentLine();
					continue;
				}

				if ((line.Trim() == "}" || line.Trim() == "{") && string.IsNullOrWhiteSpace(previousLine))
				{
					start.DeletePreviousLine();
					continue;
				}

				editPoint.LineDown();
			}
		}


	}
}
