using GitLab.TeamFoundation.ViewModels;
using GitLab.TeamFoundation.Views;
using GitLab.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace GitLab.TeamFoundation.Home
{
    [TeamExplorerSection(Settings.GitLabSettingsSectionId, TeamExplorerPageIds.Settings, Settings.GitLabSettingsSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitLabSettingsSection : TeamExplorerSectionBase
    {
        private readonly IStorage _storage;

        [ImportingConstructor]
        public GitLabSettingsSection(IStorage storage)
        {
            _storage = storage;
        }

        protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        {
            return new TeamExplorerSectionViewModelBase
            {
                Title = Strings.Name
            };
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);
            IsVisible = _storage.IsLogined;
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            return new GitLabSettingsView();
        }

        protected override void InitializeView(SectionInitializeEventArgs e)
        {
            var view = SectionContent as FrameworkElement;
            if (view != null)
            {
                view.DataContext = new GitLabSettingsViewModel(_storage);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
