namespace FactonExtensionPackage.TeamExplorer
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Microsoft.TeamFoundation.Controls;
	using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
	using Microsoft.TeamFoundation.VersionControl.Client;

	public class PendingChangesInclusion
	{
		private readonly Action<IList<PendingChange>> includeChanges;
		private readonly Action<IList<PendingChange>> excludeChanges;
		private readonly Func<IList<PendingChange>> getIncludedChanges;
		private readonly Func<IList<PendingChange>> getExcludedChanges;

		public IList<PendingChange> IncludedChanges => this.getIncludedChanges();

		public IList<PendingChange> ExcludedChanges => this.getExcludedChanges();

		public PendingChangesInclusion(ITeamExplorer teamExplorer)
		{
			var pendingChangesPage = (TeamExplorerPageBase)teamExplorer.NavigateToPage(new Guid(TeamExplorerPageIds.PendingChanges), null);

			var model = pendingChangesPage.Model;
			var p = model.GetType().GetProperty("DataProvider", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

			var dataProvider = p.GetValue(model); // IPendingChangesDataProvider is internal;
			var dataProviderType = dataProvider.GetType();

			p = dataProviderType.GetProperty("IncludedChanges", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var m = p.GetMethod;
			this.getIncludedChanges = (Func<IList<PendingChange>>)m.CreateDelegate(typeof(Func<IList<PendingChange>>), dataProvider);

			p = dataProviderType.GetProperty("ExcludedChanges", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			m = p.GetMethod;
			this.getExcludedChanges = (Func<IList<PendingChange>>)m.CreateDelegate(typeof(Func<IList<PendingChange>>), dataProvider);

			m = dataProviderType.GetMethod("IncludeChanges", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			this.includeChanges = (Action<IList<PendingChange>>)m.CreateDelegate(typeof(Action<IList<PendingChange>>), dataProvider);

			m = dataProviderType.GetMethod("ExcludeChanges", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			this.excludeChanges = (Action<IList<PendingChange>>)m.CreateDelegate(typeof(Action<IList<PendingChange>>), dataProvider);
		}

		public void IncludeChanges(IList<PendingChange> changes)
		{
			this.includeChanges(changes);
		}

		public void ExcludeChanges(IList<PendingChange> changes)
		{
			this.excludeChanges(changes);
		}
	}
}
