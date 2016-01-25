namespace FactonExtensionPackage.Extensions
{
	using EnvDTE;

	public static class EditPointExtensions
	{
		public static string GetLineText(this EditPoint point)
		{
			point.StartOfLine();
			var start = point.CreateEditPoint();

			point.EndOfLine();
			var end = point.CreateEditPoint();

			return start.GetText(end);
		}

		public static void DeletePreviousLine(this EditPoint start)
		{
			start.LineUp();
			start.StartOfLine();
			var point = start.CreateEditPoint();
			start.LineDown();
			start.StartOfLine();
			point.Delete(start);
		}

		public static void DeleteCurrentLine(this EditPoint start)
		{
			start.StartOfLine();
			var point = start.CreateEditPoint();
			start.LineDown();
			start.StartOfLine();
			point.Delete(start);
		}

		public static string GetPreviousLineText(this EditPoint point)
		{
			point.LineUp();
			point.StartOfLine();
			var start = point.CreateEditPoint();

			point.EndOfLine();
			var end = point.CreateEditPoint();

			string text = start.GetText(end);
			point.LineDown();

			return text;
		}

		public static string GetNextLineText(this EditPoint point)
		{
			point.LineDown();
			point.StartOfLine();
			var start = point.CreateEditPoint();

			point.EndOfLine();
			var end = point.CreateEditPoint();

			string text = start.GetText(end);
			point.LineUp();

			return text;
		}
	}
}