namespace FactonExtensionPackage
{
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.InteropServices;
	using EnvDTE;
	using FactonExtensionPackage.Commands;
	using FactonExtensionPackage.Windows;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>	
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
	[ProvideToolWindow(typeof(ModuleToolWindow))]
	public sealed class FactonizeCommandPackage : Package, IVsSolutionLoadEvents, IVsSolutionEvents
	{
		public const string PackageGuidString = "5ef15350-76b6-4c9d-a1da-30c19ebadb66";

		private DTE dte;

		private IVsSolution solution;

		private uint pdwCookie = uint.MaxValue;

		public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeCloseSolution(object pUnkReserved)
		{
			GlobalVariables.ProjectsLoaded = false;
			return VSConstants.S_OK;
		}

		public int OnAfterCloseSolution(object pUnkReserved)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeOpenSolution(string pszSolutionFilename)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeBackgroundSolutionLoadBegins()
		{
			return VSConstants.S_OK;
		}

		public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
		{
			pfShouldDelayLoadToNextIdle = false;
			return VSConstants.S_OK;
		}

		public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterBackgroundSolutionLoadComplete()
		{
			GlobalVariables.ProjectsLoaded = true;
			return VSConstants.S_OK;
		}

		protected override void Initialize()
		{
			base.Initialize();
			FactonizeCommand.Initialize(this);
			GoToModuleCommand.Initialize(this);
			GoToConfigCommand.Initialize(this);
			GoToProvidedConfigCommand.Initialize(this);
			this.dte = (DTE)this.GetService(typeof(DTE));
			this.AdviseSolutionEvents();
			ModuleToolWindowCommand.Initialize(this);
			FactonizeCtorsCommand.Initialize(this);
			AddInheritDocCommand.Initialize(this);
			SolutionExplorerCommands.InheritDocInSolutionExplorerCommand.Initialize(this);
			SolutionExplorerCommands.FactonizeInSolutionExplorerCommand.Initialize(this);
			SolutionExplorerCommands.FactonizeCtorsInSolutionExplorerCommand.Initialize(this);
			SolutionExplorerCommands.DeleteBinFolderCommand.Initialize(this);
			SolutionExplorerCommands.BuildFacton7DebugCommand.Initialize(this);
			SolutionExplorerCommands.GetBuildFacton7DebugCommand.Initialize(this);
			SolutionExplorerCommands.SolutionFolderDeleteBinFoldersCommand.Initialize(this);
		    SolutionExplorerCommands.FactonizeEverythingInProjectCommand.Initialize(this);
		    SolutionExplorerCommands.FactonizeEverythingInSolutionFolderCommand.Initialize(this);
		    SolutionExplorerCommands.FactonizePendingChangesInProjectCommand.Initialize(this);
		    SolutionExplorerCommands.FactonizePendingChangesInFolderSolutionCommand.Initialize(this);
		}

		protected override void Dispose(bool disposing)
		{
			this.UnadviseSolutionEvents();

			base.Dispose(disposing);
		}

		private void AdviseSolutionEvents()
		{
			this.UnadviseSolutionEvents();
			this.solution = this.GetService(typeof(SVsSolution)) as IVsSolution;

			if (this.solution != null)
			{
				this.solution.AdviseSolutionEvents(this, out this.pdwCookie);
			}
		}

		private void UnadviseSolutionEvents()
		{
			if (this.solution != null)
			{
				if (this.pdwCookie != uint.MaxValue)
				{
					this.solution.UnadviseSolutionEvents(this.pdwCookie);
					this.pdwCookie = uint.MaxValue;
				}
				this.solution = null;
			}
		}
	}
}
