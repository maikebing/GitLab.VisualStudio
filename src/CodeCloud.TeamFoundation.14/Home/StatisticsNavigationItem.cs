using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.StatisticsNavigationItemId, Settings.Statistics)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StatisticsNavigationItem : CodeCloudNavigationItem
    {
        private readonly IVisualStudioService _vs;

        [ImportingConstructor]
        public StatisticsNavigationItem(IGitService git, IShellService shell, IStorage storage, IVisualStudioService vs, IWebService ws)
           : base(Octicon.graph, git, shell, storage, vs, ws)
        {
            _vs = vs;
            Text = Strings.Items_Statistics;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Execute()
        {
            OpenInBrowser($"repository/stats/{_vs.Branch}");
        }
    }
}
