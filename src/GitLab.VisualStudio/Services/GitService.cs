

using GitLab.VisualStudio.Shared;
using LibGit2Sharp;
using System;
using System.Collections;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IGitService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GitService : IGitService
    {
        private static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public void FillAccessories(string fullname, string email, string path, string gitignore, string license)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string resourcesName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resourcesName))
            using (var reader = new System.Resources.ResourceReader(stream))
            {
                var entries = reader.Cast<DictionaryEntry>();

                if (!string.IsNullOrEmpty(gitignore))
                {
                    var name = $"resources/gitignores/{gitignore}".Replace(" ", "%20");
                    var ignoresEntry = entries.First(o => string.Equals((string)o.Key, name, StringComparison.OrdinalIgnoreCase));
                    using (var froms = (Stream)ignoresEntry.Value)
                    using (var target = File.Create(Path.Combine(path, ".gitignore")))
                    {
                        froms.CopyTo(target);
                    }
                }

                if (!string.IsNullOrEmpty(license))
                {
                    var name = $"resources/licenses/{license}".Replace(" ", "%20");
                    var licensesEntry = entries.First(o => string.Equals((string)o.Key, name, StringComparison.OrdinalIgnoreCase));
                    using (var froms = new StreamReader((Stream)licensesEntry.Value))
                    using (var target = new StreamWriter(Path.Combine(path, "LICENSE")))
                    {
                        while (!froms.EndOfStream)
                        {
                            var line = froms.ReadLine();
                            line = line.Replace("[year]", DateTime.Now.Year.ToString());
                            line = line.Replace("[fullname]", fullname);
                            line = line.Replace("[email]", email);

                            target.WriteLine(line);
                        }
                    }
                }
            }
        }

        public void PushInitialCommit(string fullname, string email, string password, string url, string gitignore, string license)
        {
            var path = GetTemporaryDirectory();
            Directory.CreateDirectory(path);

            FillAccessories(fullname, email, path, gitignore, license);

            LibGit2Sharp.Repository.Init(path);

            using (var repo = new LibGit2Sharp.Repository(path))
            {
                if (File.Exists(Path.Combine(path, ".gitignore")))
                {
                    LibGit2Sharp.Commands.Stage(repo,".gitignore");
                }

                if (File.Exists(Path.Combine(path, "LICENSE")))
                {
                    LibGit2Sharp.Commands.Stage(repo, "LICENSE");
                }

                // Create the committer's signature and commit
                Signature author = new Signature(fullname, email, DateTime.Now);
                Signature committer = author;

                // Commit to the repository
                Commit commit = repo.Commit("Initial commit", author, committer);

                //var options = new LibGit2Sharp.PushOptions()
                //{
                //    CredentialsProvider = (url, usernameFromUrl, types) =>
                //        new UsernamePasswordCredentials()
                //        {
                //            Username = email,
                //            Password = password
                //        }
                //};
                //repo.Network.Push(repo.Branches["master"], options);
                repo.Network.Remotes.Add("origin", url);
                var remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = email, Password = password };
                repo.Network.Push(remote, @"refs/heads/master", options);
            }

            DeleteDirectory(path);
        }

        public void PushWithLicense(string fullname, string email, string password, string url, string path, string license)
        {

            using (var repo = new LibGit2Sharp.Repository(path))
            {
                if (!string.IsNullOrEmpty(license))
                {
                    FillAccessories(fullname, email, path, null, license);
                    LibGit2Sharp.Commands.Stage(repo, "LICENSE");

                    // Create the committer's signature and commit
                    Signature author = new Signature(fullname, email, DateTime.Now);
                    Signature committer = author;

                    // Commit to the repository
                    Commit commit = repo.Commit($"Use {license}", author, committer);
                }

                repo.Network.Remotes.Add("origin", url);
                var remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = email, Password = password };
                repo.Network.Push(remote, @"refs/heads/master", options);
            }
        }

        public IReadOnlyList<string> GetGitIgnores()
        {
            var result = new List<string>();

            var assembly = Assembly.GetExecutingAssembly();

            string resourcesName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resourcesName))
            using (var reader = new System.Resources.ResourceReader(stream))
            {
                var entries = reader.Cast<DictionaryEntry>();

                var entry = entries.First(o => ((string)o.Key) == "resources/gitignores/index");
                using (var indexreader = new StreamReader((Stream)entry.Value))
                {
                    while (!indexreader.EndOfStream)
                    {
                        var line = indexreader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            result.Add(line);
                        }
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<string> GetLicenses()
        {
            var result = new List<string>();

            var assembly = Assembly.GetExecutingAssembly();

            string resourcesName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resourcesName))
            using (var reader = new System.Resources.ResourceReader(stream))
            {
                var entries = reader.Cast<DictionaryEntry>();

                var entry = entries.First(o => ((string)o.Key) == "resources/licenses/index");
                using (var indexreader = new StreamReader((Stream)entry.Value))
                {
                    while (!indexreader.EndOfStream)
                    {
                        var line = indexreader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            result.Add(line);
                        }
                    }
                }
            }

            return result;
        }

        public string GetRemote(string path)
        {
            try
            {
                var repository = new LibGit2Sharp.Repository(path);
                return repository.Network.Remotes["origin"].Url;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
