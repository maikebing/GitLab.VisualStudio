using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Helpers;
using CodeCloud.VisualStudio.Shared.Helpers.Commands;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeCloud.TeamFoundation.ViewModels
{
    public class PublishSectionViewModel : Bindable, IDisposable
    {
        private readonly IMessenger _messenger;
        private readonly IGitService _git;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _tes;
        private readonly IViewFactory _viewFactory;
        private readonly IVisualStudioService _vs;
        private readonly IWebService _web;
        public PublishSectionViewModel(IMessenger messenger, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            messenger.Register("OnLogined", OnLogined);
            messenger.Register("OnSignOuted", OnSignOuted);

            _messenger = messenger;
            _git = git;
            _shell = shell;
            _storage = storage;
            _tes = tes;
            _viewFactory = viewFactory;
            _vs = vs;
            _web = web;

            Name = Strings.Common_Name;
            Provider = Strings.Common_Provider;
            Description = Strings.Common_Description;

            _loginCommand = new DelegateCommand(OnLogin);
            _signUpCommand = new DelegateCommand(OnSignUp);
            _getStartedCommand = new DelegateCommand(OnGetStarted);
            _publishCommand = new DelegateCommand(OnPublish, CanPublish);

            ShowGetStarted = true;
            //IsStarted = true;
            LoadResources();
        }

        private void LoadResources()
        {
            Licenses.Add(string.Empty, Strings.Common_ChooseALicense);
            SelectedLicense = string.Empty;
            foreach (var line in _git.GetLicenses())
            {
                Licenses.Add(line, line);
            }
        }

        public string Name { get; set; }
        public string Provider { get; set; }
        public string Description { get; set; }

        public IDictionary<string, string> Licenses { get; } = new Dictionary<string, string>();

        private string _selectedLicense;
        public string SelectedLicense
        {
            get { return _selectedLicense; }
            set { SetProperty(ref _selectedLicense, value); }
        }

        private bool _isStarted;
        public bool IsStarted
        {
            get { return _isStarted; }
            set { SetProperty(ref _isStarted, value); }
        }

        public bool ShowLogin
        {
            get { return !_storage.IsLogined; }
        }

        public bool ShowSignUp
        {
            get { return !_storage.IsLogined; }
        }

        private bool _showGetStarted;
        public bool ShowGetStarted
        {
            get { return _showGetStarted; }
            set { SetProperty(ref _showGetStarted, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private bool _isPrivate;
        public bool IsPrivate
        {
            get { return _isPrivate; }
            set { SetProperty(ref _isPrivate, value); }
        }

        private DelegateCommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand;
            }
        }

        private DelegateCommand _signUpCommand;
        public ICommand SignUpCommand
        {
            get
            {
                return _signUpCommand;
            }
        }

        private DelegateCommand _getStartedCommand;
        public ICommand GetStartedCommand
        {
            get
            {
                return _getStartedCommand;
            }
        }

        private DelegateCommand _publishCommand;
        public ICommand PublishCommand
        {
            get { return _publishCommand; }
        }

        private string _repositoryName;
        public string RepositoryName
        {
            get { return _repositoryName; }
            set
            {
                SetProperty(ref _repositoryName, value, () =>
                {
                    _publishCommand.InvalidateCanExecute();
                });
            }
        }

        private string _repositoryDescription;
        public string RepositoryDescription
        {
            get { return _repositoryDescription; }
            set { SetProperty(ref _repositoryDescription, value); }
        }

        public string LoginLabel
        {
            get { return Strings.Publish_Login; }
        }

        public string SignUpLabel
        {
            get { return Strings.Publish_SignUp; }
        }

        public string PublishLabel
        {
            get { return Strings.Publish_Publish; }
        }

        public string PrivateLabel
        {
            get { return Strings.Common_Private; }
        }

        private void OnLogin()
        {
            var dialog = _viewFactory.GetView<Dialog>(ViewTypes.Login);
            _shell.ShowDialog(string.Format(Strings.Login_ConnectTo, Strings.Common_Name), dialog);
        }

        public void OnLogined()
        {
            OnPropertyChanged(nameof(ShowLogin));
            OnPropertyChanged(nameof(ShowSignUp));
            OnPropertyChanged(nameof(ShowGetStarted));
        }

        public void OnSignOuted()
        {
            OnPropertyChanged(nameof(ShowLogin));
            OnPropertyChanged(nameof(ShowSignUp));


            ShowGetStarted = true;
        }

        private void OnSignUp()
        {
            _shell.OpenUrl(@"https://git.oschina.net/signup");
        }

        private void OnGetStarted()
        {
            if (!_storage.IsLogined)
            {
                var dialog = _viewFactory.GetView<Dialog>(ViewTypes.Login);
                _shell.ShowDialog(string.Format(Strings.Login_ConnectTo, Strings.Common_Name), dialog);
            }

            if (_storage.IsLogined)
            {
                ShowGetStarted = false;
                IsStarted = true;
            }
        }

        private void OnPublish()
        {
            CreateResult result = null;
            string error = null;

            IsBusy = true;

            Task.Run(() =>
            {
                try
                {
                    result = _web.CreateProject(RepositoryName, RepositoryDescription, IsPrivate);
                    if (result.Project != null)
                    {
                        var activeRepository = _tes.GetActiveRepository();

                        var path = activeRepository == null ? _tes.GetSolutionPath() : activeRepository.Path;

                        var user = _storage.GetUser();
                        var password = _storage.GetPassword();

                        _git.PushWithLicense(user.Name, user.Email, password, result.Project.Url, path, SelectedLicense);
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }).ContinueWith(task =>
            {
                IsBusy = false;
                if (error != null)
                {
                    _tes.ShowError(error);
                }
                else if (result.Message != null)
                {
                    _tes.ShowError(result.Message);
                }
                else
                {
                    IsStarted = false;
                    ShowGetStarted = true;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CanPublish()
        {
            return _repositoryName != null && _repositoryName.Trim().Length < 64;
        }

        public void Dispose()
        {
            _messenger.UnRegister(this);
            GC.SuppressFinalize(this);
        }
    }
}
