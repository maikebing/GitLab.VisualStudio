using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Models;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GitLab.VisualStudio.Helpers;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IStorage))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Storage : IStorage
    {
        public Storage()
        {
            LoadConfig();
        }
        public bool IsLogined
        {
            get
            {
                bool islogin = false;
                var _user = LoadUser();
                islogin = _user != null && _user.PrivateToken != null;
                return islogin;
            }
        }

        public string Host
        {
            get
            {
                string url = Strings.DefaultHost;
                var slndir = GitLabPackage.GetSolutionDirectory();
                if (System.IO.Directory.Exists(slndir))
                {
                    using (var git = new GitAnalysis(GitLabPackage.GetSolutionDirectory()))
                    {
                        if (git != null && git.IsDiscoveredGitRepository)
                        {
                            string hurl = git.GetRepoUrlRoot();
                            if (!string.IsNullOrEmpty(hurl))
                            {
                                Uri uri = new Uri(hurl);
                                url = $"{uri.Scheme}://{uri.Host}/";
                            }
                        }
                    }
                }
                return url;
            }
        }

        public string GetPassword(string _host)
        {
            var key = $"git:{_host}";

            using (var credential = new Credential())
            {
                credential.Target = key;
                return credential.Load()
                    ? credential.Password
                    : null;
            }
        }

        private string GetToken(string _host)
        {
            var key = $"token:{_host}";

            using (var credential = new Credential())
            {
                credential.Target = key;
                return credential.Load()
                    ? credential.Password
                    : null;
            }
        }

        public User GetUser()
        {
            return LoadUser();
        }

        public void SaveUser(User user, string password)
        {
            SavePassword(user.Host, user.Username, password);
            if (user.TwoFactorEnabled)
            {
                user.PrivateToken = password;
            }
            SaveToken(user.Host, user.Username, user.PrivateToken);
            SaveUserToLocal(user);
        }

        private void SaveUserToLocal(User user)
        {
            try
            {
                string _path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{new Uri(Host).Host}.gitlab4vs");
                var fi = new System.IO.FileInfo(_path);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                var pt = user.PrivateToken;
                user.PrivateToken = null;
                System.IO.File.WriteAllText(_path, Newtonsoft.Json.JsonConvert.SerializeObject(user));
                user.PrivateToken = pt;
            }
            catch (Exception)
            {

            }
        }

        private void SavePassword(string _host, string username, string password)
        {
            var key = $"git:{_host}";
            using (var credential = new Credential(username, password, key))
            {
                credential.Save();
            }
        }

        private void SaveToken(string _host, string username, string token)
        {
            var key = $"token:{_host}";
            using (var credential = new Credential(username, token, key))
            {
                credential.Save();
            }
        }

        private User LoadUser()
        {
            User _user = null;
            try
            {
                var _path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{new Uri(Host).Host}.gitlab4vs");
                if (!System.IO.File.Exists(_path))
                {
                    _path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{new Uri(Strings.DefaultHost).Host}.gitlab4vs");
                }

                _user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(System.IO.File.ReadAllText(_path));
                _user.PrivateToken = GetToken(_user.Host);
            }
            catch (Exception ex)
            {
            }
            return _user;
        }

        public void Erase()
        {
            try
            {
                List<FileInfo> files = new List<FileInfo>();
                files.Add(new FileInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{new Uri(Host).Host}.gitlab4vs")));
                files.Add(new FileInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{new Uri(Strings.DefaultHost).Host}.gitlab4vs")));
                files.Add(new FileInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "gitlab4vs.cfg")));
                files.ForEach(f =>
                {
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception)
                    {
                    }
                });
                EraseCredential($"git:{Host}");
                EraseCredential($"token:{Host}");
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WarningWriteLine("Erase"+ex.Message);
            }
        }

        private static void EraseCredential(string key)
        {
            using (var credential = new Credential())
            {
                credential.Target = key;
                credential.Delete();
            }
        }

        public string GetBaseRepositoryDirectory() => AppSettings?.BasePath;


        public AppSettings AppSettings { get; set; }
        public void LoadConfig()
        {
            try
            {
                var filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "gitlab4vs.cfg");
                if (System.IO.File.Exists(filename))
                {
                    AppSettings = JsonConvert.DeserializeObject<AppSettings>(System.IO.File.ReadAllText(filename));
                }
            }
            catch (Exception)
            {
            }
            if (AppSettings==null)
            {
                AppSettings = new AppSettings();
            }
            if (string.IsNullOrEmpty(AppSettings.BasePath))
            {
                var user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                AppSettings.BasePath = System.IO.Path.Combine(user, "Source", "Repos");
            }
        }
        public void SaveConfig()
        {
            try
            {
                var filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "gitlab4vs.cfg");
                System.IO.File.WriteAllText(filename, JsonConvert.SerializeObject(AppSettings));
            }
            catch (Exception)
            {
                OutputWindowHelper.WarningWriteLine("Can't save config!");
            }
        }

        private Dictionary<string, ApiVersion> HostVersionInfo { get; set; }

        public bool HaveHost(string host)
        {
            bool result = false;
            if (HostVersionInfo == null)
            {
                LoadHostVersionInfo();
            }
            result = HostVersionInfo.ContainsKey(host);

            if (!result && Uri.TryCreate(host, UriKind.Absolute, out Uri uri))
            {
                result = HostVersionInfo.ContainsKey(uri.Host);
            }

            return result;
        }

        public ApiVersion GetApiVersion(string host)
        {
            ApiVersion apiVersion = ApiVersion.AutoDiscovery;
            if (HostVersionInfo == null)
            {
                LoadHostVersionInfo();
            }
            if (Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var uri))
            {
                if (HostVersionInfo.ContainsKey(uri.Host))
                {
                    apiVersion = HostVersionInfo[uri.Host];
                }
            }
            return apiVersion;
        }

        public void LoadHostVersionInfo()
        {
            try
            {
                var filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "gitlab4vs.dat");
                if (System.IO.File.Exists(filename))
                {
                    HostVersionInfo = JsonConvert.DeserializeObject<Dictionary<string, ApiVersion>>(System.IO.File.ReadAllText(filename));
                }
            }
            catch (Exception ex)
            {
                HostVersionInfo = new Dictionary<string, ApiVersion>();
                OutputWindowHelper.WarningWriteLine("Can't load host version infos");
            }
            if (HostVersionInfo == null) HostVersionInfo = new Dictionary<string, ApiVersion>();
            if (HostVersionInfo.Count == 0)
            {
                HostVersionInfo.Add("gitlab.com", ApiVersion.V4_Oauth);
                HostVersionInfo.Add("gitclub.cn", ApiVersion.V4_Oauth);
                HostVersionInfo.Add("gitee.com", ApiVersion.V3_1);
                SaveHostVersion();
            }
        }

        public void AddHostVersionInfo(string host, ApiVersion apiVersion)
        {
            try
            {
                if (Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var uri))
                {
                    if (HostVersionInfo.ContainsKey(uri.Host))
                    {
                        HostVersionInfo.Remove(uri.Host);
                    }
                    HostVersionInfo.Add(uri.Host, apiVersion);
                    SaveHostVersion();
                }
                else
                {
                    OutputWindowHelper.WarningWriteLine($"Can't Create Uri Host:{host},ApiVersion:{apiVersion}");
                }
            }
            catch (Exception ex)
            {
                OutputWindowHelper.ExceptionWriteLine("AddHostVersionInfo", ex);
            }
        }

        private void SaveHostVersion()
        {
            try
            {
                var filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "gitlab4vs.dat");
                System.IO.File.WriteAllText(filename, JsonConvert.SerializeObject(HostVersionInfo));
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WarningWriteLine("Can't save host version info");
            }
        }
    }
}