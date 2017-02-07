using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Helpers;
using CodeCloud.VisualStudio.Shared.Helpers.Commands;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CodeCloud.VisualStudio.UI.ViewModels
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

        private string _email;
        [Required(ErrorMessage = "邮箱地址为必填")]
        [EmailAddress(ErrorMessage = "不是合法的邮箱地址")]
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        [Required(ErrorMessage = "密码为必填")]
        [MinLength(6, ErrorMessage ="密码至少为6位")]
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
            BusyContent = "登录中...";

            var successed = false;
            Task.Run(() =>
            {
                var user = _web.Login(Email, Password);
                if (user != null)
                {
                    successed = true;

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
                    MessageBox.Show("登录失败");
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnForgetPassword()
        {
            _shell.OpenUrl("https://git.oschina.net/password/new");
        }

        private void OnActiveAccount()
        {
            _shell.OpenUrl("https://git.oschina.net/user/activate");
        }

        private void OnSignup()
        {
            _shell.OpenUrl("https://git.oschina.net/signup");
        }
    }
}
