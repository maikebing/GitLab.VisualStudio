using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(WikiNavigationItemId, NavigationItemPriority.Wiki)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public WikiNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Wiki";
            this.IsVisible = true;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.BlueNavigationItem);
        }

        public override void Execute()
        {
            MessageBox.Show("Wiki");
        }
    }
}
