using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.IssuesNavigationItemId, Settings.Issues)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssuesNavigationItem : GitLabNavigationItem
    {
        private readonly ITeamExplorerServices _tes;

        [ImportingConstructor]
        public IssuesNavigationItem(IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService ws)
           : base(Octicon.issue_opened, git, shell, storage, tes, ws)
        {
            _tes = tes;
            Text = Strings.Items_Issues;
           
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Invalidate()
        {
            base.Invalidate();
            IsVisible = IsVisible && _tes.Project != null && _tes.Project.IsIssueEnabled;
        }

        public override void Execute()
        {
            OpenInBrowser("issues");
        }
    }
}
