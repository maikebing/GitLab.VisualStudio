using GitLab.TeamFoundation.ViewModels;
using GitLab.TeamFoundation.Views;
using GitLab.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace GitLab.TeamFoundation.Sync
{
    [TeamExplorerSection(Settings.PublishSectionId, TeamExplorerPageIds.GitCommits, Settings.PublishSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitLabPublishSection : TeamExplorerSectionBase
    {

        private readonly IMessenger _messenger;
        private readonly IGitService _git;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _tes;
        private readonly IViewFactory _viewFactory;
        private readonly IVisualStudioService _vs;
        private readonly IWebService _web;

        [ImportingConstructor]
        public GitLabPublishSection(IMessenger messenger, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            _messenger = messenger;
            _git = git;
            _shell = shell;
            _storage = storage;
            _tes = tes;
            _viewFactory = viewFactory;
            _vs = vs;
            _web = web;
        }

        protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        {
            var temp = new TeamExplorerSectionViewModelBase
            {
                Title = string.Format(Strings.Publish_Title, Strings.Name)
            };

            return temp;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            IsVisible = !_tes.IsGitLabRepo();
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            return new PublishSectionView();
        }

        protected override void InitializeView(SectionInitializeEventArgs e)
        {
            var view = this.SectionContent as FrameworkElement;
            if (view != null)
            {
                var temp = new PublishSectionViewModel(_messenger, _git, _shell, _storage, _tes, _viewFactory, _vs, _web);
                temp.Published += OnPublished;

                view.DataContext = temp;
            }
        }

        private void OnPublished()
        {
            IsVisible = false;
        }

        public void ShowPublish()
        {
            IsVisible = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            var disposable = ViewModel as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}