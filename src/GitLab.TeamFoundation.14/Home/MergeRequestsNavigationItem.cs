using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.MergeRequestsNavigationItemId, Settings.MergeRequests)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MergeRequestsNavigationItem : GitLabNavigationItem
    {
        private readonly ITeamExplorerServices _tes;

        [ImportingConstructor]
        public MergeRequestsNavigationItem(IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService ws)
           : base(Octicon.git_pull_request, git, shell, storage, tes, ws)
        {
            _tes = tes;
            Text = Strings.Items_PullRequests;
        }

        public override void Invalidate()
        {
            base.Invalidate();

            IsVisible = IsVisible && _tes.Project != null && _tes.Project.IsPullRequestsEnabled;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.RedNavigationItem);
        }

        public override void Execute()
        {
            OpenInBrowser("merge_requests");
        }
    }
}
