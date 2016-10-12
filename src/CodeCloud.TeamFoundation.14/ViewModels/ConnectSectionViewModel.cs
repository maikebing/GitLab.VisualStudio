using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Helpers;
using CodeCloud.VisualStudio.Shared.Helpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CodeCloud.TeamFoundation.ViewModels
{
    public class ConnectSectionViewModel : Bindable
    {
        public ObservableCollection<Repository> Repositories { get; }

        private readonly IMessenger _messenger;
        private readonly IRegistry _registry;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _teamexplorer;
        private readonly IViewFactory _viewFactory;
        private readonly IVisualStudioService _vs;
        private readonly IWebService _web;

        public ConnectSectionViewModel(IMessenger messenger, IRegistry registry, IShellService shell, IStorage storage, ITeamExplorerServices teamexplorer, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            messenger.Register("OnLogined", OnLogined);

            _messenger = messenger;
            _registry = registry;
            _shell = shell;
            _storage = storage;
            _teamexplorer = teamexplorer;
            _viewFactory = viewFactory;
            _vs = vs;
            _web = web;

            Repositories = new ObservableCollection<Repository>();
            Repositories.CollectionChanged += OnRepositoriesChanged;

            _signOutCommand = new DelegateCommand(OnSignOut);
            _cloneCommand = new DelegateCommand(OnClone);
            _createCommand = new DelegateCommand(OnCreate);
            _openRepositoryCommand = new DelegateCommand<Repository>(OnOpenRepository);

            LoadRepositoriesAsync();
        }

        public void OnLogined()
        {
            LoadRepositoriesAsync();
        }

        private void OnRepositoriesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsRepositoriesVisible));
        }

        private Repository _selectedRepository;
        public Repository SelectedRepository
        {
            get { return _selectedRepository; }
            set { SetProperty(ref _selectedRepository, value); }
        }

        private DelegateCommand _signOutCommand;
        public ICommand SignOutCommand
        {
            get { return _signOutCommand; }
        }

        private DelegateCommand _cloneCommand;
        public ICommand CloneCommand
        {
            get { return _cloneCommand; }
        }

        private DelegateCommand _createCommand;
        public ICommand CreateCommand
        {
            get { return _createCommand; }
        }

        private DelegateCommand<Repository> _openRepositoryCommand;
        public ICommand OpenRepositoryCommand
        {
            get { return _openRepositoryCommand; }
        }

        public bool IsRepositoriesVisible
        {
            get { return Repositories.Count > 0; }
        }

        private void OnSignOut()
        {
            if (MessageBox.Show("确定要退出吗？", "退出码云", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _storage.Erase();
                _messenger.Send("OnSignOuted");
            }
        }

        private void OnClone()
        {
            var dialog = _viewFactory.GetView<Dialog>(ViewTypes.Clone);
            _shell.ShowDialog("克隆", dialog);
        }

        private void OnCreate()
        {
            var dialog = _viewFactory.GetView<Dialog>(ViewTypes.Create);
            _shell.ShowDialog("创建仓库", dialog);
        }

        private void OnOpenRepository(Repository repo)
        {
            if (repo == null)
            {
                return;
            }

            var path = _vs.GetActiveRepository();
            if (path == null || repo.Path != path)
            {
                _messenger.Send("OnOpenSolution", repo.Path);
            }
        }

        private void LoadRepositoriesAsync()
        {
            IReadOnlyList<Repository> known = null;
            IReadOnlyList<Project> remotes = null;

            string error = null;
            Task.Run(() =>
            {
                try
                {
                    remotes = _web.GetProjects();
                    known = _registry.GetKnownRepositories();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }).ContinueWith(task =>
            {
                if (error == null)
                {
                    Repositories.Clear();

                    var actived = _vs.GetActiveRepository();

                    var valid = new List<Repository>();

                    foreach (var k in known)
                    {
                        var r = remotes.FirstOrDefault(o => o.Name == k.Name);
                        if (r != null)
                        {
                            k.Icon = r.Icon;

                            valid.Add(k);
                        }
                    }

                    if (actived != null)
                    {
                        var matched = valid.FirstOrDefault(o => o.Path == actived);
                        if (matched != null)
                        {
                            matched.IsActived = true;
                        }
                    }

                    valid.Each(o => Repositories.Add(o));
                }
                else
                {
                    _teamexplorer.ShowMessage(error);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
