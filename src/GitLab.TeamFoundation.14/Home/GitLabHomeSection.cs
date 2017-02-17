using GitLab.TeamFoundation.Views;
using GitLab.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerSection(Settings.HomeSectionId, TeamExplorerPageIds.Home, Settings.HomeSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitLabHomeSection : TeamExplorerSectionBase
    {
        private readonly ITeamExplorerServices _tes;

        [ImportingConstructor]
        public GitLabHomeSection(ITeamExplorerServices tes)
        {
            _tes = tes;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            IsVisible = _tes.IsGitLabRepo();
        }

        protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        {
            var temp = new TeamExplorerSectionViewModelBase();
            temp.Title = Strings.Name;

            return temp;
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            return new TextBlock
            {
                Text = Strings.Description,
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
        }
    }
}