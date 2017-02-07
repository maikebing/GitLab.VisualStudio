using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.PullRequestsNavigationItemId, Settings.PullRequests)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : CodeCloudNavigationItem
    {
        private readonly IVisualStudioService _vs;

        [ImportingConstructor]
        public PullRequestsNavigationItem(IGitService git, IShellService shell, IStorage storage, IVisualStudioService vs, IWebService ws)
           : base(Octicon.git_pull_request, git, shell, storage, vs, ws)
        {
            _vs = vs;
            Text = Strings.Items_PullRequests;
        }

        public override void Invalidate()
        {
            base.Invalidate();

            IsVisible = IsVisible && _vs.Current.IsPullRequestsEnabled;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.RedNavigationItem);
        }

        public override void Execute()
        {
            OpenInBrowser("pulls");
        }
    }
}
