using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(IssuesNavigationItemId, NavigationItemPriority.Issues)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssuesNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string IssuesNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA4";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public IssuesNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Issue";
            this.IsVisible = true;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Execute()
        {
            MessageBox.Show("Issue");
        }
    }
}
