using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.SnippetsNavigationItemId, Settings.Snippets)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SnippetsNavigationItem : GitLabNavigationItem
    {
        private readonly ITeamExplorerServices _tes;

        [ImportingConstructor]
        public SnippetsNavigationItem(IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService ws)
           : base(Octicon.book, git, shell, storage, tes, ws)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _tes = tes;
            Text = Strings.Items_Snippets;
        }

        public override void Invalidate()
        {
            IsVisible = IsVisible && _tes.Project != null && _tes.Project.SnippetsEnabled;
        }

        public override void Execute()
        {
            if (_tes.Project != null && _tes.Project.SnippetsEnabled)
            {
                OpenInBrowser("snippets");
            }
            else
            {
                OpenHostUrlInBrowser("explore/snippets");
            }
        }
    }
}