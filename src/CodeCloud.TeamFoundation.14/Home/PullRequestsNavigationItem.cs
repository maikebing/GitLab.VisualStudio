using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId, NavigationItemPriority.PullRequests)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        [ImportingConstructor]
        public PullRequestsNavigationItem()
        {
            Text = "Pull";
            IsVisible = true;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.RedNavigationItem);
        }

        public override void Execute()
        {
            MessageBox.Show("Pull");
        }
    }
}
