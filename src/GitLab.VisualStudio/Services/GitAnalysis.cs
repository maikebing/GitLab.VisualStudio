using GitLab.VisualStudio.Shared;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace GitLab.VisualStudio.Services
{
    public enum GitLabUrlType
    {
        Master,
        CurrentBranch,
        CurrentRevision,
        CurrentRevisionFull,
        Blame,
        Commits,
    }

    public sealed class GitAnalysis : IDisposable
    {
        private readonly LibGit2Sharp.Repository repository;
        private readonly string targetFullPath;

        public bool IsDiscoveredGitRepository => repository != null;

        public GitAnalysis(string targetFullPath)
        {
            this.targetFullPath = targetFullPath;
            var repositoryPath = LibGit2Sharp.Repository.Discover(targetFullPath);
            if (repositoryPath != null)
            {
                this.repository = new LibGit2Sharp.Repository(repositoryPath);
                RepositoryPath = repositoryPath;
            }
        }
        static Dictionary<string, GitAnalysis> dicgit = new Dictionary<string, GitAnalysis>();
        public static GitAnalysis GetBy(string fullpath)
        {
            if (!dicgit.ContainsKey(fullpath))
            {
                dicgit.Add(fullpath, new GitAnalysis(fullpath));
            }
            return dicgit[fullpath];
        }
        public string RepositoryPath { get; private set; }
        public LibGit2Sharp.Repository Repository { get { return repository; } }

        public string GetGitLabTargetPath(GitLabUrlType urlType)
        {
            switch (urlType)
            {
                case GitLabUrlType.CurrentBranch:
                    return repository.Head.FriendlyName.Replace("origin/", "");

                case GitLabUrlType.CurrentRevision:
                    return repository.Commits.First().Id.ToString(8);

                case GitLabUrlType.CurrentRevisionFull:
                    return repository.Commits.First().Id.Sha;

                case GitLabUrlType.Master:
                default:
                    return "master";
            }
        }

        public string GetGitLabTargetDescription(GitLabUrlType urlType)
        {
            switch (urlType)
            {
                case GitLabUrlType.CurrentBranch:
                    return Strings.GitAnalysisn_Branch + repository.Head.FriendlyName.Replace("origin/", "");

                case GitLabUrlType.CurrentRevision:
                    return Strings.GitAnalysis_Revision + repository.Commits.First().Id.ToString(8);

                case GitLabUrlType.CurrentRevisionFull:
                    return Strings.GitAnalysis_Revision + repository.Commits.First().Id.ToString(8) + Strings.GitAnalysis_GetGitLabTargetDescription_FullID;

                case GitLabUrlType.Blame:
                    return Strings.GitAnalysis_Blame;

                case GitLabUrlType.Commits:
                    return Strings.GitAnalysis_Commits;

                case GitLabUrlType.Master:
                default:
                    return "master";
            }
        }

        public string BuildGitLabUrl(GitLabUrlType urlType, Tuple<int, int> selectionLineRange)
        {
            // https://GitLab.com/user/repo.git
            string urlRoot = GetRepoUrlRoot();

            // foo/bar.cs
            var rootDir = repository.Info.WorkingDirectory;
            var fileIndexPath = targetFullPath.Substring(rootDir.Length).Replace("\\", "/");

            var repositoryTarget = GetGitLabTargetPath(urlType);

            // line selection
            var fragment = (selectionLineRange != null)
                                ? (selectionLineRange.Item1 == selectionLineRange.Item2)
                                    ? string.Format("#L{0}", selectionLineRange.Item1)
                                    : string.Format("#L{0}-L{1}", selectionLineRange.Item1, selectionLineRange.Item2)
                                : "";

            var urlshowkind = "blob";
            if (urlType == GitLabUrlType.Blame)
            {
                urlshowkind = "blame";
            }
            if (urlType == GitLabUrlType.Commits)
            {
                urlshowkind = "commits";
            }
            var fileUrl = string.Format("{0}/{4}/{1}/{2}{3}", urlRoot.Trim('/'), WebUtility.UrlEncode(repositoryTarget.Trim('/')), fileIndexPath.Trim('/'), fragment, urlshowkind);

            return fileUrl;
        }

        public string GetRepoUrlRoot()
        {
            string urlRoot = string.Empty;
            var originUrl = repository.Config.Get<string>("remote.origin.url");
            if (originUrl != null)
            {
                // https://GitLab.com/user/repo
                urlRoot = (originUrl.Value.EndsWith(".git", StringComparison.InvariantCultureIgnoreCase))
                  ? originUrl.Value.Substring(0, originUrl.Value.Length - 4) // remove .git
                  : originUrl.Value;

                // git@GitLab.com:user/repo -> http://GitLab.com/user/repo
                urlRoot = Regex.Replace(urlRoot, "^git@(.+):(.+)/(.+)$", match => "http://" + string.Join("/", match.Groups.OfType<Group>().Skip(1).Select(group => group.Value)), RegexOptions.IgnoreCase);

                // https://user@GitLab.com/user/repo -> https://GitLab.com/user/repo
                urlRoot = Regex.Replace(urlRoot, "(?<=^https?://)([^@/]+)@", "");
            }
            return urlRoot;
        }

        public string GetRepoOriginRemoteUrl()
        {
            string urlRoot = string.Empty;
            var originUrl = repository.Config.Get<string>("remote.origin.url");
            if (originUrl != null)
            {
                urlRoot = originUrl.Value;
            }
            return urlRoot;
        }

        private void Dispose(bool disposing)
        {
            if (repository != null)
            {
                repository.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GitAnalysis()
        {
            Dispose(false);
        }
    }
}