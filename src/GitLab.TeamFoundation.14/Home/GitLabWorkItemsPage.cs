using GitLab.TeamFoundation.ViewModels;
using GitLab.TeamFoundation.Views;
using GitLab.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerPage(Settings.GitLabWorkItemsPageId)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitLabWorkItemsPage : TeamExplorerPageBase
    {
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _tes;
        private readonly IWebService _web;

        [ImportingConstructor]
        public GitLabWorkItemsPage(IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService web)
        {
            _shell = shell;
            _storage = storage;
            _tes = tes;
            _web = web;
        }

        protected override ITeamExplorerPage CreateViewModel(PageInitializeEventArgs e)
        {
            return new TeamExplorerPageViewModelBase
            {
                Title = GetTitle(GetMode(e))
            };
        }

        protected override object CreateView(PageInitializeEventArgs e)
        {
            return new GitLabWorkItemsView(GetMode(e) == GitLabWorkItemsPageMode.MergeRequests);
        }

        protected override void InitializeView(PageInitializeEventArgs e)
        {
            var view = PageContent as FrameworkElement;
            if (view != null)
            {
                view.DataContext = new GitLabWorkItemsViewModel(_storage, _web, _shell, _tes, GetMode(e));
            }
        }

        public override void Refresh()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var view = PageContent as FrameworkElement;
            var vm = view != null ? view.DataContext as GitLabWorkItemsViewModel : null;
            if (vm != null)
            {
                vm.Refresh();
            }

            base.Refresh();
        }

        private static GitLabWorkItemsPageMode GetMode(PageInitializeEventArgs e)
        {
            if (e != null && e.Context is GitLabWorkItemsPageMode)
            {
                return (GitLabWorkItemsPageMode)e.Context;
            }

            return GitLabWorkItemsPageMode.Issues;
        }

        private static string GetTitle(GitLabWorkItemsPageMode mode)
        {
            return mode == GitLabWorkItemsPageMode.MergeRequests
                ? Strings.Items_MergeRequests
                : Strings.Items_Issues;
        }
    }
}
