using GitLab.VisualStudio.Shared;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IStorage))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Storage : IStorage
    {
 
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
                string url = string.Empty;
                using (var git = new GitAnalysis(GitLabPackage.GetSolutionDirectory()))
                {
                    if (git != null && git.IsDiscoveredGitRepository)
                    {
                        string hurl = git.GetRepoUrlRoot();
                        if (!string.IsNullOrEmpty(hurl))
                        {
                            Uri uri = new Uri(hurl);
                            url = uri.Host;
                        }
                    }
                }
                if (string.IsNullOrEmpty(url))
                {
                    var _u = LoadUser();
                    if (_u != null)
                    {
                        url = _u.Host;
                    }
                }
                return url;
            }
        }
        public string Path
        {
            get
            {
                string slnpath = GitLabPackage.GetSolutionDirectory();
                string _path = string.Empty;
                if (string.IsNullOrEmpty(slnpath))
                {
                    _path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".GitLab");
                }
                else
                {
                      _path = slnpath + "\\.vs\\.gitlab";
                }
                return _path;
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
            SaveToken(user.Host,user.Username, user.PrivateToken);
            SaveUserToLocal(user);
          
        }

        private void SaveUserToLocal(User user)
        {
            var serializer = new JsonSerializer();
            string _path = Path;
            if (File.Exists(_path))
            {
                JObject o = null;
                using (var reader = new JsonTextReader(new StreamReader(_path)))
                {
                    o = (JObject)serializer.Deserialize(reader);

                    o["User"] = JToken.FromObject(user);
                }
                using (var writer = new JsonTextWriter(new StreamWriter(_path)))
                {
                    writer.Formatting = Formatting.Indented;
                    o.WriteTo(writer);
                }
            }
            else
            {
                var fi = new System.IO.FileInfo(_path);
                if (fi.Directory.Exists)
                {
                    fi.Directory.Create();
                  //  fi.Directory.Attributes = FileAttributes.Hidden;
                }
                using (var writer = new JsonTextWriter(new StreamWriter(_path)))
                {
                    writer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, new { User = user });
                }
                //System.IO.File.SetAttributes(_path,   FileAttributes.Hidden);
            }
         
        }

        private void SavePassword(string _host,string username, string password)
        {
            var key = $"git:{_host}";
            using (var credential = new Credential(username, password, key))
            {
                credential.Save();
            }
        }

        private void SaveToken(string _host,string username, string token)
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
                string _path = Path;
                if (!File.Exists(_path))
                {
                   var  tmp_path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".GitLab");
                    if (System.IO.File.Exists(tmp_path))
                    {
                        System.IO.File.Copy(tmp_path, _path);
                    }
                }
                JObject o = null;
                using (var reader = new JsonTextReader(new StreamReader(_path)))
                {
                    var serializer = new JsonSerializer();
                    o = (JObject)serializer.Deserialize(reader);
                    var token = o["User"];
                    _user = token.ToObject<User>();
                    _user.PrivateToken = GetToken(_user.Host);
                }

            }
            catch (Exception ex)
            {

             
            }
            return _user;
        }

        public void Erase()
        {
            EraseCredential($"git:{Host}");
            EraseCredential($"token:{Host}");
        }

        private static void EraseCredential(string key)
        {
            using (var credential = new Credential())
            {
                credential.Target = key;
                credential.Delete();
            }
        }

        public string GetBaseRepositoryDirectory()
        {
            string _path = this.Path;
            if (!System.IO.Directory.Exists(_path))
            {
                var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
              _path=   System.IO.Path.Combine(user, "Source", "Repos");
            }

            return _path;
        }
    }
}
