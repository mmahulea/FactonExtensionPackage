namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;
	using EnvDTE80;

	public static class SolutionExtensions
	{
		public static Project FindProject(this Solution solution, Predicate<Project> condition)
		{
			var activeProject = solution?.DTE?.ActiveDocument?.ProjectItem?.ContainingProject;
			if (activeProject != null && condition(activeProject))
			{
				return activeProject;
			}

			var project = solution?.Projects?.Cast<Project>().FirstOrDefaultFromMany(
				proj =>
					{
						return (proj?.ProjectItems != null && proj.Kind == ProjectKinds.vsProjectKindSolutionFolder)
									? proj.ProjectItems.Cast<ProjectItem>().Where(p => p.SubProject != null).Select(p => p.SubProject)
									: Enumerable.Empty<Project>();
					},
				condition);
			return project;
		}

		public static ProjectItem FindProjectItem(this Solution solution, Predicate<Project> projectCondition, Predicate<ProjectItem> projectItemCondition)
		{
			ProjectItem projectItem = null;
			solution.FindProject(
					p =>
					{
						if (projectCondition(p))
						{
							projectItem = p.FindProjectItem(projectItemCondition);
							if (projectItem != null)
							{
								return true;
							}
						}
						return false;
					});
			return projectItem;
		}

		public static ProjectItem FindProjectItem(this Solution solution, Predicate<ProjectItem> condition)
		{
			ProjectItem projectItem = null;
			solution.FindProject(
					p =>
					{
						projectItem = p.FindProjectItem(condition);
						return projectItem != null;
					});
			return projectItem;
		}

		public static List<Project> FindProjects(this Solution solution, Predicate<Project> condition)
		{
			var list = new List<Project>();
			foreach (Project project in solution.Projects)
			{
				if (project != null)
				{
					if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
					{
						list.AddRange(GetSolutionFolderProjects(project, condition));
					}
					else if (condition(project))
					{
						list.Add(project);
					}
				}
			}
			return list;
		}

		public static List<ProjectItem> FindProjectItems(this Solution solution, Predicate<Project> projectCondition, Predicate<ProjectItem> projectItemCondition)
		{
			List<ProjectItem> projectItems = new List<ProjectItem>();

			foreach (Project project in solution.Projects)
			{
				if (project != null)
				{
					if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
					{
						projectItems.AddRange(FindProjectItemsInSolutionFolderProjects(project, projectCondition, projectItemCondition));
					}
					else if (projectCondition(project))
					{
						projectItems.AddRange(project.FindProjectItems(projectItemCondition));
					}
				}
			}

			return projectItems.Where(p => p != null).ToList();
		}

		private static IEnumerable<Project> GetSolutionFolderProjects(Project project, Predicate<Project> condition)
		{
			List<Project> list = new List<Project>();
			foreach (ProjectItem projectItem in project.ProjectItems)
			{
				if (projectItem.SubProject != null)
				{
					if (projectItem.SubProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
					{
						list.AddRange(GetSolutionFolderProjects(projectItem.SubProject, condition));
					}
					else if (condition(projectItem.SubProject))
					{
						list.Add(projectItem.SubProject);
					}
				}
			}
			return list;
		}

		private static IEnumerable<ProjectItem> FindProjectItemsInSolutionFolderProjects(Project project, Predicate<Project> projectCondition, Predicate<ProjectItem> projectItemCondition)
		{
			List<ProjectItem> list = new List<ProjectItem>();
			foreach (ProjectItem projectItem in project.ProjectItems)
			{
				if (projectItem.SubProject != null)
				{
					if (projectItem.SubProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
					{
						list.AddRange(FindProjectItemsInSolutionFolderProjects(projectItem.SubProject, projectCondition, projectItemCondition));
					}
					else if (projectCondition(projectItem.SubProject))
					{
						list.AddRange(projectItem.SubProject.FindProjectItems(projectItemCondition));
					}
				}
			}
			return list;
		}
	}
}