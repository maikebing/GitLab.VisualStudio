using CodeCloud.TeamFoundation.Base;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId, NavigationItemPriority.PullRequests)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : TeamExplorerNavigationItem
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        [ImportingConstructor]
        public PullRequestsNavigationItem()
        {
            Text = "Pull";
            ArgbColor = Colors.RedNavigationItem.ToInt32();
            this.IsVisible = true;
        }

        public override void Execute()
        {
            MessageBox.Show("Pull");
        }
    }
}
