namespace FactonExtensionPackage.FormatingCommands.SubCommands
{
	using System;
	using System.Linq;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using VSLangProj;

	public class FactonizeCurrentProjectCommand
	{
		public static void Execute(ProjectItem projectItem)
		{
			var project = projectItem.ContainingProject;
			if (!project.IsTestProject())
			{
				var proj = project.Object as VSProject;
				foreach (Reference reference in proj.References)
				{
					if (reference.Name.StartsWith("Facton.", StringComparison.CurrentCulture) && reference.CopyLocal)
					{
						reference.CopyLocal = false;
					}
				}

				if (project.ConfigurationManager != null)
				{
					ConfigurationManager manager = project.ConfigurationManager;
					foreach (string cfgName in (Array)manager.ConfigurationRowNames)
					{
						if (cfgName.ToLower() == "release")
						{
							Configuration configuration = manager.ConfigurationRow(cfgName).OfType<Configuration>().FirstOrDefault();
							if (configuration != null)
							{
								var documentationFileProperty = configuration.Properties.Item("DocumentationFile");
								var documentationFile = Convert.ToString(documentationFileProperty.Value);
								if (string.IsNullOrWhiteSpace(documentationFile))
								{
									documentationFileProperty.Value = project.Name + ".XML";
								}
							}
						}
					}
				}
			}
		}
	}
}
