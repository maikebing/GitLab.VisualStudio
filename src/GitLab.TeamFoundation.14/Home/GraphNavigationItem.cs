using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.GraphsNavigationItemId, Settings.Graphs)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GraphNavigationItem : GitLabNavigationItem
    {
        private readonly ITeamExplorerServices _tes;

        [ImportingConstructor]
        public GraphNavigationItem(IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService ws)
           : base(Octicon.graph, git, shell, storage, tes, ws)
        {
            _tes = tes;
            Text = Strings.Items_Graph;
        }

        public override void Execute()
        {
            var repo = _tes.GetActiveRepository();
            OpenInBrowser($"graphs/{repo.Branch}");
        }
    }
}