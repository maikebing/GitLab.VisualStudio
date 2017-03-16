using LibGit2Sharp;
using System;
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
        Commits
    }

    public sealed class GitAnalysis : IDisposable
    {
        readonly Repository repository;
        readonly string targetFullPath;

        public bool IsDiscoveredGitRepository => repository != null;

        public GitAnalysis(string targetFullPath)
        {
            this.targetFullPath = targetFullPath;
            var repositoryPath = LibGit2Sharp.Repository.Discover(targetFullPath);
            if (repositoryPath != null)
            {
                this.repository = new LibGit2Sharp.Repository(repositoryPath);
            }
        }

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
                   default:
                    return "master";
            }
        }

        public string GetGitLabTargetDescription(GitLabUrlType urlType)
        {
            switch (urlType)
            {
                case GitLabUrlType.CurrentBranch:
                    return "Branch: " + repository.Head.FriendlyName.Replace("origin/", "");
                case GitLabUrlType.CurrentRevision:
                    return "Revision: " + repository.Commits.First().Id.ToString(8);
                case GitLabUrlType.CurrentRevisionFull:
                    return "Revision: " + repository.Commits.First().Id.ToString(8) + "... (Full ID)";
                case GitLabUrlType.Blame:
                    return "Blame";
                case GitLabUrlType.Commits:
                    return "Commits";
                case GitLabUrlType.Master:
                default:
                    return "master";
            }
        }

        public string BuildGitLabUrl(GitLabUrlType urlType, Tuple<int, int> selectionLineRange)
        {
            // https://github.com/user/repo.git
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
            var originUrl = repository.Config.Get<string>("remote.origin.url");
            if (originUrl == null) throw new InvalidOperationException("OriginUrl can't found");

            // https://github.com/user/repo
            var urlRoot = (originUrl.Value.EndsWith(".git", StringComparison.InvariantCultureIgnoreCase))
                ? originUrl.Value.Substring(0, originUrl.Value.Length - 4) // remove .git
                : originUrl.Value;

            // git@github.com:user/repo -> http://github.com/user/repo
            urlRoot = Regex.Replace(urlRoot, "^git@(.+):(.+)/(.+)$", match => "http://" + string.Join("/", match.Groups.OfType<Group>().Skip(1).Select(group => group.Value)), RegexOptions.IgnoreCase);

            // https://user@github.com/user/repo -> https://github.com/user/repo
            urlRoot = Regex.Replace(urlRoot, "(?<=^https?://)([^@/]+)@", "");
            return urlRoot;
        }

        void Dispose(bool disposing)
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