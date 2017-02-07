using CodeCloud.TeamFoundation.Views;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerSection(Settings.HomeSectionId, TeamExplorerPageIds.Home, Settings.HomeSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudHomeSection : TeamExplorerSectionBase
    {
        private readonly IVisualStudioService _vs;

        [ImportingConstructor]
        public CodeCloudHomeSection(IVisualStudioService vs)
        {
            _vs = vs;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            _vs.ServiceProvider = ServiceProvider;

            IsVisible = _vs.IsCodeCloudProject;
        }

        protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        {
            var temp = new TeamExplorerSectionViewModelBase();
            temp.Title = Strings.Common_Name;

            return temp;
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            return new TextBlock
            {
                Text = Strings.Common_Description,
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
        }
    }
}