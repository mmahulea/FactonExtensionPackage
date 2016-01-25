namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Xml;

	public static class XmlDocumentExtensions
	{
		public static void IterateThroughAllNodes(this XmlDocument doc, Action<XmlNode> elementVisitor)
		{
			if (doc != null && elementVisitor != null)
			{
				foreach (XmlNode node in doc.ChildNodes)
				{
					DoIterateNode(node, elementVisitor);
				}
			}
		}

		private static void DoIterateNode(XmlNode node, Action<XmlNode> elementVisitor)
		{
			elementVisitor(node);

			foreach (XmlNode childNode in node.ChildNodes)
			{
				DoIterateNode(childNode, elementVisitor);
			}
		}
	}
}