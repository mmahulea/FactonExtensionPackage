namespace FactonExtensionPackage.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EnvDTE;

	public static class ProjectExtensions
	{
		public static ProjectItem FindProjectItem(this Project project, Predicate<ProjectItem> condition)
		{
			var projectItem = project?.ProjectItems?.Cast<ProjectItem>().FirstOrDefaultFromMany(
				p => p?.ProjectItems?.Cast<ProjectItem>() ?? Enumerable.Empty<ProjectItem>(),
				condition);
			return projectItem;
		}

		public static IEnumerable<ProjectItem> FindProjectItems(this Project project, Predicate<ProjectItem> condition)
		{
			List<ProjectItem> projectItems = new List<ProjectItem>();
			if (project != null && project.ProjectItems != null)
			{
				foreach (ProjectItem projectItem in project.ProjectItems)
				{
					if (condition(projectItem))
					{
						projectItems.Add(projectItem);
					}
					if (projectItem.HasValidSubProjectItems())
					{
						var childProjectItems = FindProjectItems(projectItem, condition);
						if (childProjectItems.Any())
						{
							projectItems.AddRange(childProjectItems);
						}
					}
				}
			}
			return projectItems.Where(p => p != null);
		}

		public static IEnumerable<Project> FindProjects(this Project parentProject, Predicate<Project> condition)
		{
			List<Project> projects = new List<Project>();
			if (parentProject != null && parentProject.ProjectItems != null)
			{
				foreach (ProjectItem projectItem in parentProject.ProjectItems)
				{
					var project = projectItem.Object as Project;
					if (project != null)
					{
						if (condition(project))
						{
							projects.Add(project);
						}

						if (projectItem.HasValidSubProjectItems())
						{
							var childProjects = FindProjects(project, condition);
							if (childProjects.Any())
							{
								projects.AddRange(childProjects);
							}
						}
					}
				}
			}
			return projects.Where(p => p != null);
		}

		private static IEnumerable<ProjectItem> FindProjectItems(ProjectItem mainProjectItem, Predicate<ProjectItem> condition)
		{
			List<ProjectItem> projectItems = new List<ProjectItem>();

			if (mainProjectItem != null && mainProjectItem.ProjectItems != null)
			{
				foreach (ProjectItem projectItem in mainProjectItem.ProjectItems)
				{
					if (condition(projectItem))
					{
						projectItems.Add(projectItem);
					}

					if (projectItem.HasValidSubProjectItems())
					{
						projectItems.AddRange(FindProjectItems(projectItem, condition));
					}
				}
			}

			return projectItems;
		}

		public static bool IsTestProject(this Project project)
		{
			return project.Name.Contains(".Test") || project.Name.Contains(".TestTool") || project.Name.Contains(".IntegrationTesting.");
		}
	}
}