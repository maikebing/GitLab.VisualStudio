using GitLab.TeamFoundation.ViewModels;
using GitLab.TeamFoundation.Views;
using GitLab.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace GitLab.TeamFoundation.Connect
{
    [TeamExplorerSection(Settings.ConnectSectionId, TeamExplorerPageIds.Connect, Settings.ConnectSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitLabConnectSection : TeamExplorerSectionBase
    {
        private readonly IMessenger _messenger;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _teamexplorer;
        private readonly IViewFactory _viewFactory;
        private readonly IWebService _web;

        [ImportingConstructor]
        public GitLabConnectSection(IMessenger messenger, IShellService shell, IStorage storage, ITeamExplorerServices teamexplorer, IViewFactory viewFactory,  IWebService web)
        {
            _messenger = messenger;
            _shell = shell;
            _storage = storage;
            _teamexplorer = teamexplorer;
            _viewFactory = viewFactory;
            _web = web;

            messenger.Register("OnLogined", OnLogined);
            messenger.Register("OnSignOuted", OnSignOuted);
            messenger.Register<string, Repository>("OnClone", OnClone);
            messenger.Register<string>("OnOpenSolution", OnOpenSolution);
        }

        protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        {
            var temp = new TeamExplorerSectionViewModelBase
            {
                Title = Strings.Name
            };

            return temp;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            IsVisible = _storage.IsLogined;
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            return new ConnectSectionView();
        }
        protected override void InitializeView(SectionInitializeEventArgs e)
        {
            var view = this.SectionContent as FrameworkElement;
            if (view != null)
            {
                view.DataContext = new ConnectSectionViewModel(_messenger, _shell, _storage, _teamexplorer, _viewFactory, _web);
            }
        }

        public void OnLogined()
        {
            IsVisible = true;
        }

        public void OnSignOuted()
        {
            IsVisible = false;
        }

        public void OnClone(string url, Repository repository)
        {
            var gitExt = ServiceProvider.GetService<IGitRepositoriesExt>();
            gitExt.Clone(url, repository.Path, CloneOptions.RecurseSubmodule);
        }

        public void OnOpenSolution(string path)
        {
            var x = ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (x != null)
            {
                x.OpenSolutionViaDlg(path, 1);
            }
        }

        public override void Dispose()
        {
            _messenger.UnRegister(this);

            var disposable = ViewModel as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
