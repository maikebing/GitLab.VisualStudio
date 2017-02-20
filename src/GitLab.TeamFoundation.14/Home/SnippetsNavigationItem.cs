using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

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
            _tes = tes;

            Text = Strings.Items_Snippets;
        }

        public override void Invalidate()
        {
            base.Invalidate();

            IsVisible = IsVisible && _tes.Project != null && _tes.Project.IsSnippetsEnabled;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.BlueNavigationItem);
        }

        public override void Execute()
        {
            OpenInBrowser("snippets");
        }
    }
}
      