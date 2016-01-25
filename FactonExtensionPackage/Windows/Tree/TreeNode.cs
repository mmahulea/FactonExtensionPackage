namespace FactonExtensionPackage.Tree
{
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
	using FactonExtensionPackage.Annotations;

	public class TreeNode : INotifyPropertyChanged
	{
		private ModuleProxy moduleProxy;

		private bool isExpanded;

		public TreeNode()
		{
			this.Children = new ObservableCollection<TreeNode>();
		}

		public TreeNode(ModuleProxy moduleProxy)
			: this()
		{
			this.moduleProxy = moduleProxy;
			this.Name = moduleProxy.ConfigFileName;
		}

		public TreeNode(string name)
			: this()
		{
			this.Name = name;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string Name { get; set; }

		public ObservableCollection<TreeNode> Children { get; set; }

		public bool IsCheckBoxVisible
		{
			get
			{
				return false;
				return this.Name.EndsWith(".config");
			}
		}

		public bool IsRequired { get; set; }

		public bool IsDependend { get; set; }

		public bool IsExpanded
		{
			get
			{
				return this.isExpanded;
			}
			set
			{
				this.isExpanded = value;
				this.OnPropertyChanged(nameof(this.IsExpanded));
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}