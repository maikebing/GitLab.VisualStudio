using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.IssuesNavigationItemId, Settings.Issues)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssuesNavigationItem : CodeCloudNavigationItem
    {
        private readonly IVisualStudioService _vs;

        [ImportingConstructor]
        public IssuesNavigationItem(IGitService git, IShellService shell, IStorage storage, IVisualStudioService vs, IWebService ws)
           : base(Octicon.issue_opened, git, shell, storage, vs, ws)
        {
            _vs = vs;
            Text = Strings.Items_Issues;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Invalidate()
        {
            base.Invalidate();

            IsVisible = IsVisible && _vs.Current.IsIssueEnabled;
        }

        public override void Execute()
        {
            OpenInBrowser("issues");
        }
    }
}
