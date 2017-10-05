using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.WikiNavigationItemId, Settings.Wiki)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WikiNavigationItem : GitLabNavigationItem
    {
        private readonly ITeamExplorerServices _tes;

        [ImportingConstructor]
        public WikiNavigationItem(IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService ws)
           : base(Octicon.book, git, shell, storage, tes, ws)
        {
            _tes = tes;

            Text = Strings.Items_Wiki;
        }

        public override void Invalidate()
        {
            base.Invalidate();
            IsVisible = IsVisible && _tes.Project != null && _tes.Project.WikiEnabled;
        }

        public override void Execute()
        {
            OpenInBrowser("wikis");
        }
    }
}
      