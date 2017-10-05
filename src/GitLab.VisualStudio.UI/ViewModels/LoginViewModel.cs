using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Helpers;
using GitLab.VisualStudio.Shared.Helpers.Commands;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitLab.VisualStudio.UI.ViewModels
{
    public interface IPasswordMediator
    {
        string Password { get; set; }
    }

    public class LoginViewModel : Validatable
    {
        private IPasswordMediator _mediator;

        private readonly IDialog _dialog;
        private readonly IMessenger _messenger;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly IWebService _web;

        public LoginViewModel(IDialog dialog, IPasswordMediator mediator, IMessenger messenger, IShellService shell, IStorage storage, IWebService web)
        {
            _dialog = dialog;
            _messenger = messenger;
            _shell = shell;
            _storage = storage;
            _web = web;

            _mediator = mediator;

            _loginCommand = new DelegateCommand(OnLogin);
            _forgetPasswordCommand = new DelegateCommand(OnForgetPassword);
            _activeAccountCommand = new DelegateCommand(OnActiveAccount);
            _signupCommand = new DelegateCommand(OnSignup);
        }

        private string _host;
        [Required(ErrorMessageResourceType = typeof(Strings),AllowEmptyStrings =false, ErrorMessageResourceName = "Login_HostIsRequired")]
        public string Host
        {
            get {
                if (string.IsNullOrEmpty(_host) || string.IsNullOrWhiteSpace(_host))
                {
                    _host = Strings.DefaultHost;
                }
                return _host;
            }
            set { SetProperty(ref _host, value); }
        }

        private string _email;
        [Required(ErrorMessageResourceType =typeof(Strings), ErrorMessageResourceName = "Login_EmailIsRequired")]
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }
        private bool _Enable2FA;
        public bool Enable2FA
        {
            get { return _Enable2FA; }
            set { SetProperty(ref _Enable2FA, value); }
        }
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Login_PasswordIsRequired")]
        public string Password
        {
            get { return _mediator?.Password; }
            set
            {
                ValidateProperty(value);

                // Do not store the password in a private field as it should not be stored in memory in plain-text.
                // Instead, the supplied PasswordAccessor serves as the backing store for the value.

                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private string _busyContent;
        public string BusyContent
        {
            get { return _busyContent; }
            set { SetProperty(ref _busyContent, value); }
        }

        private DelegateCommand _loginCommand;
        public ICommand LoginCommand
        {
            get { return _loginCommand; }
        }

        private DelegateCommand _forgetPasswordCommand;
        public ICommand ForgetPasswordCommand
        {
            get { return _forgetPasswordCommand; }
        }

        private DelegateCommand _activeAccountCommand;
        public ICommand ActiveAccountCommand
        {
            get { return _activeAccountCommand; }
        }

        private DelegateCommand _signupCommand;
        public ICommand SignupCommand
        {
            get { return _signupCommand; }
        }

        private void OnLogin()
        {
            Validate();

            if (HasErrors)
            {
                return;
            }

            IsBusy = true;
            BusyContent = Strings.Common_Loading;

            var successed = false;
            Task.Run(() =>
            {
                
                    var user = _web.Login(Enable2FA, Host, Email, Password);
                    if (user != null)
                    {
                        successed = true;
                        user.Host = Host;
                        _storage.SaveUser(user, Password);
                    }
              
            }).ContinueWith(task =>
            {
                IsBusy = false;
                BusyContent = null;

                if (successed)
                {
                    _dialog.Close();
                    _messenger.Send("OnLogined");
                }
                else
                {
                    MessageBox.Show(Strings.Login_FailedToLogin);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnForgetPassword()
        {
            _shell.OpenUrl($"{Host}/users/password/new");
        }

        private void OnActiveAccount()
        {
            _shell.OpenUrl($"{Host}/users/confirmation/new");
        }

        private void OnSignup()
        {
            _shell.OpenUrl($"{Host}users/sign_in#register-pane");
        }
    }
}
