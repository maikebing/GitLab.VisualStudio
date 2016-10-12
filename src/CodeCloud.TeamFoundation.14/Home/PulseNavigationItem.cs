using CodeCloud.TeamFoundation.Base;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(PulseNavigationItemId, NavigationItemPriority.Pulse)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PulseNavigationItem : TeamExplorerNavigationItem
    {
        public const string PulseNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA2";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public PulseNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Pulse";
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
            this.IsVisible = true;
        }

        public override void Execute()
        {
            MessageBox.Show("Pulse");
        }
    }
}
