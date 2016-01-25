namespace FactonExtensionPackage.Windows
{
	using System;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;
	using FactonExtensionPackage.Tree;

	public class ColorToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var treeNode = value as TreeNode;
			if (treeNode != null && (treeNode.IsDependend || treeNode.IsRequired))
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}