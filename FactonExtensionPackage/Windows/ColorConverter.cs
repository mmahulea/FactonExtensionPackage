namespace FactonExtensionPackage.Windows
{
	using System;
	using System.Globalization;
	using System.Windows.Data;
	using System.Windows.Media;
	using FactonExtensionPackage.Tree;

	public class ColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var treeNode = value as TreeNode;
			if (treeNode != null)
			{
				if (treeNode.IsDependend)
				{
					return new SolidColorBrush(Colors.CornflowerBlue);
				}
				if (treeNode.IsRequired)
				{
					return new SolidColorBrush(Colors.Red);
				}
			}
			return Colors.Transparent;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}