using CodeCloud.TeamFoundation;
using CodeCloud.TeamFoundation.Base;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(GraphsNavigationItemId, NavigationItemPriority.Graphs)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GraphsNavigationItem : TeamExplorerNavigationItem
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public GraphsNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Graph";
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
            this.IsVisible = true;
        }

        public override void Execute()
        {
            MessageBox.Show("Graph");
        }
    }
}
