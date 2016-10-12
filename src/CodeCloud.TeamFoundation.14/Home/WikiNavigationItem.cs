using CodeCloud.TeamFoundation.Base;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(WikiNavigationItemId, NavigationItemPriority.Wiki)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WikiNavigationItem : TeamExplorerNavigationItem
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public WikiNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Wiki";
            ArgbColor = Colors.BlueNavigationItem.ToInt32();
            this.IsVisible = true;
        }

        public override void Execute()
        {
            MessageBox.Show("Wiki");
        }
    }
}
