using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(GraphsNavigationItemId, NavigationItemPriority.Graphs)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GraphsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        readonly Lazy<IShellService> browser;

        [ImportingConstructor]
        public GraphsNavigationItem(Lazy<IShellService> browser)
        {
            this.browser = browser;
            Text = "Graph";
            this.IsVisible = true;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Execute()
        {
            MessageBox.Show("Graph");
        }
    }
}
