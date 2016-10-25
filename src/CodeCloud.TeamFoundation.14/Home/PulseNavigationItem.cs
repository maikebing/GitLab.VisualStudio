using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(PulseNavigationItemId, NavigationItemPriority.Pulse)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PulseNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PulseNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA2";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public PulseNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Pulse";
            this.IsVisible = true;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Execute()
        {
            MessageBox.Show("Pulse");
        }
    }
}
