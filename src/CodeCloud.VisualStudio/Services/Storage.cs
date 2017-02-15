using CodeCloud.VisualStudio.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace CodeCloud.VisualStudio.Services
{
    [Export(typeof(IStorage))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Storage : IStorage
    {
        private static readonly string _path;
        private User _user;

        static Storage()
        {
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codecloud");
        }

        private bool _isChecked;
        public bool IsLogined
        {
            get
            {
                lock (_path)
                {
                    if (!_isChecked)
                    {
                        LoadUser();

                        _isChecked = true;
                    }
                }

                return _user != null && _user.Token != null;
            }
        }

        public string GetPassword()
        {
            var key = "git:https://gitclub.cn";

            using (var credential = new Credential())
            {
                credential.Target = key;
                return credential.Load()
                    ? credential.Password
                    : null;
            }
        }

        private string GetToken()
        {
            var key = "token:https://gitclub.cn";

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
            if (_user != null)
            {
                return _user;
            }

            LoadUser();

            return _user;
        }

        public void SaveUser(User user, string password)
        {
            SavePassword(user.Email, password);
            SaveToken(user.Email, user.Token);

            SaveUserToLocal(user);

            _user = user;
        }

        private void SaveUserToLocal(User user)
        {
            var serializer = new JsonSerializer();
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
                using (var writer = new JsonTextWriter(new StreamWriter(_path)))
                {
                    writer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, new { User = user });
                }
            }
        }

        private void SavePassword(string email, string password)
        {
            var key = "git:https://gitclub.cn";
            using (var credential = new Credential(email, password, key))
            {
                credential.Save();
            }
        }

        private void SaveToken(string email, string token)
        {
            var key = "token:https://gitclub.cn";
            using (var credential = new Credential(email, token, key))
            {
                credential.Save();
            }
        }

        private void LoadUser()
        {
            if (File.Exists(_path))
            {
                JObject o = null;
                using (var reader = new JsonTextReader(new StreamReader(_path)))
                {
                    var serializer = new JsonSerializer();

                    o = (JObject)serializer.Deserialize(reader);

                    var token = o["User"];
                    if (token != null)
                    {
                        _user = token.ToObject<User>();

                        _user.Token = GetToken();
                    }
                }
            }
        }

        public void Erase()
        {
            _user = null;

            EraseCredential("git:https://gitclub.cn");
            EraseCredential("token:https://gitclub.cn");

            File.Delete(_path);
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
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(user, "Source", "Repos");
        }
    }
}
