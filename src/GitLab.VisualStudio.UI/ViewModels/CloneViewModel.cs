using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Helpers;
using GitLab.VisualStudio.Shared.Helpers.Commands;
using GitLab.VisualStudio.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace GitLab.VisualStudio.UI.ViewModels
{
    public class CloneViewModel : Bindable
    {
        private ObservableCollection<ProjectViewModel> _repositories;

        private readonly IDialog _dialog;
        private readonly IMessenger _messenger;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly IWebService _web;

        public CloneViewModel(IDialog dialog, IMessenger messenger, IShellService shell, IStorage storage, IWebService web)
        {
            _dialog = dialog;
            _messenger = messenger;
            _shell = shell;
            _storage = storage;
            _web = web;

            _repositories = new ObservableCollection<ProjectViewModel>();

            Repositories = CollectionViewSource.GetDefaultView(_repositories);
            Repositories.GroupDescriptions.Add(new PropertyGroupDescription("Owner"));

            _baseRepositoryPath = _storage.GetBaseRepositoryDirectory();

            LoadRepositoriesAsync();

            _cloneCommand = new DelegateCommand(OnClone, CanClone);
            _browseCommand = new DelegateCommand(OnBrowse);
        }

        public ICollectionView Repositories { get; }

        private string _baseRepositoryPath;
        public string BaseRepositoryPath
        {
            get { return _baseRepositoryPath; }
            set { SetProperty(ref _baseRepositoryPath, value); }
        }

        private DelegateCommand _browseCommand;
        public ICommand BrowseCommand
        {
            get { return _browseCommand; }
        }

        private DelegateCommand _cloneCommand;
        public ICommand CloneCommand
        {
            get { return _cloneCommand; }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private ProjectViewModel _selectedRepository;
        public ProjectViewModel SelectedRepository
        {
            get { return _selectedRepository; }
            set
            {
                SetProperty(ref _selectedRepository, value, () => _cloneCommand.InvalidateCanExecute());
            }
        }

        private bool CanClone()
        {
            if (SelectedRepository == null)
            {
                return false;
            }

            var potentialPath = System.IO.Path.Combine(BaseRepositoryPath, SelectedRepository.Name);
            return !System.IO.Directory.Exists(potentialPath);
        }

        private void OnBrowse()
        {
            var browsed = _shell.BrowseFolder();
            if (browsed != null)
            {
                BaseRepositoryPath = browsed;
            }
        }

        private void OnClone()
        {
            var path = System.IO.Path.Combine(BaseRepositoryPath, SelectedRepository.Name);

            var repository = new Repository
            {
                Name = SelectedRepository.Name,
                Path = path,
                Icon = SelectedRepository.Icon
            };
            _messenger.Send("OnClone", SelectedRepository.Url, repository);

            _dialog.Close();
        }

        private void LoadRepositoriesAsync()
        {
            string error = null;
            IEnumerable<Project> loaded = null;

            IsBusy = true;
            Task.Run(() =>
            {
                try
                {
                    loaded = LoadRepositories();
                }
                catch (Exception)
                {
                    error = Strings.CloneView_FailedToLoadProjects;
                }
            }).ContinueWith(task =>
            {
                IsBusy = false;
                _repositories.Clear();

                if (error == null)
                {
                    if (loaded == null)
                    {
                        Message = Strings.CloneView_NoProjects;
                    }
                    else
                    {
                        loaded.Each(o => _repositories.Add(new ProjectViewModel(o)));

                        var first = _repositories.Select(o => o.Owner).FirstOrDefault();
                        if (first != null)
                        {
                            first.IsExpanded = true;
                        }
                    }
                }
                else
                {
                    Message = error;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private IReadOnlyList<Project> LoadRepositories()
        {
            return _web.GetProjects();
        }
    }
}
