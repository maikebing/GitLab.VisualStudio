using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.AttachmentsNavigationItemId, Settings.Attachments)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PipelinesNavigationItem : GitLabNavigationItem
    {
        [ImportingConstructor]
        public PipelinesNavigationItem(IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService ws)
           : base(Octicon.zap, git, shell, storage, tes, ws)
        {
            Text = Strings.Items_Pipeline;
        
        }

        public override void Execute()
        {
            OpenInBrowser("pipelines");
        }
    }
}
