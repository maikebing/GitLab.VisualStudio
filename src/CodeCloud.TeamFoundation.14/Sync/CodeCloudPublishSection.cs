using CodeCloud.TeamFoundation.ViewModels;
using CodeCloud.TeamFoundation.Views;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeCloud.TeamFoundation.Sync
{
    [TeamExplorerSection(Settings.PublishSectionId, TeamExplorerPageIds.GitCommits, Settings.PublishSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudPublishSection : TeamExplorerSectionBase
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
        public CodeCloudPublishSection(IMessenger messenger, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
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
                Title = string.Format(Strings.Publish_Title, Strings.Common_Name)
            };

            return temp;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            IsVisible = _storage.IsLogined && !IsCodeCloudRepo();

            _vs.ServiceProvider = ServiceProvider;
        }

        private bool IsCodeCloudRepo()
        {
            var projects = _web.GetProjects();
            var repo = _vs.GetActiveRepository();
            if (repo == null)
            {
                return false;
            }

            var path = repo.Path;
            var url = _git.GetRemote(path);

            var visible = false;

            foreach (var project in projects)
            {
                if (string.Equals(project.Url, url, StringComparison.OrdinalIgnoreCase))
                {
                    visible = true;

                    break;
                }
            }

            return visible;
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
                view.DataContext = new PublishSectionViewModel(_messenger, _git, _shell, _storage, _tes, _viewFactory, _vs, _web);
            }
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