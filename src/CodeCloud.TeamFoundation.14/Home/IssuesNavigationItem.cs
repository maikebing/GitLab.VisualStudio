using CodeCloud.TeamFoundation;
using CodeCloud.TeamFoundation.Base;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(IssuesNavigationItemId, NavigationItemPriority.Issues)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssuesNavigationItem : TeamExplorerNavigationItem
    {
        public const string IssuesNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA4";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public IssuesNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Issue";
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
            this.IsVisible = true;
        }

        public override void Execute()
        {
            MessageBox.Show("Issue");
        }
    }
}
