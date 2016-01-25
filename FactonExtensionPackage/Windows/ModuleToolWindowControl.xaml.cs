namespace FactonExtensionPackage.Windows
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Xml.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using FactonExtensionPackage.Modularity;
	using FactonExtensionPackage.Services;
	using FactonExtensionPackage.Tree;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	public partial class ModuleToolWindowControl : UserControl
	{
		private List<IModuleConfig> modules;

		private DTE dte;

		public ModuleToolWindowControl()
		{
			this.InitializeComponent();
			this.dte = (DTE)Package.GetGlobalService(typeof(SDTE));
		}

		private static TreeNode CreateTreeStructure(List<ModuleProxy> modules)
		{
			var root = CreateSimpleTreeNode("Facton");
			root.IsExpanded = true;

			var configFileNames = modules.Select(m => m.ConfigFileName).ToList();
			var query =
				configFileNames.Select(configName => FindParentNodeName(configName, configFileNames))
					.Distinct()
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.OrderBy(s => s.Replace(".config", ""))
					.ToList();

			var splits = query.Select(m => m.Split('.')).ToList();
			List<string> addedModules = new List<string>();

			var firstLayer = splits.Where(a => a.Count() >= 2).Select(a => a[1]).Distinct();
			foreach (var firstNodeName in firstLayer)
			{
				var firstNode = CreateSimpleTreeNode(firstNodeName);

				var secondLayer = splits.Where(a => a.Count() >= 3 && a[1] == firstNodeName).Select(a => a[2]).Distinct();
				foreach (var secondNodeName in secondLayer)
				{
					var secondNode = CreateSimpleTreeNode(secondNodeName);

					var thirdLayer = splits.Where(a => a.Count() >= 4 && a[1] == firstNodeName && a[2] == secondNodeName).Select(a => a[3]).Distinct();
					foreach (var thirdNodeName in thirdLayer)
					{
						var thirdNode = CreateSimpleTreeNode(thirdNodeName);

						var fourthLayer =
							splits.Where(a => a.Count() >= 5 && a[1] == firstNodeName && a[2] == secondNodeName && a[3] == thirdNodeName)
								.Select(a => a[4])
								.Distinct();
						foreach (var fourthNodeName in fourthLayer)
						{
							var fourthNode = CreateSimpleTreeNode(fourthNodeName);

							var fourthPath = "Facton." + firstNodeName + "." + secondNodeName + "." + thirdNodeName + "." + fourthNodeName;
							var nodesToAddToFourthNode =
								modules.Where(m => !addedModules.Contains(m.ConfigFileName) && m.ConfigFileName.StartsWith(fourthPath)).ToList();
							nodesToAddToFourthNode.ForEach(
								n =>
									{
										fourthNode.Children.Add(CreatTreeNode(n));
										addedModules.Add(n.ConfigFileName);
									});
							thirdNode.Children.Add(fourthNode);
						}

						var path = "Facton." + firstNodeName + "." + secondNodeName + "." + thirdNodeName;
						var nodesToAdd = modules.Where(m => !addedModules.Contains(m.ConfigFileName) && m.ConfigFileName.StartsWith(path)).ToList();
						nodesToAdd.ForEach(n => thirdNode.Children.Add(CreatTreeNode(n)));
						addedModules.AddRange(nodesToAdd.Select(m => m.ConfigFileName));
						secondNode.Children.Add(thirdNode);
					}

					var localPath = "Facton." + firstNodeName + "." + secondNodeName;
					var localNodesToAdd = modules.Where(m => m.ConfigFileName.StartsWith(localPath) && !addedModules.Contains(m.ConfigFileName)).ToList();
					localNodesToAdd.ForEach(n => secondNode.Children.Add(CreatTreeNode(n)));
					addedModules.AddRange(localNodesToAdd.Select(m => m.ConfigFileName));

					firstNode.Children.Add(secondNode);
				}

				var lastPath = "Facton." + firstNodeName;
				var lastNodes = modules.Where(m => m.ConfigFileName.StartsWith(lastPath) && !addedModules.Contains(m.ConfigFileName)).ToList();
				lastNodes.ForEach(
					n =>
						{
							addedModules.Add(n.ConfigFileName);
							firstNode.Children.Add(CreatTreeNode(n));
						});

				root.Children.Add(firstNode);
			}
			return root;
		}

		private static TreeNode CreatTreeNode(ModuleProxy module)
		{
			var item = new TreeNode(module.ConfigFileName);
			foreach (var m in module.Children)
			{
				item.Children.Add(CreateEmptyTreeNode(m.Value, false, true));
			}
			foreach (var m in module.Dependents)
			{
				item.Children.Add(CreateEmptyTreeNode(m, true, false));
			}
			return item;
		}

		private static TreeNode CreateEmptyTreeNode(ModuleProxy m, bool isDependent, bool isRequired)
		{
			TreeNode node = new TreeNode(m.ConfigFileName) { IsDependend = isDependent, IsRequired = isRequired };
			node.Children.Add(new TreeNode("Loading..."));
			return node;
		}

		private static TreeNode CreateEmptyTreeNode(string name, bool isDependent, bool isRequired)
		{
			TreeNode node = new TreeNode(name) { IsDependend = isDependent, IsRequired = isRequired };
			node.Children.Add(new TreeNode("Loading..."));
			return node;
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var auz = 1;
			this.modules = ModuleSystemService.LoadModules();
			TreeNode root = CreateTreeStructure2(this.modules);

			//List<ProjectItem> configFiles = this.dte.Solution.FindProjectItems(
			//	p => p.Name.Contains("Facton") && !p.IsTestProject(),
			//	p => p.Name.EndsWith(".config") && p.Name != "CodeAnalysisDictionary.xml" && p.Name != "App.config");

			//this.modules =
			//	configFiles.GroupBy(t => t.ModuleName())
			//		.ToDictionary(grp => grp.Key, grp => new ModuleProxy(grp.First()))
			//		.Select(m => m.Value)
			//		.OrderBy(m => m.ConfigFileName)
			//		.ToList();

			//foreach (var moduleProxy in this.modules)
			//{
			//	var localModule = this.GetOrAddModule(moduleProxy);

			//	foreach (var providedService in moduleProxy.ProvidedServices)
			//	{
			//		var service = this.GetOrAddService(providedService, localModule);
			//		service.SetParrentModule(localModule);
			//		localModule.AddProvidedService(service);
			//	}

			//	foreach (var requiredService in moduleProxy.RequiredServices)
			//	{
			//		var service = this.GetOrAddService(requiredService.Name, localModule);
			//		localModule.AddRequiredService(service, requiredService.Type);
			//	}

			//	foreach (var dependentService in moduleProxy.DependingServices)
			//	{
			//		var service = this.GetOrAddService(dependentService, localModule);
			//		localModule.AddDependentService(service);
			//	}
			//}

			//foreach (var localService in this.localServices)
			//{
			//	if (localService.Value.ProvidedModule == null)
			//	{
			//		MessageBox.Show(localService.Key);
			//	}
			//}

			//TreeNode root = CreateTreeStructure2(this.localModules);
			this.ModuleTree.Items.Clear();
			this.ModuleTree.Items.Add(root);

			//this.LoadTree();
		}

		private static TreeNode CreateTreeStructure2(List<IModuleConfig> modules)
		{
			var root = CreateSimpleTreeNode("Facton");
			root.IsExpanded = true;

			var allModuleNames = modules.Select(m => m.ModuleName).ToList();

			var query =
				allModuleNames.Select(configName => FindParentNodeName(configName, allModuleNames))
					.Distinct()
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.OrderBy(s => s.Replace(".config", ""))
					.ToList();

			var splits = query.Select(m => m.Split('.')).ToList();
			List<string> addedModules = new List<string>();

			var firstLayer = splits.Where(a => a.Count() >= 2).Select(a => a[1]).Distinct();
			foreach (var firstNodeName in firstLayer)
			{
				var firstNode = CreateSimpleTreeNode(firstNodeName);

				var secondLayer = splits.Where(a => a.Count() >= 3 && a[1] == firstNodeName).Select(a => a[2]).Distinct();
				foreach (var secondNodeName in secondLayer)
				{
					var secondNode = CreateSimpleTreeNode(secondNodeName);

					var thirdLayer = splits.Where(a => a.Count() >= 4 && a[1] == firstNodeName && a[2] == secondNodeName).Select(a => a[3]).Distinct();
					foreach (var thirdNodeName in thirdLayer)
					{
						var thirdNode = CreateSimpleTreeNode(thirdNodeName);

						var fourthLayer =
							splits.Where(a => a.Count() >= 5 && a[1] == firstNodeName && a[2] == secondNodeName && a[3] == thirdNodeName)
								.Select(a => a[4])
								.Distinct();
						foreach (var fourthNodeName in fourthLayer)
						{
							var fourthNode = CreateSimpleTreeNode(fourthNodeName);

							var fourthPath = "Facton." + firstNodeName + "." + secondNodeName + "." + thirdNodeName + "." + fourthNodeName;
							var nodesToAddToFourthNode =
								modules.Where(m => !addedModules.Contains(m.ModuleName) && m.ModuleName.StartsWith(fourthPath)).ToList();
							nodesToAddToFourthNode.ForEach(
								n =>
								{
									fourthNode.Children.Add(CreatTreeNode(n));
									addedModules.Add(n.ModuleName);
								});
							thirdNode.Children.Add(fourthNode);
						}

						var path = "Facton." + firstNodeName + "." + secondNodeName + "." + thirdNodeName;
						var nodesToAdd = modules.Where(m => !addedModules.Contains(m.ModuleName) && m.ModuleName.StartsWith(path)).ToList();
						nodesToAdd.ForEach(n => thirdNode.Children.Add(CreatTreeNode(n)));
						addedModules.AddRange(nodesToAdd.Select(m => m.ModuleName));
						secondNode.Children.Add(thirdNode);
					}

					var localPath = "Facton." + firstNodeName + "." + secondNodeName;
					var localNodesToAdd = modules.Where(m => m.ModuleName.StartsWith(localPath) && !addedModules.Contains(m.ModuleName)).ToList();
					localNodesToAdd.ForEach(n => secondNode.Children.Add(CreatTreeNode(n)));
					addedModules.AddRange(localNodesToAdd.Select(m => m.ModuleName));

					firstNode.Children.Add(secondNode);
				}

				var lastPath = "Facton." + firstNodeName;
				var lastNodes = modules.Where(m => m.ModuleName.StartsWith(lastPath) && !addedModules.Contains(m.ModuleName)).ToList();
				lastNodes.ForEach(
					n =>
					{
						addedModules.Add(n.ModuleName);
						firstNode.Children.Add(CreatTreeNode(n));
					});

				root.Children.Add(firstNode);
			}
			return root;
		}

		private static string FindParentNodeName(string name, IEnumerable<string> allNames)
		{
			name = name.Replace(".config", "");
			var splits = name.Split('.');

			if (splits.Count() == 3)
			{
				return name;
			}
			if (splits.Count() >= 5)
			{
				var possibleName = splits[0] + "." + splits[1] + "." + splits[2] + "." + splits[3] + "." + splits[4];
				if (allNames.Count(n => n.StartsWith(possibleName)) >= 3)
				{
					return possibleName;
				}
			}
			else if (splits.Count() >= 4)
			{
				var possibleName = splits[0] + "." + splits[1] + "." + splits[2] + "." + splits[3];
				if (allNames.Count(n => n.StartsWith(possibleName)) >= 3)
				{
					return possibleName;
				}
				return splits[0] + "." + splits[1] + "." + splits[2];
			}

			return string.Empty;
		}

		private static TreeNode CreatTreeNode(IModuleConfig module)
		{
			var item = new TreeNode(module.ModuleName);
			module.RequiredModulesNames.ForEach(c => item.Children.Add(CreateEmptyTreeNode(c, false, true)));
			module.DependingModulesNames.ForEach(c => item.Children.Add(CreateEmptyTreeNode(c, true, false)));
			module.ProvidedServices.Select(s => s.ServiceName).OrderBy(s => s).ToList().ForEach(c => item.Children.Add(CreateSimpleTreeNode(c)));			
			return item;
		}

		//private void LoadTree()
		//{
		//	this.ModuleTree.Items.Clear();
		//	this.dte = (DTE)Package.GetGlobalService(typeof(SDTE));

		//	foreach (var module in this.modules)
		//	{
		//		foreach (var possibleChild in this.modules.Where(m => m.ConfigFileName != module.ConfigFileName))
		//		{
		//			var requiredService = module.RequiredServices.FirstOrDefault(service => possibleChild.ProvidedServices.Contains(service.Name));
		//			if (requiredService != null)
		//			{
		//				possibleChild.Parents.Add(module);
		//				module.AddChild(possibleChild, requiredService);
		//			}
		//			if (module.DependingServices.Any(service => possibleChild.ProvidedServices.Contains(service)))
		//			{
		//				module.Dependents.Add(possibleChild);
		//			}
		//		}
		//	}

		//	TreeNode root = CreateTreeStructure(this.modules);
		//	this.ModuleTree.Items.Add(root);
		//}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (this.ModuleTree.SelectedItem != null)
			{
				var selectedItem = this.ModuleTree.SelectedItem;
				string name = string.Empty;
				if (selectedItem is TreeNode)
				{
					TreeNode treeNode = selectedItem as TreeNode;
					name = treeNode.Name;
				}
				var selectedModule = this.modules.FirstOrDefault(m => m.ModuleName == name);
				if (selectedModule != null)
				{
					var prpjectItem = this.dte.Solution.FindProjectItem(p => p.Name == selectedModule.ModuleName);
					prpjectItem?.OpenInEditor();
				}
			}
		}

		private void Expand_OnClick(object sender, RoutedEventArgs e)
		{
			//var selectedItem = this.ModuleTree.SelectedItem;
			//if (selectedItem is TreeNode)
			//{
			//	TreeNode treeNode = selectedItem as TreeNode;
			//	treeNode.IsExpanded = true;
			//}
		}

		private void ModuleTree_OnExpanded(object sender, RoutedEventArgs e)
		{
			var treeViewItem = e.OriginalSource as TreeViewItem;
			var treeNode = treeViewItem?.Header as TreeNode;
			if (treeNode?.Children.Count == 1 && treeNode.Children[0].Name == "Loading...")
			{
				treeNode.Children.Clear();
				var module = this.modules.FirstOrDefault(m => m.ModuleName == treeNode.Name.Replace(" - runtimeOnly", ""));
				if (module != null)
				{
					module.RequiredModulesNames.ForEach(c => treeNode.Children.Add(CreateEmptyTreeNode(c, false, true)));
					module.DependingModulesNames.ForEach(c => treeNode.Children.Add(CreateEmptyTreeNode(c, true, false)));
					module.ProvidedServices.Select(s => s.ServiceName).OrderBy(s => s).ToList().ForEach(c => treeNode.Children.Add(CreateSimpleTreeNode(c)));
				}
			}
		}

		private static TreeNode CreateSimpleTreeNode(string name)
		{
			return new TreeNode(name);
		}

		private void ModuleTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
		}

		private List<ModuleProxy> GetProjectItems(string text)
		{
			var projectItems = new List<ModuleProxy>();
			//var factonModules = XDocument.Parse(text).Descendants().FirstOrDefault(p => p.Name.LocalName == "factonModules");
			//var moduleEntryList = factonModules?.ToString().Deserialize<XmlModuleEntryList>();
			//if (moduleEntryList != null)
			//{
			//	foreach (IModuleEntry moduleEntry in moduleEntryList.ModuleEntries)
			//	{
			//		ModuleProxy projectItem = this.modules.FirstOrDefault(m => m.ModuleName == moduleEntry.ModuleName);
			//		if (projectItem == null)
			//		{
			//			throw new Exception($"{moduleEntry.ModuleName} not found");
			//		}
			//		projectItems.Add(projectItem);
			//	}

			//	foreach (var configFileName in moduleEntryList.ModuleRegions.Where(mr => mr.HasConfigFile).Select(mr => mr.ConfigFileName))
			//	{
			//		ModuleProxy xmlFile = this.modules.FirstOrDefault(m => m.ModuleName == configFileName);
			//		if (xmlFile == null)
			//		{
			//			throw new Exception($"{configFileName} not found");
			//		}
			//		projectItems.AddRange(this.GetProjectItems(xmlFile.Text));
			//	}
			//}
			return projectItems;
		}
	}
}