using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Helpers;
using GitLab.VisualStudio.Shared.Helpers.Commands;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            Host = Strings.DefaultHost;
            _mediator = mediator;
            ApiVersions = new Dictionary<string, string>();
            ApiVersions.Add(Enum.GetName(typeof(ApiVersion), ApiVersion.AutoDiscovery), Strings.AutoDiscovery);
            ApiVersions.Add(Enum.GetName(typeof(ApiVersion), ApiVersion.V4_Oauth), Strings.GitLabApiV4Oauth2);
            ApiVersions.Add(Enum.GetName(typeof(ApiVersion), ApiVersion.V3_Oauth), Strings.GitLabApiV3Oauth2);
            ApiVersions.Add(Enum.GetName(typeof(ApiVersion), ApiVersion.V4), Strings.GitLabApiV4);
            ApiVersions.Add(Enum.GetName(typeof(ApiVersion), ApiVersion.V3), Strings.GitLabApiV3);
            ApiVersions.Add(Enum.GetName(typeof(ApiVersion), ApiVersion.V3_1), Strings.GitLabApiV31);
            SelectedApiVersion = Enum.GetName(typeof(ApiVersion), ApiVersion.AutoDiscovery);
            _loginCommand = new DelegateCommand(OnLogin);
            _forgetPasswordCommand = new DelegateCommand(OnForgetPassword);
            _activeAccountCommand = new DelegateCommand(OnActiveAccount);
            _signupCommand = new DelegateCommand(OnSignup);
        }

        private string _host;

        [Required(ErrorMessageResourceType = typeof(Strings), AllowEmptyStrings = false, ErrorMessageResourceName = "Login_HostIsRequired")]
        public string Host
        {
            get
            {
                return _host;
            }
            set
            {
                try
                {
                    string tmpurl = value;
                    if (value.StartsWith("git@"))
                    {
                        var ary = value.Split('@', ':', '/');
                        tmpurl = $"https://{ary[2]}@{ary[1]}";
                    }
                    var urlhost = new UriBuilder(tmpurl);
                    if (!string.IsNullOrEmpty(urlhost.UserName))
                    {
                        Email = urlhost.UserName;
                        urlhost.UserName = "";
                    }
                    if (!string.IsNullOrEmpty(urlhost.Password))
                    {
                        Password = urlhost.Password;
                        urlhost.Password = "";
                    }
                    SetProperty(ref _host, urlhost.Uri.ToString());
                    var apiver = _storage.GetApiVersion(_host);
                    if (apiver != ApiVersion.AutoDiscovery)
                    {
                        SelectedApiVersion = Enum.GetName(typeof(ApiVersion), apiver);
                    }
                }
                catch (Exception ex)
                {
                    _dialog.Error(ex.Message);
                }
            }
        }

        public IDictionary<string, string> ApiVersions { get; }
        private string _email;

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Login_EmailIsRequired")]
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

        private string _apiversion;

        public string SelectedApiVersion
        {
            get { return _apiversion; }
            set { SetProperty(ref _apiversion, value); }
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
            string logmsg = "";
            Task.Run(() =>
            {
                bool ok = Enum.TryParse(SelectedApiVersion, true, out ApiVersion apiVersion);
                if (ok || apiVersion == ApiVersion.AutoDiscovery)
                {
                    var arys = new ApiVersion[] { ApiVersion.V4_Oauth, ApiVersion.V4, ApiVersion.V3_Oauth, ApiVersion.V3, ApiVersion.V3_1 };
                    foreach (var apiv in arys)
                    {
                        try
                        {
                            BusyContent = Strings.Trying + apiv.ToString();
                            var user = _web.LoginAsync(Enable2FA, Host, Email, Password, apiv);
                            if (user != null)
                            {
                                BusyContent = null;
                                successed = true;
                                user.Host = Host;
                                _storage.AddHostVersionInfo(Host, apiv);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            BusyContent = ex.Message;
                            logmsg += apiv.ToString() + ":" + ex.Message + Environment.NewLine;
                        }
                    }
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
                    _dialog.Warning(Strings.Login_FailedToLogin + Environment.NewLine + Strings.PleaseCheckYourUsernameOrPassword + Environment.NewLine + logmsg);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()).Forget();
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