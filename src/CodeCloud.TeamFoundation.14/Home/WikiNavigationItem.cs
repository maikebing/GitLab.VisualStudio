using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.WikiNavigationItemId, Settings.Wiki)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WikiNavigationItem : CodeCloudNavigationItem
    {
        [ImportingConstructor]
        public WikiNavigationItem(IGitService git, IShellService shell, IStorage storage, IVisualStudioService vs, IWebService ws)
           : base(Octicon.book, git, shell, storage, vs, ws)
        {
            Text = Strings.Items_Wiki;
        }

        public override void Invalidate()
        {
            base.Invalidate();

            IsVisible = IsVisible && Project.IsWikiEnabled;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.BlueNavigationItem);
        }

        public override void Execute()
        {
            OpenInBrowser("wikis");
        }
    }
}
